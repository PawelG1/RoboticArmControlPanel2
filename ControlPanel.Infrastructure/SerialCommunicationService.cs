using ControlPanel.Application.Interfaces;
using ControlPanel.Infrastructure.Hardware;

namespace ControlPanel.Infrastructure;

public sealed class SerialCommunicationService : ISerialCommunicationService
{
    private readonly SerialPortController _serialPortController;
    public event EventHandler<string>? MessageReceived;


    public SerialCommunicationService(SerialPortController port)
    {
        _serialPortController = port;
        _serialPortController.OnMessageReceived += OnMessageReceivedFromPort;
    }

    public void ConfigureConnection(string portName)
    {
        _serialPortController.SetPortName(portName);
        _serialPortController.ConfigureSerialPort();
    }

    public void Connect()
    {
        _serialPortController.OpenSerialPort();
    }
    
    public void Disconnect()
    {
        _serialPortController.CloseSerialPort();
    }

    public bool GetConnectionStatus()
    {
        return _serialPortController.GetPortStatus();
    }

    public string[] GetAvailableSerialPorts()
    {
        return SerialPortController.GetSerialPorts();
    }

    public Task<bool> SendJsonLineAsync(string jsonLine)
    { 
        return _serialPortController.WriteToSerialPort(jsonLine);
    }

    public Task<string?> SendJsonRequestAsync(string jsonLine)
    {
        return _serialPortController.SendRequestAsync(jsonLine);
    }

    public string GetSelectedPortName()
    {
        return _serialPortController.GetPortName();
    }

    private void OnMessageReceivedFromPort(object? sender, string message)
    {
        MessageReceived?.Invoke(this, message);
    }
}
