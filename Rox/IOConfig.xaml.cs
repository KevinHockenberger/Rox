using System.Windows;
using System.Windows.Input;
namespace Rox
{
  /// <summary>
  /// Interaction logic for IOConfig.xaml
  /// </summary>
  public partial class IOConfig : Window
  {
    public IOConfig()
    {
      InitializeComponent();
      this.DataContext = this;
    }

    public string IpAddress { get; set; }
    public int Port { get; set; }
    public Rox.SupportedAdvantechUnits Unit { get; set; }
    public Rox.ProtocolTypes Protocol { get; set; }
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
