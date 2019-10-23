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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rox
{
  /// <summary>
  /// Interaction logic for ucAdams.xaml
  /// </summary>
  public partial class ucAdams : UserControl
  {
    public ucAdams()
    {
      InitializeComponent();
      this.DataContext = this;
    }
    public string IpAddress { get; set; }
    public int Port { get; set; }
  }
}
