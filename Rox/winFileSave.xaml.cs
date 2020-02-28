using System.Windows;
using System.Windows.Input;

namespace Rox
{
  /// <summary>
  /// Interaction logic for FileSavePrompt.xaml
  /// </summary>
  public partial class winFileSave : Window
  {
    private string _filename;
    public string Filename { get { return _filename; } private set { _filename = value; txtFilename.Text = value; } }
    public string Path { get; private set; }
    public winFileSave(string path, string filename)
    {
      InitializeComponent();
      if (string.IsNullOrWhiteSpace(path) || !System.IO.Directory.Exists(path) || !CanWriteToDirectory(path))
      {
        txtFilename.IsEnabled = false;
        btnOk.IsEnabled = false;
        lblInvalid.Content = "Invalid file path.";
        lblInvalid.Visibility = Visibility.Visible;
        return;
      }
      Path = path;
      Filename = filename;
    }
    private bool CanWriteToDirectory(string path)
    {
      try
      {
        System.Security.AccessControl.DirectorySecurity ds = System.IO.Directory.GetAccessControl(path);
        return true;
      }
      catch (System.Exception)
      {
        return false;
      }
    }
    private void OK_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtFilename.Text)) { return; }
      Filename = txtFilename.Text.Split('.')[0] + ".rox";
      // check if file exists
      if (System.IO.File.Exists(Path.TrimEnd('\\') + @"\" + Filename))
      {
        if (MessageBox.Show("Overwrite existing file?", "File already exists!", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
          return;
        }
      }
      this.DialogResult = true;
      this.Close();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }
    private void TxtFilename_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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
      txtFilename.Focus();
    }
  }
}
