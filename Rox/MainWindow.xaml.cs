using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Rox
{
    //public class DelegateCommand<T> : System.Windows.Input.ICommand where T : class
    //{
    //    private readonly Predicate<T> _canExecute;
    //    private readonly Action<T> _execute;
    //    public DelegateCommand(Action<T> execute) : this(execute, null)
    //    {
    //    }
    //    public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
    //    {
    //        _execute = execute;
    //        _canExecute = canExecute;
    //    }
    //    public bool CanExecute(object parameter)
    //    {
    //        if (_canExecute == null) { return true; }
    //        return _canExecute((T)parameter);
    //    }

    //    public void Execute(object parameter)
    //    {
    //        _execute((T)parameter);
    //    }
    //    public event EventHandler CanExecuteChanged;
    //    public void RaiseCanExecuteChanged()
    //    {
    //        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    //    }
    //}
    public interface INode
    {
        NodeTypes NodeType { get; }
        string Name { get; set; }
        string Description();
        List<INode> Items { get; set; }
        List<NodeTypes> AllowedNodes { get; }
    }
    public enum NodeTypes
    {
        General = 0,
        Mode = 1,
        Condition = 2,
        Timer = 3,
        Initialized = 4,
        Continuous = 5,
        ConditionTrue = 6,
        ConditionFalse = 7,
    }
    public class IteMode : INode
    {
        public NodeTypes NodeType { get; } = NodeTypes.Mode;
        public string Name { get; set; }
        public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
        public List<INode> Items { get; set; } = new List<INode>();
        public string Description() { return "{ MODE } A mode can be created to easily abort a running mode and start new sequencing. Stop and Auto modes will run with Start/Stop button. No items can be added directly. Add items to Initialize or Continuous branches."; }
        public IteMode(string name)
        {
            Name = name;
        }
    }
    public class IteFirstScan : INode
    {
        public NodeTypes NodeType { get; } = NodeTypes.Initialized;
        public string Name { get; set; }
        public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
        public List<INode> Items { get; set; } = new List<INode>();
        public string Description() { return "{ First Scan branch } This sequence will run one time when the program is loaded and can accept any item."; }
        public IteFirstScan(string name)
        {
            Name = name;
        }
    }
    public class IteIntialize : INode
    {
        public NodeTypes NodeType { get; } = NodeTypes.Initialized;
        public string Name { get; set; }
        public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
        public List<INode> Items { get; set; } = new List<INode>();
        public string Description() { return "{ Initialize branch } This sequence will only run one time when the mode is first started and before the continuous branch. This node can accept any item."; }
        public IteIntialize(string name)
        {
            Name = name;
        }
    }
    public class IteContinuous : INode
    {
        public NodeTypes NodeType { get; } = NodeTypes.Continuous;
        public string Name { get; set; }
        public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
        public List<INode> Items { get; set; } = new List<INode>();
        public string Description() { return "{ Continuous branch } This sequence will run repeatedly after the initalize branch has completed and while the mode is active."; }
        public IteContinuous(string name)
        {
            Name = name;
        }
    }
    public class IteCondition : INode
    {
        public NodeTypes NodeType { get; } = NodeTypes.Condition;
        public string Name { get; set; }
        public List<NodeTypes> AllowedNodes { get; } = new List<NodeTypes>() { };
        public List<INode> Items { get; set; } = new List<INode>();
        public string Description() { return "{ Condition } This will evaluate a statement to true or false and run the appropriate sequence. Add items to True / False branches."; }
        public IteCondition(string name)
        {
            Name = name;
        }
    }
    public class IteTrue : INode
    {
        public NodeTypes NodeType { get; } = NodeTypes.ConditionTrue;
        public string Name { get; set; }
        public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
        public List<INode> Items { get; set; } = new List<INode>();
        public string Description() { return "{ Condition True branch } This sequence will run while the condition is true."; }
        public IteTrue(string name)
        {
            Name = name;
        }
    }
    public class IteFalse : INode
    {
        public NodeTypes NodeType { get; } = NodeTypes.ConditionFalse;
        public string Name { get; set; }
        public List<NodeTypes> AllowedNodes { get; } = Rox.MainWindow.SequenceNodes;
        public List<INode> Items { get; set; } = new List<INode>();
        public string Description() { return "{ Condition False branch } This sequence will run while the condition is not true."; }
        public IteFalse(string name)
        {
            Name = name;
        }
    }
    public class IteNodeViewModel : INotifyPropertyChanged
    {
        //public DelegateCommand<string> ButtonClickCommand
        //{
        //    get { return _clickCommand; }
        //}
        //private readonly DelegateCommand<string> _clickCommand;
        private INode Node { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isSelected;
        private bool _isExpanded = true;
        public IteNodeViewModel Parent { get; set; }
        public Collection<IteNodeViewModel> Items { get; set; }
        public IteNodeViewModel(INode node) : this(node, null) { }
        public IteNodeViewModel(INode node, IteNodeViewModel parent)
        {
            Node = node;
            Parent = parent;
            Items = new Collection<IteNodeViewModel>();
            foreach (var item in node.Items)
            {
                switch (item.NodeType)
                {
                    case NodeTypes.General:
                        //Items.Add(new IteNodeViewModel(item));
                        break;
                    case NodeTypes.Mode:
                        Items.Add(new IteMODE_VM(item));
                        break;
                    case NodeTypes.Condition:
                        Items.Add(new IteNodeViewModel(item));
                        break;
                    case NodeTypes.Timer:
                        Items.Add(new IteTIMER_VM(item));
                        break;
                    case NodeTypes.Initialized:
                        Items.Add(new IteFIRST_VM(item));
                        break;
                    case NodeTypes.Continuous:
                        Items.Add(new IteCONTINUOUS_VM(item));
                        break;
                    case NodeTypes.ConditionTrue:
                        Items.Add(new IteTRUE_VM(item));
                        break;
                    case NodeTypes.ConditionFalse:
                        Items.Add(new IteFALSE_VM(item));
                        break;
                    default:
                        break;
                }
            }
            //Items = new Collection<IteNodeViewModel>((from item in node.Items select new IteNodeViewModel(item, this)).ToList<IteNodeViewModel>());
            //_clickCommand = new DelegateCommand<string>(
            //           (s) =>
            //           {
            //               Console.WriteLine(node.Description());
            //           }, //Execute
            //           (s) => { return true; } //CanExecute
            //           );
        }
        public string Name
        {
            get { return Node.Name; }
            set
            {
                if (value != Node.Name)
                {
                    Node.Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
        public bool IsLocked { get; set; }
        public string Description
        {
            get { return Node.Description(); }
        }
        public NodeTypes NodeType
        {
            get { return Node.NodeType; }
        }
        public List<NodeTypes> AllowedNodes
        {
            get { return Node.AllowedNodes; }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
                // Expand all the way up to the root.
                if (_isExpanded && Parent != null) { Parent.IsExpanded = true; }
            }
        }
    }
    public class IteCONTINUOUS_VM : IteNodeViewModel
    {
        public IteCONTINUOUS_VM(INode node) : base(node) { }
    }
    public class IteMODE_VM : IteNodeViewModel
    {
        public IteMODE_VM(INode node) : base(node) { }
    }
    public class IteCONDITION_VM : IteNodeViewModel
    {
        public IteCONDITION_VM(INode node) : base(node) { }
    }
    public class IteTRUE_VM : IteNodeViewModel
    {
        public IteTRUE_VM(INode node) : base(node) { }
    }
    public class IteFALSE_VM : IteNodeViewModel
    {
        public IteFALSE_VM(INode node) : base(node) { }
    }
    public class IteFIRST_VM : IteNodeViewModel
    {
        public IteFIRST_VM(INode node) : base(node) { }
    }
    public class IteTIMER_VM : IteNodeViewModel
    {
        public IteTIMER_VM(INode node) : base(node) { }
    }
    public interface IVariable : INotifyPropertyChanged
    {
        string Name { get; set; }
        string Note { get; set; }
    }
    public class vString : IVariable
    {
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        private string _note;
        public string Note
        {
            get { return _note; }
            set
            {
                if (value != _note)
                {
                    _note = value;
                    NotifyPropertyChanged("Note");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
    public class vBool : IVariable
    {
        private bool _value;
        public bool Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        private string _note;
        public string Note
        {
            get { return _note; }
            set
            {
                if (value != _note)
                {
                    _note = value;
                    NotifyPropertyChanged("Note");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
    public class vNumber : IVariable
    {
        private decimal _value;
        public decimal Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        private string _note;
        public string Note
        {
            get { return _note; }
            set
            {
                if (value != _note)
                {
                    _note = value;
                    NotifyPropertyChanged("Note");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
    public enum VarType
    {
        boolType,
        stringType,
        numberType
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BindingList<IVariable> Vars = new BindingList<IVariable>();
        private static SolidColorBrush treeBackground = new SolidColorBrush(Color.FromRgb(41, 41, 41));
        private static SolidColorBrush treeBackgroundAllowDrop = new SolidColorBrush(Color.FromRgb(71, 125, 30));
        public static List<NodeTypes> SequenceNodes = new List<NodeTypes>() { NodeTypes.Condition, NodeTypes.General, NodeTypes.Timer };
        public List<IteNodeViewModel> Modes;// : INotifyPropertyChanged;
        public IteNodeViewModel selectedNode { get; set; }
        System.Threading.Timer closeMnu;
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
                }
                else
                {
                    // stopped
                    btnToggleRun.Content = "START";
                    brdrRun.Background = new SolidColorBrush(Colors.LightGreen);
                    btnToggleRun.BorderBrush = new SolidColorBrush(Colors.LightGreen);
                    btnToggleRun.Foreground = new SolidColorBrush(Colors.LawnGreen);
                }
                _running = value;
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
            gridMenu.Visibility = Visibility.Visible;
            gridFiles.Visibility = Visibility.Collapsed;
            btnSaveAs.Visibility = Visibility.Collapsed;
            ApplySettings();
            txtVer.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            closeMnu = new System.Threading.Timer(new System.Threading.TimerCallback(closeMenu), null, 10000, System.Threading.Timeout.Infinite);
            PopulateFilelist();
            //System.Threading.Thread.Sleep(12000); // << to test splash screen
            resetForm(true);
            listVars.ItemsSource = Vars;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
        }
        private void SaveSettings()
        {
            Properties.Settings.Default.LastWindowState = this.WindowState;
            Properties.Settings.Default.LastWindowRect = this.RestoreBounds;
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
                gridMenu.Visibility = Visibility.Collapsed;
                gridFiles.Visibility = Visibility.Collapsed;
                btnLoadFileText.Text = "select file";
            });
            //if (this.Dispatcher.CheckAccess())
            //{
            //    //gridMenu.Visibility = Visibility.Collapsed;
            //    //gridFiles.Visibility = Visibility.Collapsed;
            //    //btnLoadFileText.Text = "select file";
            //}
            //else
            //{
            //    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(closeFiles));
            //}
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
            //var d = new SaveAs(loadedFile) { Owner = this };
            //if (d.ShowDialog() == true)
            //{
            //    if (SharedMethods.SaveJobAs(pluginsCore, d.Filename))
            //    {
            //        FileLoad(d.Filename);
            //        PopulateFilelist();
            //    }
            //}
        }
        private void FileLoad(string filename)
        {
            FileUnload(false);
        }
        private void FileUnload(bool unselectProgram)
        {
            btnSaveAs.Visibility = Visibility.Collapsed;
            Running = null;
            resetForm(unselectProgram);
        }
        private void resetForm(bool unselectProgram = false)
        {
            loadedFile = null;
            UpdateHeader("Load a program.");
            if (unselectProgram)
            {
                listFiles.SelectedItem = null;
                listRecentFiles.SelectedItem = null;
                SetDefaultGuiElements();
            }
        }
        private void UpdateHeader(string s)
        {
            txtTitle.Text = s;
        }
        private void SetDefaultGuiElements()
        {
            Modes = new List<IteNodeViewModel>()
            {
                new IteFIRST_VM( new IteFirstScan("1st scan")),
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
        private void PopulateFilelist()
        {
            //listRecentFiles.Items.Clear();
            //foreach (var item in SharedMethods.GetVisionFiles(true, false).Take(3))
            //{
            //    listRecentFiles.Items.Add(item);
            //}
            //listFiles.ItemsSource = SharedMethods.GetVisionFiles();
            //if (listRecentFiles.Items.Count > 0)
            //{
            //    listRecentFiles.SelectedItem = listRecentFiles.Items[0];
            //}
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
                case NodeTypes.Timer:
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
                        SetNodeOptionsPanel_Simple(s.Name);
                    }
                    break;
            }
            e.Handled = true;
        }
        private void txtNodeName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (selectedNode == (IteNodeViewModel)((IteNodeViewModel)tree.SelectedItem))
                {
                    var T = (TextBox)sender;
                    selectedNode.Name = T.Text;
                    //tree.SelectedItem(T.Text);
                }
            }
        }
        private void ClearNodeOptionsPanel()
        {
            NodeOptions.ContentTemplate = null;
        }
        private void SetNodeOptionsPanel_Basic(string Name)
        {
            NodeOptions.ContentTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("NodeOptionsBasic");
        }
        private void SetNodeOptionsPanel_Simple(string Name)
        {
            NodeOptions.ContentTemplate = (DataTemplate)Application.Current.MainWindow.FindResource("NodeOptionsSimple");
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
                DataObject dragData = new DataObject("iteNode", new IteCondition("Condition") { Items = { new IteTrue("True"), new IteFalse("False") } });
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
            Console.WriteLine(sender != e.Source);
            if (e.Data.GetDataPresent("iteNode"))
            {
                Console.WriteLine(2);
                // test if node can be dropped here
                if (((INode)e.Data.GetData("iteNode")).NodeType == NodeTypes.Mode)
                {
                    Console.WriteLine(3);
                    e.Effects = DragDropEffects.Copy;
                    tree.Background = treeBackgroundAllowDrop;
                    Console.WriteLine(4);
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
            var d = new VarParamsWindow();
            d.ShowDialog();
            if(d.DialogResult==true)
            {
                Vars.Add(new vBool { Name = d.VarName, Note = d.VarNote, Value = false });

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
    }
}
