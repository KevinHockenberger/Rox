using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rox
{
        public enum NodeTypes
        {
            General = 0,
            Mode = 1,
            Condition = 2,
            Timer = 3
        }
        public class IfeRoutedEventArgs : RoutedEventArgs
        {
            public NodeTypes nodeType;
            public string NodeInfo;
        }
        public class IfeTreeNode : TreeViewItem
        {
            public event EventHandler<IfeRoutedEventArgs> NodeSelected;
            public virtual void OnSelected(IfeRoutedEventArgs e) { NodeSelected?.Invoke(this, e); }
            internal NodeTypes NodeType { get; set; }
            internal readonly string HelperText;
            public string NodeText { get; set; }
            public int uId { get; set; }
            //static IfeTreeNode()
            //{
            //    DefaultStyleKeyProperty.OverrideMetadata(typeof(IfeTreeNode), new FrameworkPropertyMetadata(typeof(IfeTreeNode)));
            //}
            public IfeTreeNode() { }
            public IfeTreeNode(string name, NodeTypes nodeType)
            {

                NodeText = name;
                NodeType = nodeType;
                switch (nodeType)
                {
                    case NodeTypes.Mode:
                        HelperText = name + " Mode.\r" +
                            "  \u2022 Items can only be added to [Initialize] and\\or [Continuous]. \r" +
                            "  \u2022 The Initialize sequence will only run one time when the mode is first started. \r" +
                            "  \u2022 The Continuous sequence will run repeatedly until the mode is no longer active.";
                        break;
                    case NodeTypes.Condition:
                        HelperText = "If-Then-Else conditional statement.";
                        break;
                    case NodeTypes.Timer:
                        HelperText = "Timer.";
                        break;
                    default:
                        HelperText = name + " general node.\r" +
                            "  \u2022 Most items are allowed here.";
                        break;
                }
                Selected += (sender, eventargs) =>
                {
                    OnSelected(new IfeRoutedEventArgs() { nodeType = nodeType, NodeInfo = HelperText });
                    eventargs.Handled = true;
                };
                //Style = (Style)FindResource("TreeViewItem");
                //SetResourceReference(StyleProperty, typeof(IfeTreeNode));
            }
        }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

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
            tree.Items.Clear();
            tree.Items.Add(GetGeneralNode("1st scan"));
            tree.Items.Add(GetBasicNode("Stop"));
            tree.Items.Add(GetBasicNode("Auto"));
            tree.Items.Add(GetModeNode("Stop"));
            tree.Items.Add(GetModeNode("Auto"));
        }
        private IfeTreeNode GetGeneralNode(string nodeName)
        {
            var ret = new IfeTreeNode(nodeName, NodeTypes.General)
            {
                Header = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/once.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = nodeName }
                    }
                }
            };
            ret.NodeSelected += new EventHandler<IfeRoutedEventArgs>(NodeSelectedEvent);
            return ret;
        }
        private IfeTreeNode GetModeNode(string modeName)
        {
            var ret = new IfeTreeNode(modeName, NodeTypes.Mode)
            {
                Foreground = Brushes.White,
                IsExpanded = true,
                Header = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/mode.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = modeName }
                    }
                },
                Items = {
                    new IfeTreeNode("Initialize", NodeTypes.General) {
                    Header = new StackPanel()
                    {
                    Orientation =Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/once.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Initialize" }
                    }
                  } },
                    new IfeTreeNode("Continuous", NodeTypes.General) {
                    Header = new StackPanel()
                    {
                    Orientation =Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/continue.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Continuous" }
                    }
                  } } }
            };
            ret.NodeSelected += new EventHandler<IfeRoutedEventArgs>(NodeSelectedEvent);
            foreach (var item in ret.Items)
            {
                ((IfeTreeNode)item).NodeSelected += new EventHandler<IfeRoutedEventArgs>(NodeSelectedEvent);
            }
            return ret;
        }
        private TreeViewItem GetBasicNode(string modeName)
        {
            var ret = new TreeViewItem() { Header = "abc", Items = { new TreeViewItem() { Header = "123" } } };
            return ret;
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
        private void NodeSelectedEvent(object sender, IfeRoutedEventArgs eventArgs)
        {
            //Dispatcher.Invoke(() => {
            SetHelperText(eventArgs.NodeInfo);
            SetAvailableAddButtons(eventArgs.nodeType);
            //});
        }
        private void SetHelperText(string t)
        {
            txtSelectedNodeInfo.Text = t;
        }
        private void SetAvailableAddButtons(NodeTypes t)
        {
            switch (t)
            {
                case NodeTypes.General:
                    break;
                case NodeTypes.Mode:
                    break;
                case NodeTypes.Condition:
                    break;
                case NodeTypes.Timer:
                    break;
                default:
                    break;
            }
        }

        private void AddMode_Click(object sender, RoutedEventArgs e)
        {
            NodeParamsWindow f = new NodeParamsWindow(null);
            if (f.ShowDialog() == true)
            {
                Console.WriteLine(f.NodeName);
                if (!string.IsNullOrWhiteSpace(f.NodeName))
                {
                    tree.Items.Add(GetModeNode(f.NodeName));
                }
            }
        }
    }
}
