using ControlPanel.Application.Interfaces;
using ControlPanel.Infrastructure.Hardware;

namespace ControlPanel.Infrastructure;
            
public sealed class SerialCommunicationService : ISerialCommunicationService
{
    private readonly SerialPortController _serialPortController;
    private ushort _sequenceCounter = 1;
    private readonly SemaphoreSlim _requestGate = new(1, 1);
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ushort, TaskCompletionSource<Hardware.Protocol.BinaryFrame>> _pendingRequests = new();

    public event EventHandler<ControlPanel.Application.DTOs.SerialCommands.BinaryMessage>? FrameReceived;

    public SerialCommunicationService(SerialPortController port)
    {
        _serialPortController = port;
        _serialPortController.OnFrameReceived += OnFrameReceivedFromPort;
    }

    public void ConfigureConnection(string portName)
    {
        _serialPortController.SetPortName(portName);
        _serialPortController.ConfigureSerialPort();
    }

    public void Connect() => _serialPortController.OpenSerialPort();
    
    public void Disconnect() => _serialPortController.CloseSerialPort();

    public bool GetConnectionStatus() => _serialPortController.GetPortStatus();

    public string[] GetAvailableSerialPorts() => SerialPortController.GetSerialPorts();

    public string GetSelectedPortName() => _serialPortController.GetPortName();

    public async Task<bool> SendCommandAsync(byte messageType, byte[] payload)
    {
        var frame = new Hardware.Protocol.BinaryFrame
        {
            MessageType = messageType,
            Sequence = GetNextSequence(),
            Payload = payload
        };
        return await _serialPortController.WriteFrameAsync(frame);
    }

    public async Task<ControlPanel.Application.DTOs.SerialCommands.BinaryMessage?> SendRequestAsync(byte messageType, byte[] payload)
    {
        await _requestGate.WaitAsync();
        try
        {
            ushort seq = GetNextSequence();
            var tcs = new TaskCompletionSource<Hardware.Protocol.BinaryFrame>();
            _pendingRequests[seq] = tcs;

            var frame = new Hardware.Protocol.BinaryFrame
            {
                MessageType = messageType,
                Sequence = seq,
                Payload = payload
            };

            if (!await _serialPortController.WriteFrameAsync(frame))
            {
                _pendingRequests.TryRemove(seq, out _);
                return null;
            }

            // Await with timeout
            using var cts = new CancellationTokenSource(2000);
            using var reg = cts.Token.Register(() => tcs.TrySetCanceled());

            try
            {
                var result = await tcs.Task;
                return new ControlPanel.Application.DTOs.SerialCommands.BinaryMessage(result.MessageType, result.Payload);
            }
            catch (TaskCanceledException)
            {
                return null; // Timeout
            }
            finally
            {
                _pendingRequests.TryRemove(seq, out _);
            }
        }
        finally
        {
            _requestGate.Release();
        }
    }

    private ushort GetNextSequence()
    {
        ushort seq = _sequenceCounter;
        _sequenceCounter++;
        if (_sequenceCounter == 0) _sequenceCounter = 1; // 0 is reserved
        return seq;
    }

    private void OnFrameReceivedFromPort(object? sender, Hardware.Protocol.BinaryFrame frame)
    {
        // If it's an ACK (0x80) or ERROR (0x81), try to complete a pending request
        if (frame.MessageType == 0x80 || frame.MessageType == 0x81)
        {
            if (_pendingRequests.TryGetValue(frame.Sequence, out var tcs))
            {
                tcs.TrySetResult(frame);
            }
        }

        FrameReceived?.Invoke(this, new ControlPanel.Application.DTOs.SerialCommands.BinaryMessage(frame.MessageType, frame.Payload));
    }
}
