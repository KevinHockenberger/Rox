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
    private bool highlight;
    private System.Threading.Tasks.Task Seq;
    BindingList<Variable> Vars = new BindingList<Variable>();
    private static SolidColorBrush treeBackground = new SolidColorBrush(Color.FromRgb(41, 41, 41));
    private static SolidColorBrush treeBackgroundAllowDrop = new SolidColorBrush(Color.FromRgb(71, 125, 30));
    private static SolidColorBrush textboxBackground = new SolidColorBrush(Color.FromRgb(241, 241, 241));
    //private static SolidColorBrush processedNodeBackground = new SolidColorBrush(Color.FromRgb(0, 107, 21));
    private static SolidColorBrush errorNodeBackground = new SolidColorBrush(Colors.Red);
    private static SolidColorBrush processedNodeBackground = new SolidColorBrush(Color.FromRgb(88, 94, 45));
    private static SolidColorBrush unprocessedNodeBackground = new SolidColorBrush(Colors.Black);
    private static SolidColorBrush trueNodeBackground = new SolidColorBrush(Color.FromRgb(0, 107, 21));
    private static SolidColorBrush falseNodeBackground = new SolidColorBrush(Color.FromRgb(107, 0, 66));
    //private static SolidColorBrush textboxBackgroundAllowDrop = new SolidColorBrush(Color.FromRgb(71, 125, 30));
    public static List<NodeTypes> SequenceNodes = new List<NodeTypes>() { NodeTypes.Condition, NodeTypes.General, NodeTypes.Timer };
    public List<IteNodeViewModel> Modes;// : INotifyPropertyChanged;
    public IteNodeViewModel selectedNode { get; set; }
    System.Threading.Timer closeMnu;
    System.Threading.Timer clrHeader;
    public string processingMode { get; set; } = null; // initialize processingMode and curMode to different values so initialize sequence will run on startup
    public string curMode { get; set; } = string.Empty;
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
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      //App.splashScreen.LoadComplete();
    }
    private void Window_Initialized(object sender, EventArgs e)
    {
      ApplySettings();
      txtVer.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      closeMnu = new System.Threading.Timer(new System.Threading.TimerCallback(closeMenu), null, 10000, System.Threading.Timeout.Infinite);
      PopulateFilelists();
      resetForm(true);
      listVars.ItemsSource = Vars;
      Paused = true;
    }
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      Paused = true; // redundant, yes
      FileUnload(false);
      SaveSettings();
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
      Properties.Settings.Default.ProgramPath = System.AppDomain.CurrentDomain.BaseDirectory;
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

      if (((ListBox)sender).SelectedItem.ToString() == "> unload <")
      {
        // unload file
        btnLoadFileText.Text = "select file";
        FileUnload(false);
        fileToBeLoaded = null;
      }
      else
      {
        fileToBeLoaded = (string)((ListBox)sender).SelectedItem;
        btnLoadFileText.Text = string.Format("load [{0}]", fileToBeLoaded);
      }
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
          SaveFile(d.Filename, true);
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
          //Console.WriteLine(" ------ START READING ------- ");
          ParseFile(reader, fileRead);
          Modes = fileRead;
          tree.DataContext = null;
          tree.DataContext = new { Modes };
        }
        btnLoadFileText.Text = string.Format("Save [{0}]", filename);
        Properties.Settings.Default.MruFiles.Insert(0, filename); PopulateRecentFilelist();
        loadedFile = filename;
        UpdateHeader(string.Format("{0} loaded.", filename));
        btnUnloadFile.Visibility = Visibility.Visible;
        processingMode = null;
        curMode = string.Empty;
        Paused = false;
        Running = false;
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
      //Console.WriteLine("type: {0}, name: {1}, value: {2}, attr(name): {3}", reader.NodeType, reader.Name, reader.Value, reader.GetAttribute("name"));
      IteNodeViewModel curNode = null;
      while (reader.Read())
      {
        //Console.WriteLine("type: {0}, name: {1}, value: {2}, attr(name): {3}", reader.NodeType, reader.Name, reader.Value, reader.GetAttribute("name"));
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == xmlTag_Var)
          {
            var v = new Variable() { Name = reader.GetAttribute("name"), Note = reader.GetAttribute("note") };
            switch (GetVarTypeFromString(reader.GetAttribute("type")))
            {
              case VarType.boolType:
                v.Value = bool.TryParse(reader.GetAttribute("val"), out bool b) ? b : false;
                break;
              case VarType.stringType:
                v.Value = reader.GetAttribute("val");
                break;
              case VarType.numberType:
                v.Value = decimal.TryParse(reader.GetAttribute("val"), out decimal d) ? d : 0;
                break;
              default:
                break;
            }
            v.UsersLastValue = v.Value;
            Vars.Add(v);
          }
          // ----------------------------------------------------- MODE
          if (reader.Name == xmlTag_Mode)
          {
            var name = reader.GetAttribute("name");
            curNode = new IteMODE_VM(new IteMode(name)); // { Items = { new IteIntialize("Initialize"), new IteContinuous("Continuous") } }
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
              curNode = new IteFIRST_VM(new IteFirstScan("1st scan"));
              modes.Add(curNode);
            }
            else
            {
              var subNode = new IteFIRST_VM(new IteIntialize(reader.GetAttribute("name"))) { Parent = curNode };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONTINUOUS
          else if (reader.Name == xmlTag_Continuous)
          {
            if (curNode == null)
            {
              curNode = new IteCONTINUOUS_VM(new IteContinuous("Always"));
              modes.Add(curNode);
            }
            else
            {
              var subNode = new IteCONTINUOUS_VM(new IteContinuous(reader.GetAttribute("name"))) { Parent = curNode };
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
              { Parent = curNode };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL TRUE1
          else if (reader.Name == xmlTag_ConditionTrue1)
          {
            if (curNode != null)
            {
              var subNode = new IteTRUE1_VM(new IteTrue1(reader.GetAttribute("name"))) { Parent = curNode };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL TRUE
          else if (reader.Name == xmlTag_ConditionTrue)
          {
            if (curNode != null)
            {
              var subNode = new IteTRUE_VM(new IteTrue(reader.GetAttribute("name"))) { Parent = curNode };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL FALSE1
          else if (reader.Name == xmlTag_ConditionFalse1)
          {
            if (curNode != null)
            {
              var subNode = new IteFALSE1_VM(new IteFalse1(reader.GetAttribute("name"))) { Parent = curNode };
              curNode.Items.Add(subNode);
              curNode = subNode;
            }
          }
          // ----------------------------------------------------- CONDITIONAL FALSE
          else if (reader.Name == xmlTag_ConditionFalse)
          {
            if (curNode != null)
            {
              var subNode = new IteFALSE_VM(new IteFalse(reader.GetAttribute("name"))) { Parent = curNode };
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
              { Parent = curNode };
              curNode.Items.Add(subNode);
              curNode = subNode;
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
    private void FileUnload(bool unselectProgram)
    {
      Paused = true;
      resetCloseMenuTimer();
      btnUnloadFile.Visibility = Visibility.Collapsed;
      btnSaveAs.Visibility = Visibility.Collapsed;
      Running = null;
      resetForm(unselectProgram);
    }
    private void toggleFiles(object sender, RoutedEventArgs e)
    {
      resetCloseMenuTimer();
      gridFiles.Visibility = gridFiles.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }
    private bool SaveFile(string filespec, bool overwriteIfExist)
    {
      if (!string.IsNullOrWhiteSpace(filespec))
      {
        if (System.IO.File.Exists(filespec) && !overwriteIfExist) { return false; }
        if (!Modes.Any()) { return false; }
        using (var sw = new System.IO.StreamWriter(filespec, false))
        {
          sw.WriteLine("<rox ver='{0}'>", txtVer.Text);
          foreach (var mode in Modes)
          {
            AppendNodeData(sw, mode);
          }
          sw.WriteLine();
          foreach (var v in Vars)
          {
            sw.WriteLine("<var name='{0}' type='{1}' val='{2}' note='{3}' />", (v.Name ?? string.Empty).Replace('\'', '"'), v.VarType, (v.Value.ToString() ?? string.Empty).Replace('\'', '"'), (v.Note ?? string.Empty).Replace('\'', '"'));
          }
          sw.WriteLine("</rox>");
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
          sw.Write("<general name='{0}' type='{1}'>", n.Name, n.NodeType);
          AppendChildren(sw, n);
          break;
        case NodeTypes.Mode:
          sw.Write("<{0} name='{1}' type='{2}'>", xmlTag_Mode, n.Name, n.NodeType);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Mode);
          break;
        case NodeTypes.Condition:
          var p = (IteCondition)n.Node;
          sw.Write("<{0} name='{1}' varname='{2}' method='{3}' desired='{4}'>", xmlTag_Condition, p.Name, p.VariableName, p.EvalMethodText, p.DesiredValue);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Condition);
          break;
        case NodeTypes.Timer:
          var t = (IteTimer)n.Node;
          sw.Write("<{0} name='{1}' type='{2}' i='{3}'>", xmlTag_Timer, n.Name, n.NodeType, t.Interval);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Timer);
          break;
        case NodeTypes.Initialized:
          sw.Write("<{0} name='{1}' type='{2}'>", xmlTag_Init, n.Name, n.NodeType);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Init);
          break;
        case NodeTypes.Continuous:
          sw.Write("<{0} name='{1}' type='{2}'>", xmlTag_Continuous, n.Name, n.NodeType);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_Continuous);
          break;
        case NodeTypes.ConditionTrue1:
          sw.Write("<{0} name='{1}' type='{2}'>", xmlTag_ConditionTrue1, n.Name, n.NodeType);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionTrue1);
          break;
        case NodeTypes.ConditionTrue:
          sw.Write("<{0} name='{1}' type='{2}'>", xmlTag_ConditionTrue, n.Name, n.NodeType);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionTrue);
          break;
        case NodeTypes.ConditionFalse1:
          sw.Write("<{0} name='{1}' type='{2}'>", xmlTag_ConditionFalse1, n.Name, n.NodeType);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionFalse1);
          break;
        case NodeTypes.ConditionFalse:
          sw.Write("<{0} name='{1}' type='{2}'>", xmlTag_ConditionFalse, n.Name, n.NodeType);
          AppendChildren(sw, n);
          sw.Write("</{0}>", xmlTag_ConditionFalse);
          break;
        default:
          sw.Write("<unknown name='{0}' type='{1}'>", n.Name, n.NodeType);
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
      UpdateHeader("Load a program.");
      btnUnloadFile.Visibility = Visibility.Collapsed;
      if (unselectProgram)
      {
        listFiles.SelectedItem = null;
        listRecentFiles.SelectedItem = null;
        SetDefaultGuiElements();
      }
    }
    private void UpdateHeader(object o)
    {
      UpdateHeader((o ?? string.Empty).ToString());
    }
    private void UpdateHeader(string s)
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
        txtTitle.Text = s ?? string.Empty;
      });
    }
    private void SetDefaultGuiElements()
    {
      Modes = new List<IteNodeViewModel>()
            {
                new IteFIRST_VM( new IteFirstScan("1st scan")),
                new IteCONTINUOUS_VM( new IteContinuous("Always")),
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
            SetNodeOptionsPanel_Basic(s.Name);
          }
          break;
        case NodeTypes.Condition:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel_Condition(s.Name);
          }
          break;
        case NodeTypes.Timer:
          ClearNodeOptionsPanel();
          if (!s.IsLocked)
          {
            SetNodeOptionsPanel_Timer(s.Name);
          }
          break;
      }
      e.Handled = true;
    }
    private void txtNodeName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      //if (e.Key == System.Windows.Input.Key.Enter)
      //{
      //  if (selectedNode == (IteNodeViewModel)((IteNodeViewModel)tree.SelectedItem))
      //  {
      //    var T = (TextBox)sender;
      //    selectedNode.Name = T.Text;
      //    //tree.SelectedItem(T.Text);
      //  }
      //}
    }
    private void ClearNodeOptionsPanel()
    {
      NodeOptions.ContentTemplate = null;
    }
    private void SetNodeOptionsPanel_Basic(string Name)
    {
      NodeOptions.ContentTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("NodeOptionsBasic");
    }
    private void SetNodeOptionsPanel_Condition(string Name)
    {
      NodeOptions.ContentTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("NodeOptionsCondition");
    }
    private void SetNodeOptionsPanel_Timer(string Name)
    {
      NodeOptions.ContentTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("NodeOptionsTimer");
    }
    private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      startDragPoint = e.GetPosition(null);
    }
    private void Mode_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      Vector diff = startDragPoint - e.GetPosition(null);
      if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
      {
        // Initialize the drag & drop operation
        DataObject dragData = new DataObject("iteNode", new IteMode("Mode") { Items = { new IteIntialize("Initialize"), new IteContinuous("Continuous") } });
        DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
      }
    }
    private void Condition_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      Vector diff = startDragPoint - e.GetPosition(null);
      if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
      {
        // Initialize the drag & drop operation
        DataObject dragData = new DataObject("iteNode", new IteCondition("Condition") { Items = { new IteTrue1("0→1"), new IteTrue("True"), new IteFalse1("1→0"), new IteFalse("False") } });
        DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
      }
    }
    private void Timer_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      Vector diff = startDragPoint - e.GetPosition(null);
      if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
      {
        // Initialize the drag & drop operation
        DataObject dragData = new DataObject("iteNode", new IteTimer("Timer") { Items = { new IteTrue("Expired"), new IteFalse("Waiting") } });
        DragDrop.DoDragDrop((StackPanel)sender, dragData, DragDropEffects.Move);
      }
    }
    private void tvi_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent("iteNode") && sender != e.Source)
      {
        // test if node can be dropped here
        var target = (TreeViewItem)sender;
        var dc = (IteNodeViewModel)target.DataContext;
        if (dc.AllowedNodes.Contains(((INode)e.Data.GetData("iteNode")).NodeType))
        {
          e.Effects = DragDropEffects.Copy;
          target.Background = treeBackgroundAllowDrop;
        }
        else
        {
          e.Effects = DragDropEffects.None;
        }
      }
      else
      {
        e.Effects = DragDropEffects.None;
      }
      e.Handled = true;
    }
    private void tvi_DragOver(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent("iteNode") && sender != e.Source)
      {
        e.Effects = DragDropEffects.Copy;
      }
      else
      {
        e.Effects = DragDropEffects.None;
        e.Handled = true;
      }
    }
    private void tvi_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent("iteNode"))
      {
        // test if node can be dropped here
        var target = (TreeViewItem)sender;
        var dc = (IteNodeViewModel)target.DataContext;
        var source = (INode)e.Data.GetData("iteNode");
        if (dc.AllowedNodes.Contains(source.NodeType))
        {
          try
          {
            ((IteNodeViewModel)tree.SelectedItem).IsSelected = false;
          }
          catch (Exception)
          {
          }
          var success = false;
          var T = source.GetType();
          IteNodeViewModel N = null;
          if (T == typeof(IteMode))
          {
            N = new IteMODE_VM(source) { IsSelected = true };
            Modes.Add(N); success = true;
          }
          else if (T == typeof(IteCondition))
          {
            N = new IteCONDITION_VM(source) { IsSelected = true };
            dc.Items.Add(N); success = true;
            //Modes.Add(new IteCONDITION_VM(source)); success = true;
          }
          else if (T == typeof(IteTimer))
          {
            N = new IteTIMER_VM(source) { IsSelected = true };
            dc.Items.Add(N); success = true;
            //Modes.Add(new IteCONDITION_VM(source)); success = true;
          }
          else
          {
            N = new IteNodeViewModel(null);
          }
          if (success)
          {
            tree.DataContext = null;
            tree.DataContext = new { Modes };
          }
        }
      }
      e.Handled = true;
      ((TreeViewItem)sender).Background = treeBackground;
    }
    private void btnReset_Click(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show("Delete the existing sequence and start over?", "Start over", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
      {
        SetDefaultGuiElements();
      }
    }
    private void tvi_DragLeave(object sender, DragEventArgs e)
    {
      ((TreeViewItem)sender).Background = treeBackground;
      //e.Handled = true;
    }
    private void Tree_DragEnter(object sender, DragEventArgs e)
    {
      //Console.WriteLine(sender != e.Source);
      if (e.Data.GetDataPresent("iteNode"))
      {
        //Console.WriteLine(2);
        // test if node can be dropped here
        if (((INode)e.Data.GetData("iteNode")).NodeType == NodeTypes.Mode)
        {
          //Console.WriteLine(3);
          e.Effects = DragDropEffects.Copy;
          tree.Background = treeBackgroundAllowDrop;
          //Console.WriteLine(4);
        }
        else
        {
          e.Effects = DragDropEffects.None;
        }
      }
      else
      {
        e.Effects = DragDropEffects.None;
      }
      e.Handled = true;
    }
    private void Tree_Drop(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent("iteNode"))
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
      var d = new VarParamsWindow() { Owner = this };
      d.ShowDialog();
      if (d.DialogResult == true && !string.IsNullOrWhiteSpace(d.VarName))
      {
        if (Vars.Where(p => p.Name == d.VarName).Any()) { return; }
        Vars.Add(new Variable { Name = d.VarName, Note = d.VarNote, Value = d.VarValue, UsersLastValue = d.VarValue });
      }
      AutoSizeVarColumns();
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
      var n = ((ListViewItem)sender).Content as Variable;

      var d = new VarParamsWindow() { Owner = this, VarName = n.Name, VarNote = n.Note, VarType = (VarType)n.VarType.Value };
      d.VarValue = n.Value;
      d.ShowDialog();
      if (d.DialogResult == true && !string.IsNullOrWhiteSpace(d.VarName))
      {
        n.Name = d.VarName;
        n.Note = d.VarNote;
        n.Value = d.VarValue;
      }
      //AutoSizeVarColumns();
    }
    private void Logic_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (((ComboBox)sender).SelectedIndex < 0) { return; }
      var l = ((ComboBoxItem)((ComboBox)sender).SelectedValue).Content.ToString();
      //Console.WriteLine(l);
      var o = (selectedNode.Node as IteCondition);
      o.EvalMethodText = l;
      o.EvalMethod = GetEvalMethodFromString(l);
      SetAndEvaluateLogicStatement(o);
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
      switch (s.ToLower())
      {
        default:
          return VarType.boolType;
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
      var o = (selectedNode.Node as IteCondition);
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
        //Console.WriteLine("Compare = {0} | desired value = {1} | current value = {2} | typeof desired = {3} | typeof current = {4}"
        //  , o.EvalMethod(o.DesiredValue, CurrentValue)
        //  , o.DesiredValue
        //  , CurrentValue
        //  , o.DesiredValue.GetType()
        //  , CurrentValue.GetType()
        //  );
        o.Evaluation = o.EvalMethod(o.DesiredValue, CurrentValue);
      }
      catch (Exception)
      {
      }
    }
    private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      Vector diff = startDragPoint - e.GetPosition(null);
      if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
      {
        // Initialize the drag & drop operation
        DataObject dragData = new DataObject("iteVar", (Variable)((ListViewItem)sender).Content);
        DragDrop.DoDragDrop((ListViewItem)sender, dragData, DragDropEffects.Move);
      }
    }
    private void Var_Drop(object sender, DragEventArgs e)
    {
      //Console.WriteLine("drop");
      if (e.Data.GetDataPresent("iteVar"))
      {
        var source = (Variable)e.Data.GetData("iteVar");
        ((TextBox)sender).Text = source.Name;
        var o = (selectedNode.Node as IteCondition);
        o.VariableName = source.Name;
        SetAndEvaluateLogicStatement(o);
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
    }
    private void RunSequence()
    {
      while (!Paused)
      {
        //System.Threading.Thread.Sleep(1);
        //await Task.Delay(1);
        bool modeChanged = curMode != processingMode;
        processingMode = curMode;
        foreach (var mode in Modes)
        {
          //if (highlight) { ResetHighlight(mode); }
          try
          {
            if ((mode.NodeType == NodeTypes.Continuous) || (curMode == mode.Name) || (mode.NodeType == NodeTypes.Initialized && modeChanged && processingMode == string.Empty)) // Always, current mode, 1st scan
            {
              ProcessValidNodeSequence(mode, modeChanged);
            }
            else
            {
              ProcessInvalidNodeSequence(mode);
              //Console.WriteLine("SKIP: " + mode.Name);
            }
          }
          catch (Exception)
          {
          }
        }
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
          //Console.WriteLine(node.Name + " - NOT IMPLEMENTED");
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
      }
    }
    private void ProcessValidNodeSequence(IteNodeViewModel node, bool initialize)
    {
      //Console.WriteLine("PROCESSING: " + node.Name);
      switch (node.NodeType)
      {
        default: // ############################################################################################ DEFAULT
          if (highlight) { node.Background = processedNodeBackground; }
          //Console.WriteLine(node.Name + " - NOT IMPLEMENTED");
          foreach (var sub in node.Items)
          {
            ProcessValidNodeSequence(sub, initialize);
          }
          break;
        case NodeTypes.Condition: // ############################################################################################ CONDITION
          // get current value
          var v = Vars.Where(p => p.Name == ((IteCondition)node.Node).VariableName).FirstOrDefault();
          if (v!=null)
          {
          if (highlight) { node.Background = processedNodeBackground; }
          //var thisNode = node;
          try
          {
            var eval = ((IteCondition)node.Node).EvalMethod(v.Value, ((IteCondition)node.Node).DesiredValue);
            //Console.WriteLine((node.Parent == null ? "root" : node.Parent.Name) + "." + node.Name + " condition. eval = " + eval);
            if (eval)
            {
              if (v.Value != v.UsersLastValue)
              {
                // initialized true
                Console.WriteLine("{0} - {1}", v.Value, v.UsersLastValue);
                v.UsersLastValue = v.Value;
                //node = node.Items.First(p => p.NodeType == NodeTypes.ConditionTrue);
                if (highlight) { node.Items[0].Background = trueNodeBackground; }
                foreach (var sub in node.Items[0].Items)
                {
                  ProcessValidNodeSequence(sub, initialize);
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
                  ProcessValidNodeSequence(sub, initialize);
                }
                ProcessInvalidNodeSequence(node.Items[0]);
                ProcessInvalidNodeSequence(node.Items[2]);
                ProcessInvalidNodeSequence(node.Items[3]);
              }
            }
            else
            {
              if (v.Value != v.UsersLastValue)
              {
                // initialized false
                Console.WriteLine("{0} - {1}", v.Value, v.UsersLastValue);
                v.UsersLastValue = v.Value;
                if (highlight) { node.Items[2].Background = falseNodeBackground; }
                foreach (var sub in node.Items[2].Items)
                {
                  ProcessValidNodeSequence(sub, initialize);
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
                  ProcessValidNodeSequence(sub, initialize);
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
            //Console.WriteLine((node.Parent == null ? "root" : node.Parent.Name) + "." + node.Name + " diff-up.");
            foreach (var sub in node.Items)
            {
              ProcessValidNodeSequence(sub, initialize);
            }
          }
          else
          {
            ProcessInvalidNodeSequence(node);
          }
          break;
        case NodeTypes.Timer: // ############################################################################################ TIMER
          var t = ((IteTimer)node.Node);
          t.UpdateTime();
          if (t.Expired)
          {
            if (highlight) { node.Items[0].Background = trueNodeBackground; }
            foreach (var sub in node.Items[0].Items)
            {
              ProcessValidNodeSequence(sub, initialize);
            }
            ProcessInvalidNodeSequence(node.Items[1]);
          }
          else
          {
            if (highlight) { node.Items[1].Background = trueNodeBackground; }
            foreach (var sub in node.Items[1].Items)
            {
              ProcessValidNodeSequence(sub, initialize);
            }
            ProcessInvalidNodeSequence(node.Items[0]);
          }
          break;
      }
    }
    private void ChkHighlight_Checked(object sender, RoutedEventArgs e)
    {
      highlight = true;
    }
    private void ChkHighlight_Unchecked(object sender, RoutedEventArgs e)
    {
      highlight = false;
      System.Threading.Thread.Sleep(250);
      foreach (var mode in Modes)
      {
        ResetHighlight(mode);
      }
    }
  }
}
