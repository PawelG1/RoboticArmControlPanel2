using ControlPanel.Application.Interfaces;
using ControlPanel.Infrastructure.Hardware;

namespace ControlPanel.Infrastructure;

public sealed class SerialCommunicationService : ISerialCommunication
{
    private readonly SerialPortController _serialPortController;

    public SerialCommunicationService(SerialPortController port)
    {
        _serialPortController = port;
    }

    public Task<bool> SendJsonLineAsync(string jsonLine)
    { 
        return _serialPortController.WriteToSerialPort(jsonLine);
    }

    public Task<string?> SendJsonRequestAsync(string jsonLine)
    {
        return _serialPortController.SendRequestAsync(jsonLine);
    }
}
