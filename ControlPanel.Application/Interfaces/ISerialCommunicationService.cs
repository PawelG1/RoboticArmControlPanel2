using ControlPanel.Application.DTOs.SerialCommands;
using System;
using System.Threading.Tasks;

namespace ControlPanel.Application.Interfaces;

public interface ISerialCommunicationService
{
    event EventHandler<BinaryMessage> FrameReceived;

    /// <summary> Sends a binary frame and awaits a matching ACK or response. </summary>
    Task<BinaryMessage?> SendRequestAsync(byte messageType, byte[] payload);
    
    /// <summary> Sends a binary frame without waiting for response. </summary>
    Task<bool> SendCommandAsync(byte messageType, byte[] payload);

    void ConfigureConnection(string portName);
    string[] GetAvailableSerialPorts();
    string GetSelectedPortName();
    void Connect();
    void Disconnect();
    bool GetConnectionStatus();
}
