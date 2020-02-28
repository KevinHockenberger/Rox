using System.Windows;
using System.Windows.Input;
namespace Rox
{
  /// <summary>
  /// Interaction logic for IOConfig.xaml
  /// </summary>
  public partial class delete_me_EeipConfig : Window
  {
    public delete_me_EeipConfig()
    {
      InitializeComponent();
      this.DataContext = this;
    }
    public bool Enabled { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public byte AssemblyIn { get; set; }
    public byte AssemblyOut { get; set; }
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void _MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left) { this.DragMove(); }
      if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
      {
        this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
      }
    }
  }
}
