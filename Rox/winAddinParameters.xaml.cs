using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;

namespace Rox
{
  /// <summary>
  /// Interaction logic for IOConfig.xaml
  /// </summary>
  public partial class winAddinParameters : Window
  {
    public winAddinParameters()
    {
      InitializeComponent();
      this.DataContext = this;
    }
    public ICollection<AddinContracts.IAddinConnection> connectionAddins { get; set; }
    public bool Enabled { get; set; }
    public string ConnString { get; set; }
    public AddinContracts.IAddinConnection Addin { get; set; }
    public string AddinName { get; set; }
    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      Enabled = chkEnabled.IsChecked==true;
      ConnString = txtConnString.Text;
      this.DialogResult = true;
      this.Close();
    }
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
