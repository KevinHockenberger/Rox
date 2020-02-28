using System.AddIn.Pipeline;
using System.Collections.Generic;
using System.ComponentModel;

namespace AddinContracts
{
  [AddInContract]
  public interface IAddinConnection : INotifyPropertyChanged
  {
    string Name { get; }
    string ConnectionString { get; set; }
    bool Enabled { get; set; }
  }
}
