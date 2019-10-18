using System.Windows;
using System.Windows.Input;

namespace Rox
{
  /// <summary>
  /// Interaction logic for VarParamsWindow.xaml
  /// </summary>
  public partial class VarParamsWindow : Window
  {
    private dynamic _varValue;
    public dynamic VarValue
    {
      get { return _varValue; }
      set
      {
        txtValue.Text = value.ToString();
        if (VarType == VarType.boolType)
        {
          try
          {
            if ((bool)value == true)
            { rdoValTrue.IsChecked = true; }
            else
            { rdoValFalse.IsChecked = true; }
          }
          catch (System.Exception)
          {
          }
        }
        _varValue = value;
      }
    }
    private string _varName;
    public string VarName
    {
      get { return _varName; }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          rdoBool.IsEnabled = false;
          rdoDecimal.IsEnabled = false;
          rdoString.IsEnabled = false;
        }
        txtName.Text = value;
        _varName = value;
      }
    }
    private string LocalVarName
    {
      get { return _varName; }
      set
      {
        txtName.Text = value;
        _varName = value;
      }
    }
    private string _varNote;
    public string VarNote
    {
      get { return _varNote; }
      set { txtNote.Text = value; _varNote = value; }
    }
    private VarType _vartype;
    public VarType VarType
    {
      get { return _vartype; }
      set
      {
        //if (_vartype != value)
        //{
        _vartype = value;
        switch (value)
        {
          case VarType.boolType:
            rdoBool.IsChecked = true;
            gValkey.Visibility = Visibility.Collapsed;
            gValrdo.Visibility = Visibility.Visible;
            VarValue = false;
            break;
          case VarType.stringType:
            rdoString.IsChecked = true;
            gValkey.Visibility = Visibility.Visible;
            gValrdo.Visibility = Visibility.Collapsed;
            VarValue = string.Empty;
            break;
          case VarType.numberType:
            rdoDecimal.IsChecked = true;
            gValkey.Visibility = Visibility.Visible;
            gValrdo.Visibility = Visibility.Collapsed;
            VarValue = 0;
            break;
          default:
            break;
        }
        //}
      }
    }
    private short _channel = -1;
    public short Channel
    {
      get { return _channel; }
      set { if (value >= 0) { _channel = value; txtChannel.Text = Channel.ToString(); } }
    }
    public bool? IsOutput
    {
      get { return cmbIO.Text == "Output" ? (bool?)true : cmbIO.Text == "Input" ? (bool?)false : null; }
      set { cmbIO.Text = value == true ? "Output" : value == false ? "Input" : ""; }
    }
    public VarParamsWindow() : this(null, VarType.boolType, null, false, null, -1)
    {
    }
    public VarParamsWindow(string Name, VarType varType, string Note, object Value, bool? IsOutput, short Channel)
    {
      InitializeComponent();
      VarName = Name;
      VarNote = Note;
      VarValue = Value;
      switch (varType)
      {
        default:
          rdoBool.IsChecked = true; VarType = VarType.boolType;
          break;
        case VarType.stringType:
          rdoString.IsChecked = true; VarType = VarType.stringType;
          break;
        case VarType.numberType:
          rdoDecimal.IsChecked = true; VarType = VarType.numberType;
          break;
      }
      this.IsOutput = IsOutput;
      this.Channel = Channel;
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = false;
      this.Close();
    }
    private void OK_Click(object sender, RoutedEventArgs e)
    {
      LocalVarName = txtName.Text;
      VarNote = txtNote.Text;
      if (VarType == VarType.stringType)
      {
        VarValue = txtValue.Text;
      }
      else if (VarType == VarType.numberType)
      {
        VarValue = decimal.TryParse(txtValue.Text, out var d) ? d : 0;
      }
      Channel = short.TryParse(txtChannel.Text, out short s) ? s : (short)-1;
      this.DialogResult = true;
      this.Close();
    }
    private void Txt_KeyUp(object sender, KeyEventArgs e)
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
    private void RdoVarType_Checked(object sender, RoutedEventArgs e)
    {
      if (rdoBool.IsChecked == true)
      {
        VarType = VarType.boolType;
      }
      else if (rdoDecimal.IsChecked == true)
      {
        VarType = VarType.numberType;
      }
      else if (rdoString.IsChecked == true)
      {
        VarType = VarType.stringType;
      }
    }
    private void TxtValue_KeyUp(object sender, KeyEventArgs e)
    {
      Txt_KeyUp(sender, e);
    }
    private void RdoVal_Checked(object sender, RoutedEventArgs e)
    {
      if (VarType == VarType.boolType)
      {
        VarValue = rdoValTrue.IsChecked == true;
      }
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      txtName.Focus();
    }
    private void Txt_GotFocus(object sender, RoutedEventArgs e)
    {
      ((System.Windows.Controls.TextBox)sender).SelectAll();
    }

    private void TxtChannel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
      //Channel =short.TryParse( txtChannel.Text, out short s)?s:(short)-1;
    }
  }
}
