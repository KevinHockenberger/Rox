using Advantech.Adam;
using System;

namespace Rox
{
  internal class IoAdams
  {
    public DateTime? LastFailedReconnectTime { get; private set; }
    public TimeSpan MinReconnectTime { get; set; } = new TimeSpan(0, 0, 60);
    public bool IsConnected { get; set; }
    private volatile bool _reading;
    private bool[] _inputVals;
    private bool[] _outputVals;
    private class myAdamsSocket : AdamSocket
    {

    }
    public ConnectionSettings Settings { get; set; } = new ConnectionSettings();

    public class ConnectionSettings
    {
      public string IpAddress { get; set; } = "127.0.0.1";
      public System.Net.Sockets.ProtocolType ProtocolType { get; set; } = System.Net.Sockets.ProtocolType.Tcp;
      public int Port { get; set; } = 502;
      public SupportedAdvantechUnits Unit { get; set; } = SupportedAdvantechUnits.Adam6000;
    }
    myAdamsSocket device;
    private myAdamsSocket AdamSocket { get; set; }
    internal IoAdams(ConnectionSettings Settings)
    {
      if (Settings == null) { return; }
      try
      {
        this.Settings.IpAddress = Settings.IpAddress;
        this.Settings.ProtocolType = Settings.ProtocolType;
        this.Settings.Port = Settings.Port;
        this.Settings.Unit = Settings.Unit;
        device = new myAdamsSocket() { AdamSeriesType = (AdamType)this.Settings.Unit };
        //if (device.Connect(this.Settings.IpAddress, this.Settings.ProtocolType, this.Settings.Port))
        //{
        //  IsConnected = true;
        //}
        //else
        //{
        //  LastFailedReconnectTime = DateTime.Now;
        //  IsConnected = false;
        //}
      }
      catch (System.Exception)
      {
        LastFailedReconnectTime = DateTime.Now;
        Disconnect();
      }
    }
    public bool Connect()
    {
      if (device.Connected) { return IsConnected = true; }
      //if (LastFailedReconnectTime == null) { LastFailedReconnectTime = DateTime.Now; }
      if (LastFailedReconnectTime == null || MinReconnectTime.TotalSeconds > 0 && (DateTime.Now - (LastFailedReconnectTime.Value)).TotalSeconds > MinReconnectTime.TotalSeconds)
      {
        device.Disconnect();
        if (device.Connect(Settings.IpAddress, Settings.ProtocolType, Settings.Port))
        {
          LastFailedReconnectTime = null;
          return IsConnected = true;
        }
        else
        {
          LastFailedReconnectTime = DateTime.Now;
          return IsConnected = false;
        }
      }
      else
      {
        return IsConnected = false;
      }
    }
    public void Disconnect()
    {
      device.Disconnect();
      IsConnected = false;
      LastFailedReconnectTime = null;
    }
    public bool[] GetInputs()
    {
      if (!_reading && IsConnected)
      {
        _reading = true;
        if (!device.Modbus().ReadCoilStatus(1, 8, out _inputVals)) { IsConnected = false; }
        _reading = false;
      }
      return _inputVals;
    }
    public bool[] GetOutputs()
    {
      if (!_reading && IsConnected)
      {
        _reading = true;
        if (!device.Modbus().ReadCoilStatus(17, 8, out _outputVals)) { IsConnected = false; }
        _reading = false;
      }
      return _outputVals;
    }
  }
}
