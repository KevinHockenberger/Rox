using System.ComponentModel;
using AddinContracts;

namespace Plugin_KeyenceEIP
{
  public class Connection : IAddinConnection
  {
    public Connection()
    {
    }

    public string Name { get { return "Keyence"; } }

    public string ConnectionString { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
