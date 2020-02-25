using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Rox
{
  /// <summary>
  /// Interaction logic for AlarmWindow.xaml
  /// </summary>
  public partial class AlarmWindow : Window
  {
    public bool IsClosed { get; private set; }
    public bool Result { get; private set; }
    public new string Title { get { return lblTitle.Text.ToString(); } set { lblTitle.Text = value; } }
    public string Prompt { get { return lblPrompt.Text.ToString(); } set { lblPrompt.Text = value; } }
    private Color color1 { get; set; }
    private Color color2 { get; set; }
    public string Color1
    {
      get { return color1.ToString(); }
      set
      {
        try
        {
          color1 = (Color)ColorConverter.ConvertFromString(value ?? "red");
        }
        catch (System.Exception)
        {
          color1 = Colors.Transparent;
        }
      }
    }
    public string Color2
    {
      get { return color2.ToString(); }
      set
      {
        try
        {
          color2 = (Color)ColorConverter.ConvertFromString(value ?? "yellow");
        }
        catch (System.Exception)
        {
          color2 = Colors.Transparent;
        }
      }
    }
    public string VariableOnOk { get; set; }
    public string VariableOnCancel { get; set; }
    public dynamic OkValue { get; set; }
    public dynamic CancelValue { get; set; }
    private byte direction = 0;
    System.Threading.Timer t;
    private void tmrCallback(object state)
    {
      try
      {
        Dispatcher.Invoke(() =>
        {
          if (!this.IsLoaded)
          {
            t.Dispose(); this.Close();
          }
          Point p1;
          Point p2;
          switch (direction)
          {
            case 0:
              direction += 1;
              p1 = new Point(0, 0);
              p2 = new Point(1, 0);
              break;
            case 1:
              direction += 1;
              p1 = new Point(0, 0);
              p2 = new Point(1, 1);
              break;
            case 2:
              direction += 1;
              p1 = new Point(0, 0);
              p2 = new Point(0, 1);
              break;
            case 3:
              direction += 1;
              p1 = new Point(1, 0);
              p2 = new Point(0, 1);
              break;
            case 4:
              direction += 1;
              p1 = new Point(1, 0);
              p2 = new Point(0, 0);
              break;
            case 5:
              direction += 1;
              p1 = new Point(1, 1);
              p2 = new Point(0, 0);
              break;
            case 6:
              direction += 1;
              p1 = new Point(0, 1);
              p2 = new Point(0, 0);
              break;
            default:
              direction = 0;
              p1 = new Point(0, 1);
              p2 = new Point(1, 0);
              break;
          }
          border.BorderBrush = new LinearGradientBrush() { StartPoint = p1, EndPoint = p2, GradientStops = new GradientStopCollection() { new GradientStop(color1, .1), new GradientStop(color2, .9) } };
        });
      }
      catch (System.Exception)
      {
      }

    }
    public AlarmWindow(string title, string prompt, string color1, string color2, string variableOnOk, string variableOnCancel, dynamic okValue, dynamic cancelValue)
    {
      InitializeComponent();
      Title = title ?? string.Empty;
      Prompt = prompt ?? string.Empty;
      VariableOnOk = variableOnOk ?? string.Empty;
      VariableOnCancel = variableOnCancel ?? string.Empty;
      OkValue = okValue;
      CancelValue = cancelValue;
      Color1 = color1; // (Color)ColorConverter.ConvertFromString(color1 ?? "red");
      Color2 = color2; // Color2 = (Color)ColorConverter.ConvertFromString(color2 ?? "yellow");
      border.BorderBrush = new LinearGradientBrush()
      {
        StartPoint = new Point(0, 0),
        EndPoint = new Point(0, 1),
        GradientStops = new GradientStopCollection() { new GradientStop(this.color1, .1), new GradientStop(this.color2, .9) }
      };
      t = new System.Threading.Timer(new System.Threading.TimerCallback(tmrCallback), null, 0, 100);
    }
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
      this.Result = true;
      this.Close();
    }
    private void Cancel_Click(object sender, RoutedEventArgs e)
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
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      t.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
      t.Dispose();
    }
    private void Window_Closed(object sender, System.EventArgs e)
    {
      IsClosed = true;
    }
  }
}
