using System;
using System.Windows;
using System.Windows.Input;

namespace Rox
{
  /// <summary>
  /// Interaction logic for Login.xaml
  /// </summary>
  public partial class Login : Window
  {
    System.Threading.Timer enableOk;
    private const int tickInterval = 100;
    private TimeSpan enableAfter;
    public string PasswordAttempt { get; set; }
    public Login(long EnableAfterTicks)
    {
      InitializeComponent();
      this.DataContext = this;
      btnOk.IsEnabled = false;
      enableAfter = new TimeSpan(EnableAfterTicks);
      if (enableAfter.TotalSeconds <= 0)
      {
        btnOk.IsEnabled = true;
      }
      else
      {
        enableOk = new System.Threading.Timer(new System.Threading.TimerCallback(testEnableOkButton), null, tickInterval, tickInterval);
      }
    }

    private void testEnableOkButton(object state)
    {
      enableAfter = enableAfter.Subtract(new TimeSpan(0, 0, 0, 0, tickInterval));
      Dispatcher.Invoke(() =>
      {
        if (enableAfter.TotalSeconds <= 0)
        {
          btnOk.Content = btnOkContent;
          btnOk.IsEnabled = true;
          enableOk.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }
        else
        {
          btnOk.Content = enableAfter.ToString(@"hh\:mm\:ss");
          enableOk.Change(tickInterval, tickInterval);
        }
      });
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      //enableOk.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      PasswordAttempt = txtPassword.Password;
      this.DialogResult = true;
      this.Close();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      if (enableOk != null) { try { enableOk.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); } catch (Exception) { } }
      this.DialogResult = false;
      this.Close();
    }
    private void Txt_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        e.Handled = true;
        if ((e.OriginalSource as FrameworkElement).PredictFocus(FocusNavigationDirection.Down) == btnOk)
        {
          OK_Click(btnOk, null);
        }
        else
        {
          (e.OriginalSource as FrameworkElement).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
      }
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      txtPassword.Focus();
    }
  }
}
