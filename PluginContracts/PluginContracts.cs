using System.ComponentModel;
using System.AddIn;
using System.AddIn.Contract;
using System.AddIn.Pipeline;

namespace PluginContracts
{
  [AddInContract]
  public interface IPluginContract : INotifyPropertyChanged
  {
    string Name { get; }
    object GetSettingsView();
    System.Collections.Generic.ICollection<IDevice> Devices { get; }
  }
  [AddInContract]
  public interface IDevice
  {
    string Name { get; set; }
  }
}
