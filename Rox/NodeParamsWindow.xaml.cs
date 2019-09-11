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
    /// Interaction logic for NodeParamsWindow.xaml
    /// </summary>
    public partial class NodeParamsWindow : Window
    {
        private string _nodeName;

        public string NodeName
        {
            get { return _nodeName; }
            set {txtName.Text=value; _nodeName = value; }
        }

        public NodeParamsWindow(string nodeName)
        {
            InitializeComponent();
            NodeName = nodeName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            NodeName = txtName.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void TxtName_KeyUp(object sender, KeyEventArgs e)
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
