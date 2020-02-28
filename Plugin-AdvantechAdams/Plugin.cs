using System.ComponentModel;
using AddinContracts;

namespace Plugin_AdvantechAdams
{
  public class Connection : IAddinConnection
  {
    public Connection()
    {
    }
    public event PropertyChangedEventHandler PropertyChanged;

    public string Name { get { return "Advantech"; } }
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; }

  }
}
