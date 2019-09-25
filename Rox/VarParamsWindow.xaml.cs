using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rox
{
    /// <summary>
    /// Interaction logic for VarParamsWindow.xaml
    /// </summary>
    public partial class VarParamsWindow : Window
    {
        private string _varName;

        public string VarName
        {
            get { return _varName; }
            set {txtName.Text=value; _varName = value; }
        }
        private string _varNote;

        public string VarNote
        {
            get { return _varNote; }
            set {txtName.Text=value; _varNote = value; }
        }
        public VarType VarType { get; set; }

        public VarParamsWindow() : this(null,VarType.boolType,null)
        {
        }
        public VarParamsWindow(string Name, VarType varType, string Note)
        {
            InitializeComponent();
            VarName = Name;
            VarNote = Note;
            switch (varType)
            {
                case VarType.boolType:
                    rdoBool.IsChecked = true; VarType = VarType.boolType;
                    break;
                case VarType.stringType:
                    rdoString.IsChecked = true; VarType = VarType.stringType;
                    break;
                case VarType.numberType:
                    rdoDecimal.IsChecked = true; VarType = VarType.numberType;
                    break;
                default:
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            VarName = txtName.Text;
            VarNote = txtNote.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void Txt_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter)
            {
                //var a =(e.OriginalSource as FrameworkElement).PredictFocus(FocusNavigationDirection.Up);
                //var b = (e.OriginalSource as FrameworkElement).PredictFocus(FocusNavigationDirection.Down);
                //var c = (e.OriginalSource as FrameworkElement).PredictFocus(FocusNavigationDirection.Left);
                //var d = (e.OriginalSource as FrameworkElement).PredictFocus(FocusNavigationDirection.Right);
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
    }
}
