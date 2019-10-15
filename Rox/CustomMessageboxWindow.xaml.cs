using System.Windows;

namespace Rox
{
  /// <summary>
  /// Interaction logic for NodeParamsWindow.xaml
  /// </summary>
  public partial class CustomMessageboxWindow : Window
  {
    public new string Title { get { return lblTitle.Content.ToString(); } private set { lblTitle.Content = value; } }
    public string Caption { get { return txtPrompt.Text; } private set { txtPrompt.Text = value; } }

    public CustomMessageboxWindow(string title, string caption, MessageBoxButton messageBoxButton)
    {
      InitializeComponent();
      this.Title = title;
      this.Caption = caption;
      btnOk.Visibility = Visibility.Collapsed;
      btnCancel.Visibility = Visibility.Collapsed;
      btnYes.Visibility = Visibility.Collapsed;
      btnNo.Visibility = Visibility.Collapsed;
      switch (messageBoxButton)
      {
        case MessageBoxButton.OK:
          btnOk.Visibility = Visibility.Visible;
          btnCancel.Visibility = Visibility.Collapsed;
          btnYes.Visibility = Visibility.Collapsed;
          btnNo.Visibility = Visibility.Collapsed;
          break;
        case MessageBoxButton.OKCancel:
          btnOk.Visibility = Visibility.Visible;
          btnCancel.Visibility = Visibility.Visible;
          btnYes.Visibility = Visibility.Collapsed;
          btnNo.Visibility = Visibility.Collapsed;
          break;
        case MessageBoxButton.YesNoCancel:
          btnYes.Visibility = Visibility.Visible;
          btnNo.Visibility = Visibility.Visible;
          btnCancel.Visibility = Visibility.Visible;
          btnOk.Visibility = Visibility.Collapsed;
          break;
        case MessageBoxButton.YesNo:
          btnYes.Visibility = Visibility.Visible;
          btnNo.Visibility = Visibility.Visible;
          btnOk.Visibility = Visibility.Collapsed;
          btnCancel.Visibility = Visibility.Collapsed;
          break;
        default:
          break;
      }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }

  }
}
