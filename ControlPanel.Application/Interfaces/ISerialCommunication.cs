namespace ControlPanel.Application.Interfaces;

public interface ISerialCommunication
{
    event EventHandler<string> MessageReceived;

    /// <summary> Sends one line of JSON and reads one line of response (request/response over serial). </summary>
    Task<string?> SendJsonRequestAsync(string jsonLine);
    /// <summary>
    /// sends one line of JSON without reading response (one-way command over serial). Use for commands that don't require confirmation or response.
    /// </summary>
    Task<bool> SendJsonLineAsync(string jsonLine);
    void ConfigureConnection(string portName);
    string[] GetAvailableSerialPorts();
    string GetSelectedPortName();
    void Connect();
    void Disconnect();
    bool GetConnectionStatus();
}
