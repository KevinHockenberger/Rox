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
    private Settings _settings = new Settings();

    public class Settings
    {
      public string IpAddress { get; set; } = "127.0.0.1";
      public System.Net.Sockets.ProtocolType ProtocolType { get; set; } = System.Net.Sockets.ProtocolType.Tcp;
      public int Port { get; set; } = 502;
    }
    myAdamsSocket device;
    private myAdamsSocket AdamSocket { get; set; }
    internal IoAdams(Settings Settings)
    {
      if (Settings == null) { return; }
      try
      {
        _settings.IpAddress = Settings.IpAddress;
        _settings.ProtocolType = Settings.ProtocolType;
        _settings.Port = Settings.Port;
        device = new myAdamsSocket() { AdamSeriesType = AdamType.Adam6000 };
        //IsConnected = device.Connect(Settings.IpAddress, Settings.ProtocolType, Settings.Port);
        if (device.Connect(_settings.IpAddress, _settings.ProtocolType, _settings.Port))
        {
          IsConnected = true;
        }
        else
        {
          LastFailedReconnectTime = DateTime.Now;
          IsConnected = false;
        }
      }
      catch (System.Exception)
      {
        LastFailedReconnectTime = DateTime.Now;
        device.Disconnect();
        IsConnected = false;
      }
    }
    public bool Reconnect()
    {
      if (device.Connected) { return IsConnected = true; }
      if (LastFailedReconnectTime == null) { LastFailedReconnectTime = DateTime.Now; }
      if (MinReconnectTime.TotalSeconds > 0 && (DateTime.Now - (LastFailedReconnectTime.Value)).TotalSeconds > MinReconnectTime.TotalSeconds)
      {
        device.Disconnect();
        if (device.Connect(_settings.IpAddress, _settings.ProtocolType, _settings.Port))
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
