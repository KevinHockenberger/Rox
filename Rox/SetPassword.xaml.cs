using System.Windows;
using System.Windows.Input;

namespace Rox
{
  /// <summary>
  /// Interaction logic for SetPassword.xaml
  /// </summary>
  public partial class SetPassword : Window
  {
    public string NewPassword { get; set; }
    public SetPassword()
    {
      InitializeComponent();
      this.DataContext = this;
      btnOk.IsEnabled = false;
    }
    private void OK_Click(object sender, RoutedEventArgs e)
    {
      if (txtCurrent.Password == Properties.Settings.Default.Password && txtNew.Password == txtConfirm.Password)
      {
        NewPassword=txtNew.Password;
        this.DialogResult = true;
        this.Close();
      }
      else
      {
        System.Windows.Forms.MessageBox.Show("Password does not match");
      }
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
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
      ValidateInput();
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      txtCurrent.Focus();
    }
    private void ValidateInput()
    {
      if (txtNew.Password == txtConfirm.Password && txtCurrent.Password==(Properties.Settings.Default.Password??string.Empty) ) // 
      {
        btnOk.IsEnabled = true;
      }
      else
      {
        btnOk.IsEnabled = false;
      }
    }
  }
}
