//using PluginContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Rox
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Dictionary<string, AlarmWindow> alarms = new Dictionary<string, AlarmWindow>();
    private IoAdams IoAdams; // = new IoAdams(new IoAdams.Settings() { IpAddress = "172.18.3.231", Port = 502, ProtocolType = System.Net.Sockets.ProtocolType.Tcp });
    private KeyenceEip keyEip;
    //private ICollection<IPluginContract> plugins;
    private class SequenceEventArgs
    {
      public bool AbortIteration { get; set; }
    }
    private bool highlight;
    private System.Threading.Tasks.Task Seq;
    BindingList<Variable> Vars = new BindingList<Variable>();
    IEnumerable<Variable> LiveVars = null;
    private static SolidColorBrush treeBackground = new SolidColorBrush(Color.FromRgb(41, 41, 41));
    private static SolidColorBrush treeBackgroundAllowDrop = new SolidColorBrush(Color.FromRgb(71, 125, 30));
    private static SolidColorBrush treeBackgroundAllowDropInSeq = new SolidColorBrush(Colors.LightBlue);
    private static SolidColorBrush textboxBackground = new SolidColorBrush(Color.FromRgb(241, 241, 241));
    //private static SolidColorBrush processedNodeBackground = new SolidColorBrush(Color.FromRgb(0, 107, 21));
    private static SolidColorBrush errorNodeBackground = new SolidColorBrush(Colors.Red);
    private static SolidColorBrush processedNodeBackground = new SolidColorBrush(Color.FromRgb(88, 94, 45));
    private static SolidColorBrush unprocessedNodeBackground = new SolidColorBrush(Colors.Transparent);
    private static SolidColorBrush trueNodeBackground = new SolidColorBrush(Color.FromRgb(0, 107, 21));
    private static SolidColorBrush falseNodeBackground = new SolidColorBrush(Color.FromRgb(107, 0, 66));
    //private static SolidColorBrush textboxBackgroundAllowDrop = new SolidColorBrush(Color.FromRgb(71, 125, 30));
    public static List<NodeTypes> SequenceNodes = new List<NodeTypes>() { NodeTypes.Condition, NodeTypes.General, NodeTypes.Timer, NodeTypes.SetVariable, NodeTypes.SetMode, NodeTypes.Return, NodeTypes.Alarm };
    public List<IteNodeViewModel> Modes;// : INotifyPropertyChanged;
    public List<string> AvailModes { get { return Modes.Where(p => p.NodeType == NodeTypes.Mode).Select(p => p.Name).ToList(); } }
    public IteNodeViewModel selectedNode { get; set; }
    public bool LoggedIn { get; set; }
    //public TimeSpan delayToLogin { get; set; }
    public long delayToLoginAsTicks { get; set; }
    System.Threading.Timer closeMnu;
    System.Threading.Timer clrHeader;
    System.Threading.Timer tmrLogout;
    public string processingMode { get; set; } = null; // initialize processingMode and curMode to different values so initialize sequence will run on startup
    public string _curMode;
    public string curMode
    {
      get { return _curMode; }
      set
      {
        if (_curMode != value)
        {
          _curMode = value;
          if (value == "Auto") { Dispatcher.Invoke(() => { Running = true; }); }
          else if (value == "Stop") { Dispatcher.Invoke(() => { Running = false; }); }
        }
      }
    }
    private bool? _running;
    public bool? Running
    {
      get { return _running; }
      private set
      {
        if (!value.HasValue)
        {
          // no file
          btnToggleRun.Content = txtblkRunDefault;
          brdrRun.Background = new SolidColorBrush(Color.FromRgb(41, 41, 41));
          btnToggleRun.BorderBrush = new SolidColorBrush(Color.FromRgb(41, 41, 41));
          btnToggleRun.Foreground = new SolidColorBrush(Colors.White);
        }
        else if (value.Value)
        {
          // running
          btnToggleRun.Content = "STOP";
          brdrRun.Background = new SolidColorBrush(Colors.OrangeRed);
          btnToggleRun.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
          btnToggleRun.Foreground = new SolidColorBrush(Colors.Red);
          curMode = "Auto";
        }
        else
        {
          // stopped
          btnToggleRun.Content = "START";
          brdrRun.Background = new SolidColorBrush(Colors.LightGreen);
          btnToggleRun.BorderBrush = new SolidColorBrush(Colors.LightGreen);
          btnToggleRun.Foreground = new SolidColorBrush(Colors.LawnGreen);
          curMode = "Stop";
        }
        _running = value;
      }
    }
    private bool _paused = false;
    public bool Paused
    {
      get { return _paused; }
      set
      {
        if (_paused != value)
        {
          _paused = value;
          if (value)
          {
            imgSeq.Source = new BitmapImage(new Uri(@"pack://application:,,,/include/Continuous.png", UriKind.Absolute));
            btnAbort.BorderBrush = new SolidColorBrush(Colors.LightGreen);
          }
          else
          {
            processingMode = null;
            Seq = Task.Run(() => { RunSequence(); });
            imgSeq.Source = new BitmapImage(new Uri(@"pack://application:,,,/include/Stop.png", UriKind.Absolute));
            btnAbort.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
          }
        }
      }
    }
    string loadedFile;
    string fileToBeLoaded;
    Point startDragPoint;
    public MainWindow()
    {
      InitializeComponent();
      //plugins = LoadPlugins(@"Plugins\");
    }
    //public ICollection<IPluginContract> LoadPlugins(string path)
    //{
    //  string[] dllFileNames = null;
    //  if (System.IO.Directory.Exists(path))
    //  {
    //    dllFileNames = System.IO.Directory.GetFiles(path, "*.dll");
    //    ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
    //    foreach (string dllFile in dllFileNames)
    //    {
    //      AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
    //      Assembly assembly = Assembly.Load(an);
    //      assemblies.Add(assembly);
    //    }
    //    Type pluginType = typeof(IPluginContract);
    //    ICollection<Type> pluginTypes = new List<Type>();
    //    foreach (Assembly assembly in assemblies)
    //    {
    //      if (assembly != null)
    //      {
    //        try
    //        {
    //          Type[] types = assembly.GetTypes();
    //          foreach (Type type in types)
    //          {
    //            if (type.IsInterface || type.IsAbstract)
    //            {
    //              continue;
    //            }
    //            else
    //            {
    //              if (type.GetInterface(pluginType.FullName) != null)
    //              {
    //                pluginTypes.Add(type);
    //              }
    //            }
    //          }
    //        }
    //        catch (Exception)
    //        {
    //        }
    //      }
    //    }
    //    ICollection<IPluginContract> plugins = new List<IPluginContract>(pluginTypes.Count);
    //    foreach (Type type in pluginTypes)
    //    {
    //      IPluginContract plugin = (IPluginContract)Activator.CreateInstance(type);
    //      plugins.Add(plugin);
    //    }
    //    return plugins;
    //  }
    //  return null;
    //}
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      //App.splashScreen.LoadComplete();
    }
    private void Window_Initialized(object sender, EventArgs e)
    {
      ApplySettings();
      SetLoginDelay();
      txtVer.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      closeMnu = new System.Threading.Timer(new System.Threading.TimerCallback(closeMenu), null, 10000, System.Threading.Timeout.Infinite);
      togglePlugins(null, null);
      PopulateFilelists();
      resetForm(true);
      listVars.ItemsSource = Vars;
      Paused = true;
      Logout(null);
      //IoAdams = new IoAdams(new IoAdams.ConnectionSettings() { IpAddress = "172.18.3.231", Port = 502, ProtocolType = System.Net.Sockets.ProtocolType.Tcp });
      //UpdateHeader(string.Format("IO module is {0}", IoAdams.IsConnected ? "connected." : string.Format("not connected. Last attempt at {0}.", (IoAdams.LastFailedReconnectTime ?? DateTime.Now).ToShortTimeString())));
    }
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      try
      {
        ClearAlarms();
        Paused = true; // redundant, yes
        FileUnload(false);
        if (keyEip != null) { keyEip.Disconnect(); keyEip = null; }
        SaveSettings();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }
    private void resetCloseMenuTimer(int interval = 10000)
    {
      closeMnu.Change(interval, System.Threading.Timeout.Infinite);
    }
    private void _MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left) { this.DragMove(); }
      if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
      {
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
      }
    }
    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
      this.WindowState = WindowState.Minimized;
    }
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
    private void ApplySettings()
    {
      if (Properties.Settings.Default.UpgradeRequired)
      {
        Properties.Settings.Default.Upgrade();
        Properties.Settings.Default.UpgradeRequired = false;
        Properties.Settings.Default.Save();
      }
      this.WindowState = Properties.Settings.Default.LastWindowState;
      this.Width = Properties.Settings.Default.LastWindowRect.Width;
      this.Height = Properties.Settings.Default.LastWindowRect.Height;
      this.Top = Properties.Settings.Default.LastWindowRect.Top;
      this.Left = Properties.Settings.Default.LastWindowRect.Left;
      Properties.Settings.Default.ProgramPath = string.IsNullOrWhiteSpace(Properties.Settings.Default.ProgramPath) ? System.AppDomain.CurrentDomain.BaseDirectory : Properties.Settings.Default.ProgramPath;
      Properties.Settings.Default.MruFiles = Properties.Settings.Default.MruFiles ?? new System.Collections.Specialized.StringCollection();
      colTree.Width = new GridLength(Properties.Settings.Default.TreePanelWidth);
      colOptions.Width = new GridLength(Properties.Settings.Default.OptionPanelWidth);
      if (listVars.View is GridView gv)
      {
        gv.Columns[0].Width = Properties.Settings.Default.VarColNameWidth;
        gv.Columns[1].Width = Properties.Settings.Default.VarColValueWidth;
        gv.Columns[2].Width = Properties.Settings.Default.VarColNoteWidth;
        gv.Columns[3].Width = Properties.Settings.Default.VarColTypeWidth;
      }
    }
    private void SaveSettings()
    {
      if (delayToLoginAsTicks - DateTime.Now.Ticks <= 0) { Properties.Settings.Default.FailedLogin = 0; }
      Properties.Settings.Default.LastWindowState = this.WindowState;
      Properties.Settings.Default.LastWindowRect = this.RestoreBounds;
      //Properties.Settings.Default.ProgramPath = System.AppDomain.CurrentDomain.BaseDirectory;
      var a = Properties.Settings.Default.MruFiles.Cast<string>().Distinct().ToArray();
      Properties.Settings.Default.MruFiles.Clear();
      Properties.Settings.Default.MruFiles.AddRange(a);
      Properties.Settings.Default.TreePanelWidth = colTree.Width.Value;
      Properties.Settings.Default.OptionPanelWidth = colOptions.Width.Value;

      if (listVars.View is GridView gv)
      {
        Properties.Settings.Default.VarColNameWidth = gv.Columns[0].Width;
        Properties.Settings.Default.VarColValueWidth = gv.Columns[1].Width;
        Properties.Settings.Default.VarColNoteWidth = gv.Columns[2].Width;
        Properties.Settings.Default.VarColTypeWidth = gv.Columns[3].Width;
      }
      Properties.Settings.Default.Save();
    }
    private void toggleMenu(object sender, RoutedEventArgs e)
    {
      if (gridMenu.Visibility == Visibility.Visible)
      {
        closeFiles();
        gridMenu.Visibility = Visibility.Collapsed;
      }
      else
      {
        gridMenu.Visibility = Visibility.Visible;
        //closeMnu = new System.Threading.Timer(new System.Threading.TimerCallback(closeMenu), null, 10000, System.Threading.Timeout.Infinite);
      }
    }
    private void closeFiles()
    {
      Dispatcher.Invoke(() =>
      {
        //gridMenu.Visibility = Visibility.Collapsed;
        gridFiles.Visibility = Visibility.Collapsed;
        //btnLoadFileText.Text = "select file";
      });
    }
    private void closeMenu(object state)
    {
      closeFiles();
    }
    private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      resetCloseMenuTimer();
      if (((ListBox)sender).SelectedItem == null) { return; }

      //if (((ListBox)sender).SelectedItem.ToString() == "> unload <")
      //{
      //  // unload file
      //  btnLoadFileText.Text = "select file";
      //  FileUnload(false);
      //  fileToBeLoaded = null;
      //}
      //else
      //{
      fileToBeLoaded = (string)((ListBox)sender).SelectedItem;
      btnLoadFileText.Text = string.Format("load [{0}]", fileToBeLoaded);
      //}
    }
    private void Files_GotFocus(object sender, RoutedEventArgs e)
    {
      if (((ListBox)sender).SelectedItem == null) { return; }
      Files_SelectionChanged(sender, null);
    }
    private void Files_LostFocus(object sender, RoutedEventArgs e)
    {
      ((ListBox)sender).SelectedItem = null;
    }
    private void btnSaveAs_Click(object sender, RoutedEventArgs e)
    {
      resetCloseMenuTimer();
      var d = new FileSavePrompt(Properties.Settings.Default.ProgramPath, null) { Owner = this };
      if (d.ShowDialog() == true)
      {
        if (!string.IsNullOrWhiteSpace(d.Filename))
        {
          SaveFile(Properties.Settings.Default.ProgramPath.TrimEnd(new char[] { '\\' }) + "\\" + d.Filename, true);
          Properties.Settings.Default.MruFiles.Insert(0, d.Filename.Substring(0, d.Filename.IndexOf(".rox")));
          PopulateFilelists();
        }
        //    if (SharedMethods.SaveJobAs(pluginsCore, d.Filename))
        //    {
        //        FileLoad(d.Filename);
        //        PopulateFilelist();
        //    }
      }
    }
    private string xmlTag_App { get { return "rox"; } }
    private string xmlTag_IO { get { return "adamsIO"; } }
    private string xmlTag_KeyenceEip { get { return "keyEip"; } }
    private string xmlTag_Var { get { return "var"; } }
    private string xmlTag_Mode { get { return "MX"; } }
    private string xmlTag_Init { get { return "init"; } }
    private string xmlTag_Continuous { get { return "cont"; } }
    private string xmlTag_Condition { get { return "condition"; } }
    private string xmlTag_ConditionTrue1 { get { return "cy1"; } }
    private string xmlTag_ConditionTrue { get { return "cy"; } }
    private string xmlTag_ConditionFalse1 { get { return "cn1"; } }
    private string xmlTag_ConditionFalse { get { return "cn"; } }
    private string xmlTag_Timer { get { return "time"; } }
    private string xmlTag_SetVar { get { return "set"; } }
    private string xmlTag_SetMode { get { return "mode"; } }
    private string xmlTag_Return { get { return "ret"; } }
    private string xmlTag_Alarm { get { return "alarm"; } }
    private void FileLoad(string filename)
    {
      Paused = true;
      resetCloseMenuTimer();
      FileUnload(false);
      var filespec = Properties.Settings.Default.ProgramPath.TrimEnd('\\') + @"\" + filename + ".rox";
      if (System.IO.File.Exists(filespec))
      {
        var fileRead = new List<IteNodeViewModel>();
        Vars.Clear();
        using (XmlReader reader = XmlReader.Create(filespec))
        {
          ParseFile(reader, fileRead);
        }
        AssignLiveVars();
        Modes = fileRead;
        tree.DataContext = null;
        tree.DataContext = new { Modes };
        ValidateSequenceVariableTypes();
        btnLoadFileText.Text = string.Format("Save [{0}]", filename);
        Properties.Settings.Default.MruFiles.Insert(0, filename); PopulateRecentFilelist();
        loadedFile = filename;
        if (IoAdams != null) { IoAdams.Connect(); }
        if (keyEip != null) { keyEip.Connect(); }
        UpdateHeader(string.Format("{0} loaded.", filename));
        btnUnloadFile.Visibility = Visibility.Visible;
        Paused = false;
        Running = false;
        processingMode = null;
        //curMode = string.Empty;
      }
      else
      {
        MessageBox.Show("File not found.");
        Properties.Settings.Default.MruFiles.Remove(filename);
        listRecentFiles.Items.Remove(filename);
        //listFiles.Items.Remove(filename);
      }
    }
    public bool ParseFile(System.Xml.XmlReader reader, List<IteNodeViewModel> modes)
    {
      if (reader == null || modes == null) { return false; }
      reader.ReadToFollowing(xmlTag_App);  // get to the first relevant node
      IteNodeViewModel curNode = null;
      string temp;
      while (reader.Read())
      {
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == xmlTag_IO)
          {
            IoAdams = new IoAdams(new IoAdams.ConnectionSettings()
            {
              IpAddress = reader.GetAttribute("ip"),
              Port = int.TryParse(reader.GetAttribute("p"), out var i) ? i : 502,
              ProtocolType = (System.Net.Sockets.ProtocolType)(int.TryParse(reader.GetAttribute("t"), out i) ? (Rox.ProtocolTypes)i : Rox.ProtocolTypes.Tcp),
              Unit = int.TryParse(reader.GetAttribute("u"), out i) ? (SupportedAdvantechUnits)i : SupportedAdvantechUnits.Adam6000
            })
            { Enabled = true };
          }
          else if (reader.Name == xmlTag_KeyenceEip)
          {
            keyEip = new KeyenceEip() { Enabled = true };
            keyEip.Settings.IpAddress = reader.GetAttribute("ip");
            keyEip.Settings.Port = int.TryParse(reader.GetAttribute("p"), out var i) ? i : 44818;
            keyEip.Settings.AssemblyIn = byte.TryParse(reader.GetAttribute("ain"), out var b) ? b : (byte)100;
            keyEip.Settings.AssemblyOut = byte.TryParse(reader.GetAttribute("aout"), out b) ? b : (byte)101;
          }
          else if (reader.Name == xmlTag_Var)
          {
            var v = new Variable() { Name = reader.GetAttribute("name"), Note = reader.GetAttribute("note") };
            decimal d;
            switch (GetVarTypeFromString(reader.GetAttribute("type")))
            {
              case VarType.boolType:
                v.Value = bool.TryParse(reader.GetAttribute("val"), out bool b) ? b : false;
                break;
              case VarType.stringType:
                v.Value = reader.GetAttribute("val");
                break;
              case VarType.numberType:
                v.Value = decimal.TryParse(reader.GetAttribute("val"), out d) ? d : 0;
                break;
              default:
                break;
            }
            v.UsersLastValue = v.Value;
            temp = reader.GetAttribute("io");
            v.IsOutput = temp == "1" ? true : temp == "0" ? (bool?)false : null;
            v.Channel = decimal.TryParse(reader.GetAttribute("i"), out d) ? d : -1;
            v.IoController = (IoControllers)(int.TryParse(reader.GetAttribute("ioc"), out int i) ? i : 0);
            Vars.Add(v);
          }
          // ----------------------------------------------------- MODE
          else if (reader.Name == xmlTag_Mode)
          {
            var name = reader.GetAttribute("name");
            curNode = new IteMODE_VM(new IteMode(name)) { IsExpanded = reader.GetAttribute("exp") != "False" }; // { Items = { new IteIntialize("Initialize"), new IteContinuous("Continuous") } }
            if (name == "Stop" || name == "Auto")
            {
              curNode.IsLocked = true;
            }
            modes.Add(curNode);
          }
          // ----------------------------------------------------- INITIALIZE
          else if (reader.Name == xmlTag_Init)
          {
            if (curNode == null)
            {
              curNode = new IteFIRST_VM(new IteFirstScan("1st scan")) { IsLocked = true, IsExpanded = reader.GetAttribute("exp") != "False" };
              modes.Add(curNode);
            }
            else
            {
              var subNode = new IteFIRST_VM(new IteIntialize(reader.GetAttribute("name"))) { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONTINUOUS
          else if (reader.Name == xmlTag_Continuous)
          {
            if (curNode == null)
            {
              curNode = new IteCONTINUOUS_VM(new IteContinuous("Always")) { IsLocked = true, IsExpanded = reader.GetAttribute("exp") != "False" };
              modes.Add(curNode);
            }
            else
            {
              var subNode = new IteCONTINUOUS_VM(new IteContinuous(reader.GetAttribute("name"))) { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITION
          else if (reader.Name == xmlTag_Condition)
          {
            if (curNode != null)
            {
              var subNode = new IteCONDITION_VM(new IteCondition(reader.GetAttribute("name"))
              {
                VariableName = reader.GetAttribute("varname"),
                DesiredValue = reader.GetAttribute("desired"),
                EvalMethodText = reader.GetAttribute("method"),
                EvalMethod = GetEvalMethodFromString(reader.GetAttribute("method"))
              })
              { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL TRUE1
          else if (reader.Name == xmlTag_ConditionTrue1)
          {
            if (curNode != null)
            {
              var subNode = new IteTRUE1_VM(new IteTrue1(reader.GetAttribute("name"))) { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL TRUE
          else if (reader.Name == xmlTag_ConditionTrue)
          {
            if (curNode != null)
            {
              var subNode = new IteTRUE_VM(new IteTrue(reader.GetAttribute("name"))) { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL FALSE1
          else if (reader.Name == xmlTag_ConditionFalse1)
          {
            if (curNode != null)
            {
              var subNode = new IteFALSE1_VM(new IteFalse1(reader.GetAttribute("name"))) { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL FALSE
          else if (reader.Name == xmlTag_ConditionFalse)
          {
            if (curNode != null)
            {
              var subNode = new IteFALSE_VM(new IteFalse(reader.GetAttribute("name"))) { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- TIMER
          else if (reader.Name == xmlTag_Timer)
          {
            if (curNode != null)
            {
              var subNode = new IteTIMER_VM(new IteTimer(reader.GetAttribute("name"))
              {
                Interval = double.TryParse(reader.GetAttribute("i"), out double d) ? d : 0
              })
              { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- SET VARIABLE
          else if (reader.Name == xmlTag_SetVar)
          {
            if (curNode != null)
            {
              var subNode = new IteSETVAR_VM(new IteSetVar(reader.GetAttribute("name"))
              {
                VariableName = reader.GetAttribute("varname"),
                AssignMethod = reader.GetAttribute("method") == "4" ? AssignMethod.invert : reader.GetAttribute("method") == "3" ? AssignMethod.decrement : reader.GetAttribute("method") == "2" ? AssignMethod.increment : AssignMethod.assign
              })
              { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              switch (GetVarTypeFromString(reader.GetAttribute("type")))
              {
                case VarType.boolType:
                  ((IteSetVar)subNode.Node).Value = bool.TryParse(reader.GetAttribute("val"), out bool b) ? b : false;
                  if (reader.GetAttribute("other") == string.Empty)
                  {
                    ((IteSetVar)subNode.Node).OtherwiseValue = null;
                  }
                  else
                  {
                    ((IteSetVar)subNode.Node).OtherwiseValue = bool.TryParse(reader.GetAttribute("other"), out b) ? b : false;
                  }
                  break;
                case VarType.stringType:
                  ((IteSetVar)subNode.Node).Value = reader.GetAttribute("val");
                  if (reader.GetAttribute("other") == string.Empty)
                  {
                    ((IteSetVar)subNode.Node).OtherwiseValue = null;
                  }
                  else
                  {
                    ((IteSetVar)subNode.Node).OtherwiseValue = reader.GetAttribute("other");
                  }
                  break;
                case VarType.numberType:
                  ((IteSetVar)subNode.Node).Value = decimal.TryParse(reader.GetAttribute("val"), out decimal d) ? d : 0;
                  if (reader.GetAttribute("other") == string.Empty)
                  {
                    ((IteSetVar)subNode.Node).OtherwiseValue = null;
                  }
                  else
                  {
                    ((IteSetVar)subNode.Node).OtherwiseValue = decimal.TryParse(reader.GetAttribute("other"), out d) ? d : 0;
                  }
                  break;
                default:
                  break;
              }

              curNode.Items.Add(subNode);
              //curNode = subNode; CANNOT CONTAIN SUBS
            }
          }
          // ----------------------------------------------------- SET MODE
          else if (reader.Name == xmlTag_SetMode)
          {
            if (curNode != null)
            {
              var subNode = new IteSETMODE_VM(new IteSetMode(reader.GetAttribute("name"))
              {
                ModeName = reader.GetAttribute("mode"),
              })
              { Parent = curNode, IsExpanded = reader.GetAttribute("exp") != "False" };
              curNode.Items.Add(subNode);
              //curNode = subNode; CANNOT CONTAIN SUBS
            }
          }
          // ----------------------------------------------------- RETURN
          else if (reader.Name == xmlTag_Return)
          {
            if (curNode != null)
            {
              var subNode = new IteRETURN_VM(new IteReturn(reader.GetAttribute("name")) { })
              { Parent = curNode };
              curNode.Items.Add(subNode);
              //curNode = subNode; CANNOT CONTAIN SUBS
            }
          }
          // ----------------------------------------------------- ALARM
          else if (reader.Name == xmlTag_Alarm)
          {
            if (curNode != null)
            {
              //sw.Write("\n<{0} name='{1}' title='{2}' prompt='{3}' c1='{4}' c2='{5}' varname='{6}' val='{7}'/>", xmlTag_Alarm, a.Name, a.Title, a.Prompt, a.Color1, a.Color2, a.VariableName, a.Value);
              var subNode = new IteALARM_VM(new IteAlarm(reader.GetAttribute("name"))
              {
                Title = reader.GetAttribute("title"),
                Prompt = reader.GetAttribute("prompt"),
                Color1 = reader.GetAttribute("c1"),
                Color2 = reader.GetAttribute("c2"),
                VariableNameOnOkClick = reader.GetAttribute("varname"),
                OkValue = reader.GetAttribute("val"),
                VariableNameOnCancelClick = reader.GetAttribute("altvarname"),
                CancelValue = reader.GetAttribute("altval"),
              })
              { Parent = curNode };
              curNode.Items.Add(subNode);
              //curNode = subNode; CANNOT CONTAIN SUBS
            }
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (curNode != null) { curNode = curNode.Parent; }
        }
      }


      return true;
    }
    private void ValidateSequenceVariableTypes()
    {
      foreach (var mode in Modes)
      {
        ValidateSequenceVariableTypesForNode(mode);
      }
    }
    private void ValidateSequenceVariableTypesForNode(IteNodeViewModel node)
    {
      var t = node.GetType();
      if (t == typeof(IteALARM_VM))
      {
        var a = Vars.Where(p => p.Name == ((IteAlarm)node.Node).VariableNameOnOkClick);
        if (a.Any())
        {
          switch (a.First().VarType.enumValue)
          {
            case VarType.boolType:
              ((IteAlarm)node.Node).OkValue = bool.TryParse(((IteAlarm)node.Node).OkValue, out bool b) ? b : false;
              break;
            case VarType.stringType:
              ((IteAlarm)node.Node).OkValue = ((IteAlarm)node.Node).OkValue.ToString();
              break;
            case VarType.numberType:
              ((IteAlarm)node.Node).OkValue = decimal.TryParse(((IteAlarm)node.Node).OkValue, out decimal d) ? d : 0;
              break;
            default:
              break;
          }
        }
        a = Vars.Where(p => p.Name == ((IteAlarm)node.Node).VariableNameOnCancelClick);
        if (a.Any())
        {
          switch (a.First().VarType.enumValue)
          {
            case VarType.boolType:
              ((IteAlarm)node.Node).CancelValue = bool.TryParse(((IteAlarm)node.Node).CancelValue, out bool b) ? b : false;
              break;
            case VarType.stringType:
              ((IteAlarm)node.Node).CancelValue = ((IteAlarm)node.Node).CancelValue.ToString();
              break;
            case VarType.numberType:
              ((IteAlarm)node.Node).CancelValue = decimal.TryParse(((IteAlarm)node.Node).CancelValue, out decimal d) ? d : 0;
              break;
            default:
              break;
          }
        }
      }
      else if (t == typeof(IteCONDITION_VM))
      {
        var a = Vars.Where(p => p.Name == ((IteCondition)node.Node).VariableName);
        if (a.Any())
        {
          switch (a.First().VarType.enumValue)
          {
            case VarType.boolType:
              ((IteCondition)node.Node).DesiredValue = bool.TryParse(((IteCondition)node.Node).DesiredValue, out bool b) ? b : false;
              break;
            case VarType.stringType:
              ((IteCondition)node.Node).DesiredValue = ((IteCondition)node.Node).DesiredValue.ToString();
              break;
            case VarType.numberType:
              ((IteCondition)node.Node).DesiredValue = decimal.TryParse(((IteCondition)node.Node).DesiredValue, out decimal d) ? d : 0;
              break;
            default:
              break;
          }
        }
      }
      else if (t == typeof(IteSETVAR_VM))
      {
        var a = Vars.Where(p => p.Name == ((IteSetVar)node.Node).VariableName);
        if (a.Any())
        {
          ((IteSetVar)node.Node).VarType = a.First().VarType;
          if (((IteSetVar)node.Node).Value != null && ((IteSetVar)node.Node).Value.GetType() != a.First().Value.GetType())
          {
            switch (a.First().VarType.enumValue)
            {
              case VarType.boolType:
                ((IteSetVar)node.Node).Value = bool.TryParse(((IteSetVar)node.Node).Value, out bool b) ? b : false;
                break;
              case VarType.stringType:
                ((IteSetVar)node.Node).Value = ((IteSetVar)node.Node).Value.ToString();
                break;
              case VarType.numberType:
                ((IteSetVar)node.Node).Value = decimal.TryParse(((IteSetVar)node.Node).Value, out decimal d) ? d : 0;
                break;
              default:
                break;
            }
          }
          if (((IteSetVar)node.Node).OtherwiseValue != null && ((IteSetVar)node.Node).OtherwiseValue.GetType() != a.First().Value.GetType())
          {
            switch (a.First().VarType.enumValue)
            {
              case VarType.boolType:
                ((IteSetVar)node.Node).OtherwiseValue = bool.TryParse(((IteSetVar)node.Node).OtherwiseValue, out bool b) ? b : false;
                break;
              case VarType.stringType:
                ((IteSetVar)node.Node).OtherwiseValue = ((IteSetVar)node.Node).OtherwiseValue.ToString();
                break;
              case VarType.numberType:
                ((IteSetVar)node.Node).OtherwiseValue = decimal.TryParse(((IteSetVar)node.Node).OtherwiseValue, out decimal d) ? d : 0;
                break;
              default:
                break;
            }
          }
        }
      }
      foreach (var n in node.Items)
      {
        ValidateSequenceVariableTypesForNode(n);
      }
    }
    private void FileUnload(bool unselectProgram)
    {
      Paused = true;
      resetCloseMenuTimer();
      btnUnloadFile.Visibility = Visibility.Collapsed;
      //btnSaveAs.Visibility = Visibility.Collapsed;
      Running = null;
      resetForm(unselectProgram);
      if (keyEip != null) { keyEip.Disconnect(); keyEip = null; }
    }
    private void toggleFiles(object sender, RoutedEventArgs e)
    {
      resetCloseMenuTimer();
      if (gridFiles.Visibility == Visibility.Visible)
      {
        gridFiles.Visibility = Visibility.Collapsed;
      }
      else
      {
        gridFiles.Visibility = Visibility.Visible;
        gridAddins.Visibility = Visibility.Collapsed;
      }
    }
    private bool SaveFile(string filespec, bool overwriteIfExist)
    {
      if (!string.IsNullOrWhiteSpace(filespec))
      {
        if (System.IO.File.Exists(filespec) && !overwriteIfExist) { return false; }
        if (!Modes.Any()) { return false; }
        using (var sw = new System.IO.StreamWriter(filespec, false))
        {
          sw.Write("<{0} ver='{1}'>", xmlTag_App, txtVer.Text);
          if (IoAdams != null && IoAdams.Enabled)
          {
            //IoAdams = new IoAdams(new IoAdams.ConnectionSettings());
            sw.Write("<{0} ip='{1}' p='{2}' t='{3}' u='{4}' />", xmlTag_IO,
                        IoAdams.Settings.IpAddress,
                         IoAdams.Settings.Port,
                         (int)IoAdams.Settings.ProtocolType,
                         (int)IoAdams.Settings.Unit
                        );
          }
          if (keyEip != null && keyEip.Enabled)
          {
            //keyEip = new KeyenceEip();
            sw.Write("<{0} ip='{1}' p='{2}' ai='{3}' ao='{4}' />", xmlTag_KeyenceEip,
                        keyEip.Settings.IpAddress,
                         keyEip.Settings.Port,
                         keyEip.Settings.AssemblyIn,
                         keyEip.Settings.AssemblyOut
                        );
          }
          foreach (var mode in Modes)
          {
            AppendNodeData(sw, mode);
          }
          foreach (var v in Vars)
          {
            sw.Write("<{0} name='{1}' type='{2}' val='{3}' note='{4}' io='{5}' i='{6}' ioc='{7}'/>", xmlTag_Var,
                        (v.Name ?? string.Empty).Replace('\'', '"'),
                        v.VarType,
                        (v.Value.ToString() ?? string.Empty).Replace('\'', '"'),
                        (v.Note ?? string.Empty).Replace('\'', '"'),
                        v.IsOutput == true ? "1" : v.IsOutput == false ? "0" : "",
                        v.Channel,
                        (int)v.IoController
                        );
          }
          sw.Write("</{0}>", xmlTag_App);
        }
        return true;
      }
      else { return false; }
    }
    private void AppendNodeData(System.IO.StreamWriter sw, IteNodeViewModel n)
    {
      switch (n.NodeType)
      {
        case NodeTypes.General:
          sw.Write("<general name='{0}' type='{1}' exp='{2}'>", n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</general>");
          break;
        case NodeTypes.Mode:
          sw.Write("<{0} name='{1}' type='{2}' exp='{3}'>", xmlTag_Mode, n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Mode);
          break;
        case NodeTypes.Condition:
          var p = (IteCondition)n.Node;
          sw.Write("<{0} name='{1}' varname='{2}' method='{3}' desired='{4}' exp='{5}'>", xmlTag_Condition, p.Name, p.VariableName, p.EvalMethodText, p.DesiredValue, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Condition);
          break;
        case NodeTypes.Timer:
          var t = (IteTimer)n.Node;
          sw.Write("<{0} name='{1}' type='{2}' i='{3}' exp='{4}'>", xmlTag_Timer, n.Name, n.NodeType, t.Interval, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Timer);
          break;
        case NodeTypes.Initialized:
          sw.Write("<{0} name='{1}' type='{2}' exp='{3}'>", xmlTag_Init, n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Init);
          break;
        case NodeTypes.Continuous:
          sw.Write("<{0} name='{1}' type='{2}' exp='{3}'>", xmlTag_Continuous, n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Continuous);
          break;
        case NodeTypes.ConditionTrue1:
          sw.Write("<{0} name='{1}' type='{2}' exp='{3}'>", xmlTag_ConditionTrue1, n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionTrue1);
          break;
        case NodeTypes.ConditionTrue:
          sw.Write("<{0} name='{1}' type='{2}' exp='{3}'>", xmlTag_ConditionTrue, n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionTrue);
          break;
        case NodeTypes.ConditionFalse1:
          sw.Write("<{0} name='{1}' type='{2}' exp='{3}'>", xmlTag_ConditionFalse1, n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionFalse1);
          break;
        case NodeTypes.ConditionFalse:
          sw.Write("<{0} name='{1}' type='{2}' exp='{3}'>", xmlTag_ConditionFalse, n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionFalse);
          break;
        case NodeTypes.SetVariable:
          var v = (IteSetVar)n.Node;
          sw.Write("<{0} name='{1}' varname='{2}' type='{3}' val='{4}' method='{5}' other='{6}' exp='{7}'/>", xmlTag_SetVar, v.Name, v.VariableName, v.VarType, v.Value, (int)v.AssignMethod, v.OtherwiseValue, n.IsExpanded);
          break;
        case NodeTypes.SetMode:
          var v1 = (IteSetMode)n.Node;
          sw.Write("<{0} name='{1}' mode='{2}' exp='{3}'/>", xmlTag_SetMode, v1.Name, v1.ModeName, n.IsExpanded);
          break;
        case NodeTypes.Return:
          sw.Write("<{0} name='{1}'/>", xmlTag_Return, ((IteReturn)n.Node).Name);
          break;
        case NodeTypes.Alarm:
          var a = (IteAlarm)n.Node;
          sw.Write("<{0} name='{1}' title='{2}' prompt='{3}' c1='{4}' c2='{5}' varname='{6}' val='{7}' altvarname='{8}' altval='{9}'/>", xmlTag_Alarm, a.Name, a.Title, a.Prompt, a.Color1, a.Color2, a.VariableNameOnOkClick, a.OkValue, a.VariableNameOnCancelClick, a.CancelValue);
          break;
        default:
          sw.Write("<unknown name='{0}' type='{1}' exp='{2}'/>", n.Name, n.NodeType, n.IsExpanded);
          AppendChildren(sw, n);
          break;
      }
    }
    private void AppendChildren(System.IO.StreamWriter sw, IteNodeViewModel n)
    {
      foreach (var item in n.Items)
      {
        AppendNodeData(sw, item);
      }
    }
    private void resetForm(bool unselectProgram = false)
    {
      loadedFile = null;
      btnLoadFileText.Text = "select file";
      //fileToBeLoaded = null;
      UpdateHeader("Load a program.");
      btnUnloadFile.Visibility = Visibility.Collapsed;
      if (unselectProgram)
      {
        listFiles.SelectedItem = null;
        listRecentFiles.SelectedItem = null;
        SetDefaultGuiElements();
        Vars.Clear();
      }
    }
    private void UpdateHeader(object o)
    {
      UpdateHeader((o ?? string.Empty).ToString(), Colors.White);
    }
    private void UpdateHeader(string text, Color textColor)
    {
      if (clrHeader == null)
      {
        clrHeader = new System.Threading.Timer(new System.Threading.TimerCallback(UpdateHeader), null, 30000, System.Threading.Timeout.Infinite);
      }
      else
      {
        clrHeader.Change(30000, System.Threading.Timeout.Infinite);
      }
      Dispatcher.Invoke(() =>
      {
        txtTitle.Text = text ?? string.Empty;
        textColor = textColor == null ? Colors.White : textColor;
        txtTitle.Foreground = new SolidColorBrush(textColor);
      });
    }
    private void SetDefaultGuiElements()
    {
      Modes = new List<IteNodeViewModel>()
            {
                new IteFIRST_VM( new IteFirstScan("1st scan")){IsLocked=true },
                new IteCONTINUOUS_VM( new IteContinuous("Always")){IsLocked=true },
                new IteMODE_VM( new IteMode("Stop"){Items={new IteIntialize("Initialize"),new IteContinuous("Continuous") } }){ IsLocked=true },
                new IteMODE_VM( new IteMode("Auto"){Items={new IteIntialize("Initialize"),new IteContinuous("Continuous") } }){ IsLocked=true },
            };
      tree.DataContext = null;
      tree.DataContext = new { Modes };
    }
    private void btnLoadFile_Click(object sender, RoutedEventArgs e)
    {
      if (btnLoadFileText.Text == "select file" || fileToBeLoaded == null)
      {
        return;
      }
      else if (btnLoadFileText.Text.StartsWith("Save ["))
      {
        if (SaveFile(Properties.Settings.Default.ProgramPath.TrimEnd('\\') + @"\" + loadedFile + ".rox", true) == true) { UpdateHeader(string.Format("{0} saved. {1:yyyy-MM-dd hh:mm}", loadedFile, DateTime.Now)); }
        return;
      }
      try
      {
        if (!string.IsNullOrWhiteSpace(fileToBeLoaded))
        {
          FileLoad(fileToBeLoaded);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }
    private void toggleRun(object sender, RoutedEventArgs e)
    {
      toggleRun();
    }
    private void toggleRun()
    {
      if (string.IsNullOrEmpty(loadedFile))
      {
        showFiles();
      }
      else
      {
        Running = !Running;
      }
    }
    private void showFiles()
    {
      resetCloseMenuTimer();
      gridFiles.Visibility = Visibility.Visible;
    }
    private void PopulateFilelists()
    {
      PopulateRecentFilelist();
      PopulateFilelist();
    }
    private void PopulateRecentFilelist()
    {
      listRecentFiles.Items.Clear();
      foreach (var item in Properties.Settings.Default.MruFiles.Cast<string>().Distinct().Take(3))
      {
        listRecentFiles.Items.Add(item);
      }
    }
    private void PopulateFilelist()
    {
      listFiles.ItemsSource = GetFiles();
      if (listRecentFiles.Items.Count > 0)
      {
        listRecentFiles.SelectedItem = listRecentFiles.Items[0];
      }
    }
    private List<string> GetFiles(bool OrderByRecent = false)
    {
      List<string> ret = new List<string>();
      //if (includeUnload) { ret.Add("> unload <"); }
      try
      {
        if (!System.IO.Directory.Exists(Properties.Settings.Default.ProgramPath))
        {
          System.IO.Directory.CreateDirectory(Properties.Settings.Default.ProgramPath);
        }
        System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(Properties.Settings.Default.ProgramPath);
        IEnumerable<System.IO.FileInfo> files;
        if (OrderByRecent)
        {
          files = dirInfo.GetFiles("*.rox").OrderByDescending(p => p.LastWriteTime);
        }
        else
        {
          files = dirInfo.GetFiles("*.rox").OrderBy(p => p.Name);
        }
        foreach (var file in files)
        {
          ret.Add(file.Name.Substring(0, file.Name.IndexOf(".rox")));
        }
      }
      catch (Exception)
      {
        throw;
      }
      return ret;

    }
    private void SetHelperText(string t)
    {
      txtSelectedNodeInfo.Text = t;
    }
    private void tvi_GotFocus(object sender, RoutedEventArgs e)
    {
      var s = ((IteNodeViewModel)((TreeViewItem)sender).DataContext);
      selectedNode = s;
      if (s.IsLocked)
      {
        btnDeleteSelectedNode.Visibility = Visibility.Collapsed;
      }
      else
      {
        btnDeleteSelectedNode.Visibility = Visibility.Visible;
      }
      SetHelperText(s.Name + " - " + s.Description);
      switch (s.NodeType)
      {
        case NodeTypes.General:
        case NodeTypes.Initialized:
        case NodeTypes.Continuous:
        case NodeTypes.ConditionTrue:
        case NodeTypes.ConditionFalse:
        default:
          ClearNodeOptionsPanel();
          break;
        case NodeTypes.Mode:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel("NodeOptionsBasic");
          }
          break;
        case NodeTypes.Condition:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel("NodeOptionsCondition");
          }
          break;
        case NodeTypes.Timer:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel("NodeOptionsTimer");
          }
          break;
        case NodeTypes.SetVariable:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel("NodeOptionsPanel_SetVar");
          }
          break;
        case NodeTypes.SetMode:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel("NodeOptionsPanel_SetMode");
          }
          break;
        case NodeTypes.Return:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel("NodeOptionsBasic");
          }
          break;
        case NodeTypes.Alarm:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel("NodeOptionsPanel_Alarm");
          }
          break;
      }
      e.Handled = true;
    }
    private void txtNodeName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      try
      {
        ((IteNodeViewModel)((TextBox)sender).GetBindingExpression(TextBox.TextProperty).ResolvedSource).Name = ((TextBox)sender).Text;
      }
      catch (Exception)
      {
      }
    }
    private void ClearNodeOptionsPanel()
    {
      NodeOptions.ContentTemplate = null;
    }
    private void SetNodeOptionsPanel(string Name)
    {
      NodeOptions.ContentTemplate = (DataTemplate)Application.Current.MainWindow.FindResource(Name);
    }
    private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      startDragPoint = e.GetPosition(null);
    }
    private void Mode_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteNode", new IteMode("Mode") { Items = { new IteIntialize("Initialize"), new IteContinuous("Continuous") } });
          DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void Condition_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteNode", new IteCondition("Condition") { Items = { new IteTrue1("0→1"), new IteTrue("True"), new IteFalse1("1→0"), new IteFalse("False") } });
          DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void Timer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteNode", new IteTimer("Timer") { Items = { new IteTrue("Expired"), new IteFalse("Waiting") } });
          DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void SetVar_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteNode", new IteSetVar("Assign") { });
          DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void SetMode_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteNode", new IteSetMode("Set Mode") { });
          DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void Return_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteNode", new IteReturn("Return") { });
          DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void Alarm_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteNode", new IteAlarm("Alarm") { });
          DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void Node_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed
          //&& (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
          )
        {
          var test = (((TreeViewItem)sender).DataContext).GetType();
          DataObject dragData = null;
          if (test == typeof(IteCONDITION_VM) ||
              test == typeof(IteTIMER_VM) ||
              test == typeof(IteSETVAR_VM) ||
              test == typeof(IteSETMODE_VM) ||
              test == typeof(IteRETURN_VM) ||
              test == typeof(IteALARM_VM))
          {
            dragData = new DataObject("iteNodeVM", (IteNodeViewModel)((dynamic)e.OriginalSource).DataContext);
          }
          else
          {
            return;
            //e.Handled = true;
          }
          try
          {
            // Initialize the drag & drop operation
            DragDrop.DoDragDrop((TreeViewItem)sender, dragData, DragDropEffects.Move);
          }
          catch (Exception)
          {
          }
        }
        resetLogoutTimer();
      }
    }
    private void tvi_DragEnter(object sender, DragEventArgs e)
    {
      try
      {
        if (LoggedIn && sender != e.Source)
        {
          if (e.Data.GetDataPresent("iteNode") || e.Data.GetDataPresent("iteNodeVM"))
          {
            // test if node can be dropped here
            var target = (TreeViewItem)sender;
            var dc = (IteNodeViewModel)target.DataContext;
            var nodetype = e.Data.GetDataPresent("iteNode") ? ((INode)e.Data.GetData("iteNode")).NodeType : ((IteNodeViewModel)e.Data.GetData("iteNodeVM")).Node.NodeType;
            if (dc.AllowedNodes.Contains(nodetype))
            {
              e.Effects = DragDropEffects.Copy;
              target.Background = treeBackgroundAllowDrop;
            }
            else if (SequenceNodes.Contains(dc.NodeType))
            {
              e.Effects = DragDropEffects.Copy;
              target.Background = treeBackgroundAllowDropInSeq;
            }
            else
            {
              e.Effects = DragDropEffects.None;
            }
          }
          resetLogoutTimer();
        }
        else
        {
          e.Effects = DragDropEffects.None;
        }
        e.Handled = true;

      }
      catch (Exception)
      {
        e.Handled = true;
      }
    }
    private void tvi_DragOver(object sender, DragEventArgs e)
    {
      if (LoggedIn && sender != e.Source && (e.Data.GetDataPresent("iteNode") || e.Data.GetDataPresent("iteNodeVM")))
      {
        e.Effects = DragDropEffects.Copy;
        resetLogoutTimer();
      }
      else
      {
        e.Effects = DragDropEffects.None;
        e.Handled = true;
      }
    }
    private void tvi_Drop(object sender, DragEventArgs e)
    {
      try
      {
        if (LoggedIn && (e.Data.GetDataPresent("iteNode") || e.Data.GetDataPresent("iteNodeVM")))
        {
          // test if node can be dropped here
          var target = (TreeViewItem)sender;
          var dc = (IteNodeViewModel)target.DataContext;
          INode sourceNode = (INode)e.Data.GetData("iteNode") ?? ((IteNodeViewModel)e.Data.GetData("iteNodeVM")).Node;
          IteNodeViewModel N = (IteNodeViewModel)e.Data.GetData("iteNodeVM");
          var success = false;
          bool IsDropInSeq = SequenceNodes.Contains(dc.NodeType);
          if (dc.AllowedNodes.Contains(sourceNode.NodeType) || IsDropInSeq)
          {
            try
            {
              ((IteNodeViewModel)tree.SelectedItem).IsSelected = false;
            }
            catch (Exception)
            {
            }
            var T = sourceNode.GetType();
            if (T == typeof(IteMode))
            {
              N = N ?? new IteMODE_VM(sourceNode) { IsSelected = true };
              if (N.Parent != null) { N.Parent.Items.Remove(N); } // should never happen but WTH
              Modes.Add(N);
              N.Parent = null;
              success = true;
            }
            else if (T == typeof(IteCondition))
            {
              N = N ?? new IteCONDITION_VM(sourceNode) { IsSelected = true };
              success = InsertNode(IsDropInSeq, N, dc, (startDragPoint - e.GetPosition(this)).Y >= 0);
            }
            else if (T == typeof(IteTimer))
            {
              N = N ?? new IteTIMER_VM(sourceNode) { IsSelected = true };
              success = InsertNode(IsDropInSeq, N, dc, (startDragPoint - e.GetPosition(this)).Y >= 0);
            }
            else if (T == typeof(IteSetVar))
            {
              N = N ?? new IteSETVAR_VM(sourceNode) { IsSelected = true };
              success = InsertNode(IsDropInSeq, N, dc, (startDragPoint - e.GetPosition(this)).Y >= 0);
            }
            else if (T == typeof(IteSetMode))
            {
              N = N ?? new IteSETMODE_VM(sourceNode) { IsSelected = true };
              success = InsertNode(IsDropInSeq, N, dc, (startDragPoint - e.GetPosition(this)).Y >= 0);
            }
            else if (T == typeof(IteReturn))
            {
              N = N ?? new IteRETURN_VM(sourceNode) { IsSelected = true };
              success = InsertNode(IsDropInSeq, N, dc, (startDragPoint - e.GetPosition(this)).Y >= 0);
            }
            else if (T == typeof(IteAlarm))
            {
              N = N ?? new IteALARM_VM(sourceNode) { IsSelected = true };
              success = InsertNode(IsDropInSeq, N, dc, (startDragPoint - e.GetPosition(this)).Y >= 0);
            }
            else
            {
              //N = new IteNodeViewModel(null);
            }
          }
          if (success)
          {
            tree.DataContext = null;
            tree.DataContext = new { Modes };
          }
          resetLogoutTimer();
        }
        e.Handled = true;
        ((TreeViewItem)sender).Background = treeBackground;

      }
      catch (Exception)
      {
      }
    }
    private bool InsertNode(bool IsDropInSeq, IteNodeViewModel newOrMoved, IteNodeViewModel dropTo, bool insertAbove = true)
    {
      if (!LoggedIn || newOrMoved == null || dropTo == null) { return false; }
      try
      {
        // detect if the source is trying to be dropped inside itself
        if (RecursiveCheckForParentInChild(newOrMoved, dropTo))
        {
          (new CustomMessageboxWindow("Invalid destination", "Unable to put the selected item here.", MessageBoxButton.OK) { Owner = this }).ShowDialog();
          return false;
        }
        if (newOrMoved.Parent != null) { newOrMoved.Parent.Items.Remove(newOrMoved); }
        if (IsDropInSeq)
        {
          // insert before or after dropped on node
          if (dropTo.Parent != null)
          {
            dropTo.Parent.Items.Insert(insertAbove ? dropTo.Parent.Items.IndexOf(dropTo) : dropTo.Parent.Items.IndexOf(dropTo) + 1, newOrMoved);
          }
          newOrMoved.Parent = dropTo.Parent;
        }
        else
        {
          // add to items
          dropTo.Items.Add(newOrMoved);
          newOrMoved.Parent = dropTo;
        }
        resetLogoutTimer();
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
    private bool RecursiveCheckForParentInChild(IteNodeViewModel parent, IteNodeViewModel child)
    {
      foreach (var n in parent.Items)
      {
        if (n == child)
        {
          return true;
        }
        if (RecursiveCheckForParentInChild(n, child)) { return true; }
      }
      return false;
    }
    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
      if (LoggedIn)
      {
        var f = new CustomMessageboxWindow(string.Format("Start over?", loadedFile), string.Format("Delete the existing sequence and start over?", loadedFile), MessageBoxButton.YesNo) { Owner = this };
        f.ShowDialog();
        if (f.DialogResult == true)
        {
          FileUnload(true);
          //SetDefaultGuiElements();
        }
        resetLogoutTimer();
      }
    }
    private void tvi_DragLeave(object sender, DragEventArgs e)
    {
      ((TreeViewItem)sender).Background = treeBackground;
      //e.Handled = true;
    }
    private void Tree_DragEnter(object sender, DragEventArgs e)
    {
      if (LoggedIn && e.Data.GetDataPresent("iteNode"))
      {
        // test if node can be dropped here
        if (((INode)e.Data.GetData("iteNode")).NodeType == NodeTypes.Mode)
        {
          e.Effects = DragDropEffects.Copy;
          tree.Background = treeBackgroundAllowDrop;
        }
        else
        {
          e.Effects = DragDropEffects.None;
        }
        resetLogoutTimer();
      }
      else
      {
        e.Effects = DragDropEffects.None;
      }
      e.Handled = true;
    }
    private void Tree_Drop(object sender, DragEventArgs e)
    {
      if (LoggedIn && e.Data.GetDataPresent("iteNode"))
      {
        // test if node can be dropped here
        var target = (TreeView)sender;
        var source = (INode)e.Data.GetData("iteNode");
        IteNodeViewModel N = null;
        if (source.GetType() == typeof(IteMode))
        {
          try
          {
            ((IteNodeViewModel)tree.SelectedItem).IsSelected = false;
          }
          catch (Exception)
          {
          }
          N = new IteMODE_VM(source) { IsSelected = true };
          Modes.Add(N);
          tree.DataContext = null;
          tree.DataContext = new { Modes };
        }
        resetLogoutTimer();
      }
      e.Handled = true;
      tree.Background = treeBackground;

    }
    private void Tree_DragLeave(object sender, DragEventArgs e)
    {
      tree.Background = treeBackground;
    }
    private void BtnAddVariable_Click(object sender, RoutedEventArgs e)
    {
      if (LoggedIn)
      {
        var d = new VarParamsWindow() { Owner = this };
        d.ShowDialog();
        if (d.DialogResult == true && !string.IsNullOrWhiteSpace(d.VarName))
        {
          if (Vars.Where(p => p.Name == d.VarName).Any()) { return; }
          Vars.Add(new Variable { Name = d.VarName, Note = d.VarNote, Value = d.VarValue, UsersLastValue = d.VarValue, Channel = d.Channel, IsOutput = d.IsOutput, IoController = d.IoController });
          AssignLiveVars();
        }
        AutoSizeVarColumns();
        resetLogoutTimer();
      }
    }
    public void AutoSizeVarColumns()
    {
      if (listVars.View is GridView gv)
      {
        foreach (var c in gv.Columns)
        {
          if (double.IsNaN(c.Width))
          {
            c.Width = c.ActualWidth;
          }
          c.Width = double.NaN;
        }
      }
    }
    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (LoggedIn)
      {
        var n = ((ListViewItem)sender).Content as Variable;

        var d = new VarParamsWindow() { Owner = this, VarName = n.Name, VarNote = n.Note, VarType = (VarType)n.VarType.Value, Channel = n.Channel, IsOutput = n.IsOutput, IoController = n.IoController };
        d.VarValue = n.Value;
        d.ShowDialog();
        if (d.DialogResult == true && !string.IsNullOrWhiteSpace(d.VarName))
        {
          n.Name = d.VarName;
          n.Note = d.VarNote;
          n.Value = d.VarValue;
          n.Channel = d.Channel;
          n.IsOutput = d.IsOutput;
          n.IoController = d.IoController;
          AssignLiveVars();
        }
        //AutoSizeVarColumns();
        resetLogoutTimer();
      }
    }
    private void Logic_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        if (((ComboBox)sender).SelectedIndex < 0) { return; }
        var l = ((ComboBoxItem)((ComboBox)sender).SelectedValue).Content.ToString();
        var o = (((INode)((ComboBox)sender).GetBindingExpression(ComboBox.TextProperty).ResolvedSource) as IteCondition);
        o.EvalMethodText = l;
        o.EvalMethod = GetEvalMethodFromString(l);
        SetAndEvaluateLogicStatement(o);
      }
      catch (Exception)
      {
      }
    }
    private Func<dynamic, dynamic, bool> GetEvalMethodFromString(string s)
    {
      switch (s)
      {
        default:
          return Logic.Equals;
        case ">":
          return Logic.GreaterThan;
        case "<":
          return Logic.LessThan;
        case ">=":
          return Logic.GreaterThanOrEqual;
        case "<=":
          return Logic.LessThanOrEqual;
        case "!=":
          return Logic.NotEqual;
      }
    }
    private VarType GetVarTypeFromString(string s)
    {
      if (string.IsNullOrEmpty(s)) { return VarType.boolType; }
      if (s.StartsWith("System.")) { s = s.Substring(7); }
      switch (s.ToLower())
      {
        default:
          return VarType.boolType;
        case "3":
        case "numbertype":
        case "n":
        case "num":
        case "numb":
        case "number":
        case "int":
        case "byte":
        case "short":
        case "long":
        case "dec":
        case "decimal":
        case "real":
        case "double":
          return VarType.numberType;
        case "2":
        case "string":
        case "stringtype":
        case "text":
        case "txt":
        case "t":
        case "s":
          return VarType.stringType;
      }
    }
    private void DesiredValue_TextChanged(object sender, TextChangedEventArgs e)
    {
      var o = ((ComboBox)sender).GetBindingExpression(ComboBox.TextProperty).ResolvedSource as IteCondition;
      var t = ((ComboBox)sender).Text;
      o.DesiredValue = ((ComboBox)sender).Text;
      SetAndEvaluateLogicStatement(o);
    }
    private void SetAndEvaluateLogicStatement(IteCondition o)
    {
      if (o == null || string.IsNullOrEmpty(o.VariableName)) { return; }
      try
      {
        var CurrentValue = Vars.Where(p => p.Name == o.VariableName).FirstOrDefault().Value;
        var CurrentValueType = CurrentValue.GetType();
        if (CurrentValueType != o.DesiredValue.GetType())
        {
          if (CurrentValueType == typeof(bool))
          {
            o.DesiredValue = bool.TryParse(o.DesiredValue.ToString(), out bool b) ? b : true;
          }
          else if (CurrentValueType == typeof(int) || CurrentValueType == typeof(decimal) || CurrentValueType == typeof(long) || CurrentValueType == typeof(short) || CurrentValueType == typeof(byte))
          {
            o.DesiredValue = decimal.TryParse(o.DesiredValue.ToString(), out decimal d) ? d : 0;
          }
          else if (CurrentValueType == typeof(string))
          {
            o.DesiredValue = o.DesiredValue.ToString();
          }
        }
        o.Evaluation = o.EvalMethod(o.DesiredValue, CurrentValue);
        resetLogoutTimer();
      }
      catch (Exception)
      {
      }
    }
    private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (LoggedIn)
      {
        Vector diff = startDragPoint - e.GetPosition(null);
        if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
        {
          // Initialize the drag & drop operation
          DataObject dragData = new DataObject("iteVar", (Variable)((ListViewItem)sender).Content);
          DragDrop.DoDragDrop((ListViewItem)sender, dragData, DragDropEffects.Move);
        }
        resetLogoutTimer();
      }
    }
    private void Var_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent("iteVar"))
      {
        var source = (Variable)e.Data.GetData("iteVar");
        ((TextBox)sender).Text = source.Name;
        var n = (INode)((TextBox)sender).GetBindingExpression(TextBox.TextProperty).ResolvedSource;
        if (n.GetType() == typeof(IteCondition))
        {
          var o = (IteCondition)n;
          o.VariableName = source.Name;
          SetAndEvaluateLogicStatement(o);
        }
        else if (n.GetType() == typeof(IteSetVar))
        {
          ((IteSetVar)n).VariableName = source.Name;
        }
      }
      e.Handled = true;
      ((TextBox)sender).Background = textboxBackground;
    }
    private void Var_PreviewDragEnter(object sender, DragEventArgs e)
    {
      e.Handled = true;
      if (e.Data.GetDataPresent("text") || e.Data.GetDataPresent("iteVar"))// && sender != e.Source)
      {
        ((TextBox)sender).Background = treeBackgroundAllowDrop;
      }
      else
      {
        ((TextBox)sender).Background = textboxBackground;
      }
    }
    private void TextBox_PreviewDragLeave(object sender, DragEventArgs e)
    {
      ((TextBox)sender).Background = textboxBackground;
      e.Handled = true;
    }
    private void BtnUnloadFile_Click(object sender, RoutedEventArgs e)
    {
      FileUnload(true);
    }
    private void BtnAbort_Click(object sender, RoutedEventArgs e)
    {
      Paused = !Paused;
      resetLogoutTimer();
    }
    private void AssignCurrentVariableValues()
    {
      if (LiveVars != null)
      {
        var AdamsInputs = new bool[0];
        var AdamsOutputs = new bool[0];
        var KeyenceInputs = new byte[0];
        var KeyenceOutputs = new byte[0];


        if (IoAdams == null && keyEip == null) { UpdateHeader("I/O module is not defined. Live variables are not updated.", Colors.Red); return; }
        else
        {
          if (IoAdams != null && IoAdams.Enabled)
          {
            if (!IoAdams.IsConnected)
            {
              IoAdams.Connect();
              if (!IoAdams.IsConnected)
              {
                UpdateHeader(string.Format("Advantech module not connected! Next attempt in {0:0} sec.", IoAdams.MinReconnectTime.TotalSeconds - (DateTime.Now - (IoAdams.LastFailedReconnectTime ?? DateTime.Now)).TotalSeconds));
                return;
              }
              else
              {
                UpdateHeader("Advantech module reconnected.");
              }
            }
            AdamsInputs = IoAdams.GetInputs();
            AdamsOutputs = IoAdams.GetOutputs();
          }
          if (keyEip != null && keyEip.Enabled)
          {
            if (!keyEip.IsConnected)
            {
              keyEip.Connect();
              if (!keyEip.IsConnected)
              {
                UpdateHeader("Keyence module not connected!");
                return;
              }
              else
              {
                UpdateHeader("Keyence module reconnected.");
              }
            }
            KeyenceInputs = keyEip.GetInputs();
            KeyenceOutputs = keyEip.GetOutputs();
          }
          foreach (var v in LiveVars)
          {
            try
            {
              switch (v.IoController)
              {
                case IoControllers.Advantech:
                  if (v.VarType.enumValue == VarType.boolType)
                  {
                    v.Value = v.IsOutput == true
                              ? (v.Channel < AdamsOutputs.Length ? AdamsOutputs[v.ChannelBit] : v.Value)
                              : v.IsOutput == false
                              ? (v.Channel < AdamsInputs.Length ? AdamsInputs[v.ChannelBit] : v.Value)
                              : v.Value;
                  }
                  else if (v.VarType.enumValue == VarType.numberType)
                  {

                  }
                  else if (v.VarType.enumValue == VarType.stringType)
                  {

                  }
                  break;
                case IoControllers.Keyence:
                  if (v.VarType.enumValue == VarType.boolType)
                  {
                    // using "(b >> bitNumber) & 1" to test bit level value (as num) then converting to bool (n==1) defaulting to false
                    // (b >> bitNumber) & 1 
                    // equates to 
                    // (KeyenceOutputs[v.ChannelByte] >> v.ChannelBit) & 1
                    v.Value = v.IsOutput == true
                      ? (v.ChannelWord < KeyenceOutputs.Length ? ((KeyenceOutputs[v.ChannelWord] >> v.ChannelBit) & 1) == 1 : v.Value)
                      : (v.ChannelWord < KeyenceInputs.Length ? ((KeyenceInputs[v.ChannelWord] >> v.ChannelBit) & 1) == 1 : v.Value);
                  }
                  else if (v.VarType.enumValue == VarType.numberType)
                  {
                    v.Value = v.IsOutput == true
                      ? (v.ChannelWord < KeyenceOutputs.Length ? KeyenceOutputs[v.ChannelWord] : v.Value)
                      : (v.ChannelWord < KeyenceInputs.Length ? KeyenceInputs[v.ChannelWord] : v.Value);
                  }
                  else if (v.VarType.enumValue == VarType.stringType)
                  {

                  }
                  break;
              }
            }
            catch (Exception)
            {
            }
          }
        }
      }
    }
    private void RunSequence()
    {
      try
      {
        while (!Paused)
        {
          //if (keyEip != null) { Console.WriteLine(keyEip.GetState()); }
          AssignCurrentVariableValues();
          System.Threading.Thread.Sleep(1);

          var e = new SequenceEventArgs();
          foreach (var mode in Modes)
          {
            try
            {
              if (
                  (mode.NodeType == NodeTypes.Continuous) ||  // Always
                  (curMode == mode.Name) || // current mode
                  (mode.NodeType == NodeTypes.Initialized && string.IsNullOrEmpty(processingMode)) // 1st scan  string.IsNullOrEmpty(curMode) || 
                  )
              {
                ProcessValidNodeSequence(mode, curMode != processingMode, e);
                if (e.AbortIteration == true) { break; }
              }
              else
              {
                ProcessInvalidNodeSequence(mode);
              }
            }
            catch (Exception)
            {
              throw;
            }
          }
          if (e.AbortIteration != true) { processingMode = curMode; }
        }
      }
      catch (Exception)
      {
        Dispatcher.Invoke(() =>
        {
          Paused = true;
          MessageBox.Show("Unhandled error while running the sequence. This is most likely caused from loading a previous version or otherwise unsupported file. The sequence is aborted. You can try to adjust the sequence and restart.", "Sequnce Error", MessageBoxButton.OK, MessageBoxImage.Error);
        });
      }
    }
    private void ResetHighlight(IteNodeViewModel node)
    {
      node.Background = new SolidColorBrush(Colors.Transparent);
      foreach (var sub in node.Items)
      {
        node.Background = new SolidColorBrush(Colors.Transparent);
        ResetHighlight(sub);
      }
    }
    private void ProcessInvalidNodeSequence(IteNodeViewModel node)
    {
      switch (node.NodeType)
      {
        default:
          if (highlight) { node.Background = unprocessedNodeBackground; }
          foreach (var sub in node.Items)
          {
            ProcessInvalidNodeSequence(sub);
          }
          break;
        case NodeTypes.Timer:
          ((IteTimer)node.Node).Stop();
          if (highlight) { node.Background = unprocessedNodeBackground; }
          foreach (var sub in node.Items)
          {
            ProcessInvalidNodeSequence(sub);
          }
          break;
        case NodeTypes.SetVariable:
          if (highlight) { node.Background = unprocessedNodeBackground; }
          if (((IteSetVar)node.Node).OtherwiseValue != null)
          {
            try
            {
              var a = Vars.Where(p => p.Name == ((IteSetVar)node.Node).VariableName).FirstOrDefault();
              if (a.Value.GetType() != ((IteSetVar)node.Node).OtherwiseValue.GetType())
              {
                switch (((IteSetVar)node.Node).VarType.enumValue)
                {
                  case VarType.boolType:
                    ((IteSetVar)node.Node).OtherwiseValue = ((IteSetVar)node.Node).OtherwiseValue.ToString().Trim().ToLower();
                    if (((string)((IteSetVar)node.Node).OtherwiseValue).Length > 0) { ((IteSetVar)node.Node).OtherwiseValue = ((IteSetVar)node.Node).OtherwiseValue.Substring(0, 1); }
                    switch (((IteSetVar)node.Node).OtherwiseValue)
                    {
                      default:
                        ((IteSetVar)node.Node).OtherwiseValue = false;
                        break;
                      case "y":
                      case "1":
                      case "t":
                        ((IteSetVar)node.Node).OtherwiseValue = true;
                        break;
                    }
                    break;
                  case VarType.numberType:
                    ((IteSetVar)node.Node).OtherwiseValue = decimal.TryParse(((IteSetVar)node.Node).OtherwiseValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out decimal d) ? d : 0;
                    break;
                  case VarType.stringType:
                    ((IteSetVar)node.Node).OtherwiseValue = ((IteSetVar)node.Node).OtherwiseValue.ToString();
                    break;
                }
              }
              else
              {
                a.Value = ((IteSetVar)node.Node).OtherwiseValue;
                if (a.Channel >= 0 && a.IsOutput == true)
                {
                  // assign physical output
                  switch (a.IoController)
                  {
                    case IoControllers.Advantech:
                      switch (a.VarType.enumValue)
                      {
                        case VarType.boolType:
                          if (IoAdams != null && IoAdams.Enabled) { IoAdams.SetBit(a.ChannelBit, a.Value == true ? 1 : 0); }
                          break;
                        case VarType.stringType:
                          break;
                        case VarType.numberType:
                          break;
                        default:
                          break;
                      }
                      break;
                    case IoControllers.Keyence:
                      switch (a.VarType.enumValue)
                      {
                        case VarType.boolType:
                          if (keyEip != null && keyEip.Enabled) { keyEip.SetBit(a.ChannelWord, a.ChannelBit, a.Value == true); };
                          break;
                        case VarType.stringType:
                          break;
                        case VarType.numberType:
                          break;
                        default:
                          break;
                      }
                      break;
                  }

                }
              }
            }
            catch (Exception)
            {
            }
          }
          break;

      }
    }
    private void ProcessValidNodeSequence(IteNodeViewModel node, bool initialize, SequenceEventArgs e)
    {
      if (e.AbortIteration || Paused) { return; }
      switch (node.NodeType)
      {
        default: // ############################################################################################ DEFAULT
          if (highlight) { node.Background = processedNodeBackground; }
          foreach (var sub in node.Items)
          {
            ProcessValidNodeSequence(sub, initialize, e);
          }
          break;
        case NodeTypes.Condition: // ############################################################################################ CONDITION
          // get current value
          var v = Vars.Where(p => p.Name == ((IteCondition)node.Node).VariableName).FirstOrDefault();
          if (v != null)
          {
            if (highlight) { node.Background = processedNodeBackground; }
            //var thisNode = node;
            try
            {
              var eval = ((IteCondition)node.Node).EvalMethod(v.Value, ((IteCondition)node.Node).DesiredValue);
              if (eval)
              {
                if (v.Value != v.UsersLastValue)
                {
                  // initialized true
                  v.UsersLastValue = v.Value;
                  //node = node.Items.First(p => p.NodeType == NodeTypes.ConditionTrue);
                  if (highlight) { node.Items[0].Background = trueNodeBackground; }
                  foreach (var sub in node.Items[0].Items)
                  {
                    ProcessValidNodeSequence(sub, initialize, e);
                  }
                  ProcessInvalidNodeSequence(node.Items[1]);
                  ProcessInvalidNodeSequence(node.Items[2]);
                  ProcessInvalidNodeSequence(node.Items[3]);
                }
                else
                {
                  // true
                  if (highlight) { node.Items[1].Background = trueNodeBackground; }
                  foreach (var sub in node.Items[1].Items)
                  {
                    ProcessValidNodeSequence(sub, initialize, e);
                  }
                  ProcessInvalidNodeSequence(node.Items[0]);
                  ProcessInvalidNodeSequence(node.Items[2]);
                  ProcessInvalidNodeSequence(node.Items[3]);
                }
              }
              else
              {
                if (!v.Value.Equals(v.UsersLastValue))
                {
                  // initialized false
                  v.UsersLastValue = v.Value;
                  if (highlight) { node.Items[2].Background = falseNodeBackground; }
                  foreach (var sub in node.Items[2].Items)
                  {
                    ProcessValidNodeSequence(sub, initialize, e);
                  }
                  ProcessInvalidNodeSequence(node.Items[0]);
                  ProcessInvalidNodeSequence(node.Items[1]);
                  ProcessInvalidNodeSequence(node.Items[3]);
                }
                else
                {
                  // false
                  if (highlight) { node.Items[3].Background = falseNodeBackground; }
                  foreach (var sub in node.Items[3].Items)
                  {
                    ProcessValidNodeSequence(sub, initialize, e);
                  }
                  ProcessInvalidNodeSequence(node.Items[0]);
                  ProcessInvalidNodeSequence(node.Items[1]);
                  ProcessInvalidNodeSequence(node.Items[2]);
                }
              }
            }
            catch (Exception)
            {
              SetAndEvaluateLogicStatement((IteCondition)node.Node);
            }
          }
          else
          {
            if (highlight) { node.Background = errorNodeBackground; }
          }
          break;
        case NodeTypes.Initialized: // ############################################################################################ INITIALIZED
          if (initialize)
          {
            if (highlight) { node.Background = processedNodeBackground; }
            foreach (var sub in node.Items)
            {
              ProcessValidNodeSequence(sub, initialize, e);
            }
          }
          else
          {
            ProcessInvalidNodeSequence(node);
          }
          break;
        case NodeTypes.Timer: // ############################################################################################ TIMER
          if (highlight) { node.Background = processedNodeBackground; }
          var t = ((IteTimer)node.Node);
          t.UpdateTime();
          if (t.Expired)
          {
            if (highlight) { node.Items[0].Background = trueNodeBackground; }
            foreach (var sub in node.Items[0].Items)
            {
              ProcessValidNodeSequence(sub, initialize, e);
            }
            ProcessInvalidNodeSequence(node.Items[1]);
          }
          else
          {
            if (highlight) { node.Items[1].Background = falseNodeBackground; }
            foreach (var sub in node.Items[1].Items)
            {
              ProcessValidNodeSequence(sub, initialize, e);
            }
            ProcessInvalidNodeSequence(node.Items[0]);
          }
          break;
        case NodeTypes.SetVariable:
          try
          {
            Rox.Variable daVar = Vars.Where(p => p.Name == ((IteSetVar)node.Node).VariableName).FirstOrDefault();
            switch (((IteSetVar)node.Node).AssignMethod)
            {
              case AssignMethod.assign:
                daVar.Value = ((IteSetVar)node.Node).Value;
                break;
              case AssignMethod.invert:
                if (daVar.VarType.Value == (int)VarType.boolType) { daVar.Value = !daVar.Value; }
                break;
              case AssignMethod.increment:
                daVar.Value += ((IteSetVar)node.Node).Value;
                break;
              case AssignMethod.decrement:
                daVar.Value -= ((IteSetVar)node.Node).Value;
                break;
                //default:
                //  break;
            }
            if (daVar.Channel >= 0 && daVar.IsOutput == true)
            {
              // assign physical output
              switch (daVar.IoController)
              {
                case IoControllers.Advantech:
                  switch (daVar.VarType.enumValue)
                  {
                    case VarType.boolType:
                      if (IoAdams != null && IoAdams.Enabled) { IoAdams.SetBit(daVar.ChannelBit, daVar.Value == true ? 1 : 0); }
                      break;
                    case VarType.stringType:
                      break;
                    case VarType.numberType:
                      break;
                    default:
                      break;
                  }
                  break;
                case IoControllers.Keyence:
                  switch (daVar.VarType.enumValue)
                  {
                    case VarType.boolType:
                      if (keyEip != null && keyEip.Enabled) { keyEip.SetBit(daVar.ChannelWord, daVar.ChannelBit, daVar.Value == true); };
                      break;
                    case VarType.stringType:
                      break;
                    case VarType.numberType:
                      if (keyEip != null && keyEip.Enabled) { keyEip.SetNumber((short)daVar.ChannelWord, (byte)daVar.Value); };
                      break;
                    default:
                      break;
                  }
                  break;
              }
            }
            if (highlight) { node.Background = processedNodeBackground; }
          }
          catch (Exception)
          {
          }
          break;
        case NodeTypes.SetMode:
          curMode = ((IteSetMode)node.Node).ModeName;
          if (highlight) { node.Background = processedNodeBackground; }
          return;
        case NodeTypes.Return:
          if (highlight) { node.Background = processedNodeBackground; }
          e.AbortIteration = true;
          return;
        case NodeTypes.Alarm:
          if (highlight) { node.Background = processedNodeBackground; }
          AddAlarm(
            ((IteAlarm)node.Node).Title,
            ((IteAlarm)node.Node).Prompt,
            ((IteAlarm)node.Node).Color1,
            ((IteAlarm)node.Node).Color2,
            ((IteAlarm)node.Node).VariableNameOnOkClick,
            ((IteAlarm)node.Node).VariableNameOnCancelClick,
            ((IteAlarm)node.Node).OkValue,
            ((IteAlarm)node.Node).CancelValue
            );
          break;
      }
    }
    private void ChkHighlight_Checked(object sender, RoutedEventArgs e)
    {
      highlight = true;
      resetLogoutTimer();
    }
    private void ChkHighlight_Unchecked(object sender, RoutedEventArgs e)
    {
      highlight = false;
      System.Threading.Thread.Sleep(250);
      resetLogoutTimer();
      foreach (var mode in Modes)
      {
        ResetHighlight(mode);
      }
    }
    private void BtnDeleteSelectedNode_Click(object sender, RoutedEventArgs e)
    {
      FindAndRemoveNode((IteNodeViewModel)tree.SelectedItem);
    }
    private void FindAndRemoveNode(IteNodeViewModel node)
    {
      resetLogoutTimer();
      if (node == null) { return; }
      if (node.IsLocked) { return; }
      foreach (var n in Modes)
      {
        if (n == node && !n.IsLocked)
        {
          Modes.Remove(node);
          tree.DataContext = null;
          tree.DataContext = new { Modes };
          return;
        }
        else
        {
          FindAndRemoveNodeFromNode(n, node);
        }
      }
    }
    private void FindAndRemoveNodeFromNode(IteNodeViewModel Source, IteNodeViewModel ToBeRemoved)
    {
      if (ToBeRemoved.IsLocked) { return; }
      foreach (var node in Source.Items)
      {
        if (!node.IsLocked && node == ToBeRemoved)
        {
          if (node.Parent != null) { node.Parent.IsSelected = true; }
          Source.Items.Remove(node);
          tree.DataContext = null;
          tree.DataContext = new { Modes };
          return;
        }
        else
        {
          FindAndRemoveNodeFromNode(node, ToBeRemoved);
        }
      }
    }
    private void Datatype_TextChanged(object sender, TextChangedEventArgs e)
    {
      try
      {
        var s = (INode)((ComboBox)sender).GetBindingExpression(ComboBox.TextProperty).ResolvedSource;
        if (s != null && s.GetType() == typeof(IteSetVar))
        {
          var t = ((ComboBox)sender).Text.ToLower().Trim();
          if (t == "true" || t == "false")
          {
            ((IteSetVar)s).VarType = VariableTypes.boolType;
            ((IteSetVar)s).Value = t == "true" ? true : false;
          }
          else if (decimal.TryParse(t, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var d))
          {
            ((IteSetVar)s).VarType = VariableTypes.numberType;
            ((IteSetVar)s).Value = d;
          }
          else
          {
            ((IteSetVar)s).VarType = VariableTypes.stringType;
          }
        }
        resetLogoutTimer();
      }
      catch (Exception)
      {
      }
    }
    private void AlarmOkVal_TextChanged(object sender, TextChangedEventArgs e)
    {
      try
      {
        var s = (INode)((ComboBox)sender).GetBindingExpression(ComboBox.TextProperty).ResolvedSource;
        if (s != null && s.GetType() == typeof(IteAlarm))
        {
          var t = ((ComboBox)sender).Text.ToLower().Trim();
          if (t == "true" || t == "false")
          {
            //((IteAlarm)((IteALARM_VM)s).Node).VarType = VariableTypes.boolType;
            ((IteAlarm)s).OkValue = t == "true" ? true : false;
          }
          else if (decimal.TryParse(t, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var d))
          {
            //((IteAlarm)((IteALARM_VM)s).Node).VarType = VariableTypes.numberType;
            ((IteAlarm)s).OkValue = d;
          }
          else
          {
            //((IteAlarm)((IteALARM_VM)s).Node).VarType = VariableTypes.stringType;
          }
        }
      }
      catch (Exception)
      {
      }
      resetLogoutTimer();
    }
    private void AlarmCancelVal_TextChanged(object sender, TextChangedEventArgs e)
    {
      try
      {
        var s = (INode)((ComboBox)sender).GetBindingExpression(ComboBox.TextProperty).ResolvedSource;
        if (s != null && s.GetType() == typeof(IteAlarm))
        {
          var t = ((ComboBox)sender).Text.ToLower().Trim();
          if (t == "true" || t == "false")
          {
            //((IteAlarm)((IteALARM_VM)s).Node).VarType = VariableTypes.boolType;
            ((IteAlarm)s).CancelValue = t == "true" ? true : false;
          }
          else if (decimal.TryParse(t, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out var d))
          {
            //((IteAlarm)((IteALARM_VM)s).Node).VarType = VariableTypes.numberType;
            ((IteAlarm)s).CancelValue = d;
          }
          else
          {
            //((IteAlarm)((IteALARM_VM)s).Node).VarType = VariableTypes.stringType;
          }
        }
      }
      catch (Exception)
      {
      }
      resetLogoutTimer();
    }
    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(loadedFile))
      {
        (new CustomMessageboxWindow("No file loaded", "Please load a file to delete. Otherwise, you will need to delete it manually.", MessageBoxButton.OK) { Owner = this }).ShowDialog();
        return;
      }
      var f = new CustomMessageboxWindow(string.Format("Delete file '{0}'?", loadedFile), string.Format("Please confirm file '{0}' deletion. This cannot be undone.", loadedFile), MessageBoxButton.OKCancel) { Owner = this };
      f.ShowDialog();
      if (f.DialogResult == true)
      {
        var filespec = Properties.Settings.Default.ProgramPath.TrimEnd('\\') + @"\" + loadedFile + ".rox";
        if (System.IO.File.Exists(filespec)) // redundant, yes
        {
          FileUnload(true);
          System.IO.File.Delete(filespec);
          PopulateFilelists();
        }
      }
      resetLogoutTimer();
    }
    private void SeqItem_MouseLeave(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = null;
    }
    private void SeqItemMode_MouseEnter(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = "{ MODE } A mode can be created to easily abort a running mode and start new sequencing. Stop and Auto modes will run with Start/Stop button. No items can be added directly. Add items to Initialize or Continuous branches. \nDrag and Drop to add this item";
    }
    private void SeqItemSetMode_MouseEnter(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = "{ Set Mode } Changes the current mode to the assigned value. Sequence items are not allowed but remaining nodes and subsequent branches will still be processed for the current iteration. Use a Return to abandon remaining sequence. \nDrag and Drop to add this item.";
    }
    private void SeqItemCondition_MouseEnter(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = "{ Condition } This will evaluate a statement to true or false and run the appropriate sequence. Add items to True / False branches. \nDrag and Drop to add this item.";
    }
    private void SeqItemTimer_MouseEnter(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = "{ Timer } An accumulating millisecond timer. The timer can be running or expired and sequence items can be added to these conditions. The timer is reset if containing sequence is not executing. \nDrag and Drop to add this item.";
    }
    private void SeqItemSetVar_MouseEnter(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = "{ Set Variable } This assigns a value to a variable. Sequence items are not allowed.\nDrag and Drop to add this item.";
    }
    private void SeqItemReturn_MouseEnter(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = "{ Return } Aborts the current iteration. \nDrag and Drop to add this item.";
    }
    private void SeqItemAlarm_MouseEnter(object sender, MouseEventArgs e)
    {
      txtSelectedNodeInfo.Text = "{ Return } Aborts the current iteration. \nDrag and Drop to add this item.";
    }
    private void togglePlugins(object sender, RoutedEventArgs e)
    {
      if (gridAddins.Visibility == Visibility.Visible)
      {
        gridAddins.Visibility = Visibility.Collapsed;
      }
      else
      {
        gridAddins.Visibility = Visibility.Visible;
        gridFiles.Visibility = Visibility.Collapsed;
      }
    }
    private void btnAdvantech_Click(object sender, RoutedEventArgs e)
    {
      if (IoAdams == null) { IoAdams = new IoAdams(new IoAdams.ConnectionSettings()); }
      IOConfig d = new IOConfig() { Owner = this, Enabled = IoAdams.Enabled, IpAddress = IoAdams.Settings.IpAddress, Port = IoAdams.Settings.Port, Protocol = (ProtocolTypes)IoAdams.Settings.ProtocolType, Unit = IoAdams.Settings.Unit };
      d.ShowDialog();
      if (IoAdams.Enabled != d.Enabled || IoAdams.Settings.IpAddress != d.IpAddress || IoAdams.Settings.Port != d.Port || (int)IoAdams.Settings.ProtocolType != (int)d.Protocol || IoAdams.Settings.Unit != d.Unit)
      {
        IoAdams.Disconnect();
        IoAdams.Enabled = d.Enabled;
        if (IoAdams.Enabled)
        {
          IoAdams.Settings.IpAddress = d.IpAddress;
          IoAdams.Settings.Port = d.Port;
          IoAdams.Settings.ProtocolType = (System.Net.Sockets.ProtocolType)d.Protocol;
          IoAdams.Settings.Unit = d.Unit;
          IoAdams.Connect();
        }
      }
      resetLogoutTimer();
    }
    private void btnChangeDir_Click(object sender, RoutedEventArgs e)
    {
      var d = new System.Windows.Forms.FolderBrowserDialog() { Description = "Change working file directory.", ShowNewFolderButton = true, SelectedPath = Properties.Settings.Default.ProgramPath };// , RootFolder = Environment.SpecialFolder.ApplicationData
      if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        Properties.Settings.Default.ProgramPath = d.SelectedPath;
        PopulateFilelists();
      }
    }
    private void FocusTextBoxOnLoad(object sender, RoutedEventArgs e)
    {
      if (!(sender is TextBox t)) return;
      t.Focus();
      t.SelectAll();
    }
    private void AddAlarm(string title, string prompt, string color1, string color2, string variableOnOk, string variableOnCancel, dynamic okValue, dynamic cancelValue)
    {
      if (string.IsNullOrEmpty(title)) { return; }
      //if (!alarms.ContainsKey(title))
      //{
      //}
      //else
      //{
      //}
      AlarmWindow a;
      if (alarms.TryGetValue(title, out a))
      {
        Dispatcher.Invoke(() =>
        {
          a.Color1 = color1;
          a.Color2 = color2;
          a.Prompt = prompt;
          a.VariableOnOk = variableOnOk;
          a.VariableOnCancel = variableOnCancel;
          a.OkValue = okValue;
          a.CancelValue = cancelValue;
        });
      }
      else
      {
        Dispatcher.Invoke(() =>
        {
          a = new AlarmWindow(title, prompt, color1, color2, variableOnOk, variableOnCancel, okValue, cancelValue);
          a.Closed += Alarm_Closed;
          alarms.Add(title, a);
          a.Show();
        });
      }
    }
    private void Alarm_Closed(object sender, EventArgs e)
    {
      //if (((AlarmWindow)sender).Result && !string.IsNullOrEmpty(((AlarmWindow)sender).Variable))
      //{
      string varName = ((AlarmWindow)sender).Result ? ((AlarmWindow)sender).VariableOnOk : ((AlarmWindow)sender).VariableOnCancel;
      try
      {
        var daVar = Vars.Where(p => p.Name == varName).FirstOrDefault();
        daVar.Value = ((AlarmWindow)sender).Result ? ((AlarmWindow)sender).OkValue : ((AlarmWindow)sender).CancelValue;
        if (daVar.Channel >= 0 && daVar.IsOutput == true)
        {
          // assign physical output
          switch (daVar.IoController)
          {
            case IoControllers.Advantech:
              switch (daVar.VarType.enumValue)
              {
                case VarType.boolType:
                  if (IoAdams != null && IoAdams.Enabled) { IoAdams.SetBit(daVar.ChannelBit, daVar.Value == true ? 1 : 0); }
                  break;
                case VarType.stringType:
                  break;
                case VarType.numberType:
                  break;
                default:
                  break;
              }
              break;
            case IoControllers.Keyence:
              switch (daVar.VarType.enumValue)
              {
                case VarType.boolType:
                  if (keyEip != null && keyEip.Enabled) { keyEip.SetBit(daVar.ChannelWord, daVar.ChannelBit, daVar.Value == true); };
                  break;
                case VarType.stringType:
                  break;
                case VarType.numberType:
                  break;
                default:
                  break;
              }
              break;
          }
        }
      }
      catch (Exception)
      {
      }
      //}
      RemoveAlarm(((AlarmWindow)sender).Title);
    }
    private void RemoveAlarm(string title)
    {
      if (alarms.TryGetValue(title, out AlarmWindow a)) { RemoveAlarm(a); }
    }
    private void RemoveAlarm(AlarmWindow alarm)
    {
      alarm.Close();
      alarms.Remove(alarm.Title);
    }
    private void ClearAlarms()
    {
      var closed = new List<string>();
      if (alarms.Any())
      {
        try
        {
          foreach (var a in alarms)
          {
            a.Value.Close();
            closed.Add(a.Value.Title);
          }
        }
        catch (Exception)
        {
        }
        foreach (var s in closed)
        {
          alarms.Remove(s);
        }
        if (alarms.Any()) { ClearAlarms(); }
      }
    }
    private void AssignLiveVars()
    {
      //LiveVars = Vars.Where(p => p.Channel >= 0);
      LiveVars = Vars.Where(p => p.IoController > 0);
      if (!LiveVars.Any()) { LiveVars = null; }
    }
    private void btnKeyence_Click(object sender, RoutedEventArgs e)
    {
      if (keyEip == null) { keyEip = new KeyenceEip(); }
      EeipConfig d = new EeipConfig() { Owner = this, Enabled = keyEip.Enabled, IpAddress = keyEip.Settings.IpAddress, Port = keyEip.Settings.Port, AssemblyIn = 100, AssemblyOut = 101 };
      d.ShowDialog();
      if (keyEip.Enabled != d.Enabled || keyEip.Settings.IpAddress != d.IpAddress || keyEip.Settings.Port != d.Port || keyEip.Settings.AssemblyIn != d.AssemblyIn || keyEip.Settings.AssemblyOut != d.AssemblyOut)
      {
        keyEip.Disconnect();
        keyEip.Enabled = d.Enabled;
        if (keyEip.Enabled)
        {
          keyEip.Settings.IpAddress = d.IpAddress;
          keyEip.Settings.Port = d.Port;
          keyEip.Settings.AssemblyIn = d.AssemblyIn;
          keyEip.Settings.AssemblyOut = d.AssemblyOut;
          keyEip.Connect();
        }
      }
      resetLogoutTimer();
    }
    private void AttemptLogin()
    {
      Login d = new Login(delayToLoginAsTicks - DateTime.Now.Ticks) { Owner = this };
      d.ShowDialog();
      if (d.DialogResult == true)
      {
        Login(d.PasswordAttempt);
      }
    }
    private void Logout(object state)
    {
      Properties.Settings.Default.Save();
      UpdateHeader("goodbye", Colors.DarkGray);
      LoggedIn = false;

      // lock form
      Dispatcher.Invoke(() =>
      {
        btnLogin.Content = "login";
        tree.AllowDrop = false;
        btnEditPwd.IsEnabled = false; btnEditPwd.Visibility = Visibility.Collapsed;
        btnPlugins.IsEnabled = false; btnPlugins.Visibility = Visibility.Collapsed;
        chkHighlight.IsEnabled = false; chkHighlight.Visibility = Visibility.Collapsed;
        btnAbort.IsEnabled = false; btnAbort.Visibility = Visibility.Collapsed;
        btnSaveAs.IsEnabled = false; btnSaveAs.Visibility = Visibility.Collapsed;
        btnDeleteBorder.IsEnabled = false; btnDeleteBorder.Visibility = Visibility.Collapsed;
        btnAdvantech.IsEnabled = false; btnAdvantech.Visibility = Visibility.Collapsed;
        btnKeyence.IsEnabled = false; btnKeyence.Visibility = Visibility.Collapsed;
        btnAddVariable.IsEnabled = false; btnAddVariable.Visibility = Visibility.Collapsed;
        SequenceOptions.IsEnabled = false; // SequenceOptions.Visibility = Visibility.Collapsed;
        seqMode.IsEnabled = false; seqMode.Visibility = Visibility.Collapsed;
        seqSetMode.IsEnabled = false; seqSetMode.Visibility = Visibility.Collapsed;
        seqCondition.IsEnabled = false; seqCondition.Visibility = Visibility.Collapsed;
        seqTimer.IsEnabled = false; seqTimer.Visibility = Visibility.Collapsed;
        seqSetVar.IsEnabled = false; seqSetVar.Visibility = Visibility.Collapsed;
        seqReturn.IsEnabled = false; seqReturn.Visibility = Visibility.Collapsed;
        seqAlarm.IsEnabled = false; seqAlarm.Visibility = Visibility.Collapsed;
        VariableOptions.IsEnabled = false; // VariableOptions.Visibility = Visibility.Collapsed;
      });
    }
    private void Login(string password)
    {
      if ((password ?? string.Empty) == (Properties.Settings.Default.Password ?? string.Empty))
      {
        btnLogin.Content = "logout";
        Properties.Settings.Default.FailedLogin = 0;
        resetLogoutTimer();
        UpdateHeader("welcome", Colors.LawnGreen);
        LoggedIn = true;
        // unlock form
        tree.AllowDrop = true;
        btnEditPwd.IsEnabled = true; btnEditPwd.Visibility = Visibility.Visible;
        btnPlugins.IsEnabled = true; btnPlugins.Visibility = Visibility.Visible;
        chkHighlight.IsEnabled = true; chkHighlight.Visibility = Visibility.Visible;
        btnAbort.IsEnabled = true; btnAbort.Visibility = Visibility.Visible;
        btnSaveAs.IsEnabled = true; btnSaveAs.Visibility = Visibility.Visible;
        btnDeleteBorder.IsEnabled = true; btnDeleteBorder.Visibility = Visibility.Visible;
        btnAdvantech.IsEnabled = true; btnAdvantech.Visibility = Visibility.Visible;
        btnKeyence.IsEnabled = true; btnKeyence.Visibility = Visibility.Visible;
        btnAddVariable.IsEnabled = true; btnAddVariable.Visibility = Visibility.Visible;
        SequenceOptions.IsEnabled = true; //SequenceOptions.Visibility = Visibility.Visible;
        btnDeleteSelectedNode.IsEnabled = true; btnDeleteSelectedNode.Visibility = Visibility.Visible;
        seqMode.IsEnabled = true; seqMode.Visibility = Visibility.Visible;
        seqSetMode.IsEnabled = true; seqSetMode.Visibility = Visibility.Visible;
        seqCondition.IsEnabled = true; seqCondition.Visibility = Visibility.Visible;
        seqTimer.IsEnabled = true; seqTimer.Visibility = Visibility.Visible;
        seqSetVar.IsEnabled = true; seqSetVar.Visibility = Visibility.Visible;
        seqReturn.IsEnabled = true; seqReturn.Visibility = Visibility.Visible;
        seqAlarm.IsEnabled = true; seqAlarm.Visibility = Visibility.Visible;
        VariableOptions.IsEnabled = true; // VariableOptions.Visibility = Visibility.Visible;
      }
      else
      {
        if (LoggedIn) { Logout(null); }
        Properties.Settings.Default.FailedLogin += 1;
        SetLoginDelay();
        UpdateHeader("failed login attempt: " + Properties.Settings.Default.FailedLogin, Colors.Orange);
      }
      Properties.Settings.Default.Save();
    }
    private void resetLogoutTimer(int interval = 60000)
    {
      if (tmrLogout == null)
      {
        tmrLogout = new System.Threading.Timer(new System.Threading.TimerCallback(Logout), null, interval, System.Threading.Timeout.Infinite);
      }
      else
      {
        tmrLogout.Change(interval, System.Threading.Timeout.Infinite);
      }
    }
    private void SetLoginDelay()
    {
      if (Properties.Settings.Default.FailedLogin < 0) { Properties.Settings.Default.FailedLogin = 0; }
      switch (Properties.Settings.Default.FailedLogin)
      {
        case 3:
          delayToLoginAsTicks = DateTime.Now.Ticks + new TimeSpan(0, 0, 10).Ticks;
          break;
        case 4:
          delayToLoginAsTicks = DateTime.Now.Ticks + new TimeSpan(0, 0, 20).Ticks;
          break;
        case 5:
          delayToLoginAsTicks = DateTime.Now.Ticks + new TimeSpan(0, 0, 45).Ticks;
          break;
        default:
          delayToLoginAsTicks = DateTime.Now.Ticks + new TimeSpan(0, 0, (Properties.Settings.Default.FailedLogin - 2) * 60).Ticks;
          break;
      }
    }
    private void Login_Click(object sender, RoutedEventArgs e)
    {
      if (LoggedIn)
      {
        Logout(null);
      }
      else
      {
        AttemptLogin();
      }
    }
    private void Edit_Click(object sender, RoutedEventArgs e)
    {
      if (LoggedIn)
      {
        var d = new SetPassword() { Owner = this };
        if (d.ShowDialog() == true)
        {
          Properties.Settings.Default.Password = d.NewPassword;
          Properties.Settings.Default.Save();
        }
        resetLogoutTimer();
      }
    }

  }
}
