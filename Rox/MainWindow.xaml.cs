using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum NodeTypes
        {
            General = 0,
            Mode = 1,
            Condition = 2,
            Timer = 3
        }
        class ExpandedTreeNode : TreeViewItem
        {
            internal NodeTypes NodeType { get; set; }
            internal readonly string HelperText;
            internal ExpandedTreeNode(NodeTypes nodeType)
            {
                NodeType = nodeType;
                Selected += (sender,eventargs)=>{ };
                switch(nodeType)
                {
                    case NodeTypes.Mode:
                        HelperText = "Mode. Add items into [Initialize] and\\or [Continuous].";
                        return;
                    case NodeTypes.Condition:
                        HelperText = "If-Then-Else conditional statement.";
                        return;
                    case NodeTypes.Timer:
                        HelperText = "Timer.";
                        return;
                    default:
                        HelperText = "General node. Any additional node can be added here.";
                        return;
                }
            }
            
        }
        //class ModeTreenode : ExpandedTreeNode
        //{
        //}
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
            }
            SetDefaultGuiElements();
        }
        private void UpdateHeader(string s)
        {
            txtTitle.Text = s;
        }
        private void SetDefaultGuiElements()
        {
            tree.Items.Clear();
            tree.Items.Add(new ExpandedTreeNode(NodeTypes.General)
            {
                Header = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/once.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "1st scan" }
                    }
                }
            });
            tree.Items.Add(new ExpandedTreeNode(NodeTypes.Mode)
            {
                Foreground = Brushes.White,
                IsExpanded = true,
                Header = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/mode.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Stop" }
                    }
                },
                Items = {
                    new ExpandedTreeNode(NodeTypes.General) {
                    Header = new StackPanel()
                    {
                    Orientation =Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/once.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Initialize" }
                    }
                  } },
                    new ExpandedTreeNode(NodeTypes.General) {
                    Header = new StackPanel()
                    {
                    Orientation =Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/continue.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Continuous" }
                    }
                  } } }
            });
            tree.Items.Add(new ExpandedTreeNode(NodeTypes.Mode)
            {
                Foreground = Brushes.White,
                IsExpanded = true,
                Header = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/mode.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Auto" }
                    }
                },
                Items = {
                    new ExpandedTreeNode(NodeTypes.General) {
                    Header = new StackPanel()
                    {
                    Orientation =Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/once.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Initialize" }
                    }
                  } },
                    new ExpandedTreeNode(NodeTypes.General) {
                    Header = new StackPanel()
                    {
                    Orientation =Orientation.Horizontal,
                    Children = {
                        new Image() {Width=20,Height=20,Source= new BitmapImage(new Uri(@"pack://application:,,,/include/continue.png", UriKind.Absolute)) },
                        new Label() { Foreground = Brushes.White, Content = "Continuous" }
                    }
                  } } }
            });
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
    }
}
