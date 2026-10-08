using System.IO.Ports;
using System.Text;
using System.Text.Json;


namespace ControlPanel.Infrastructure.Hardware
{
    /// <summary> kontrakt na rozmiar i ew. znak konca wiadomosci t.j. \n </summary>
    public enum SerialMessageFraming
    {
        /// <summary> Binary frame encoded with Consistent Overhead Byte Stuffing (COBS), separated by 0x00. </summary>
        Cobs
    }

    public class SerialPortController : IDisposable
    {
        private string _portName;
        private Int32 _baudRate;
        private Parity _parity;
        private int _dataBits = 0;
        private StopBits _stopBits;
        private Handshake _handshake;
        private int _readTimeout = 500;
        private int _writeTimeout = 500;

        private SerialPort _serialPort;
        private CancellationTokenSource? _readCts;

        public EventHandler<Protocol.BinaryFrame>? OnFrameReceived { get; set; }

        public SerialPortController(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake = Handshake.None)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _handshake = handshake;

            _serialPort = new SerialPort();
        }

        public void Dispose()
        {
            _readCts?.Cancel();
            _readCts?.Dispose();
            _serialPort.Dispose();
        }

        public void ConfigureSerialPort()
        {
            _serialPort.PortName = _portName;
            _serialPort.BaudRate = _baudRate;
            _serialPort.Parity = _parity;
            _serialPort.DataBits = _dataBits;
            _serialPort.StopBits = _stopBits;
            _serialPort.Handshake = _handshake;
            _serialPort.ReadTimeout = _readTimeout;
            _serialPort.WriteTimeout = _writeTimeout;

            _serialPort.DtrEnable = true;
            _serialPort.RtsEnable = true;
        }

        public void SetPortName(string portName)
        {
            if (string.IsNullOrEmpty(portName) || !portName.ToLower().Contains("com"))
            {
                throw new ArgumentException($"Selected port name is incorrect: {portName}");
            }
            if (_serialPort.IsOpen)
                throw new AccessViolationException($"Selected port is already open {portName}");
            _portName = portName;
        }

        public string GetPortName() => _serialPort.PortName;
        public bool GetPortStatus() => _serialPort.IsOpen;
        public void SetReadTimeout(int timeout) => _readTimeout = timeout;
        public void SetWriteTimeout(int timeout) => _serialPort.WriteTimeout = timeout;
        public static string[] GetSerialPorts() => SerialPort.GetPortNames();

        public bool OpenSerialPort()
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    _serialPort.DiscardInBuffer();
                    _readCts = new CancellationTokenSource();
                    Task.Run(() => ReadLoop(_readCts.Token));
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening serial port: {ex.Message}");
                return false;
            }
        }

        public bool CloseSerialPort()
        {
            try
            {
                _readCts?.Cancel();
                _readCts?.Dispose();
                _readCts = null;
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing serial port: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> WriteFrameAsync(Protocol.BinaryFrame frame)
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    Console.WriteLine("Serial port is not open.");
                    return false;
                }

                byte[] rawFrame = frame.ToBytes();
                byte[] encoded = Protocol.CobsCodec.Encode(rawFrame);

                await _serialPort.BaseStream.WriteAsync(encoded, 0, encoded.Length);
                await _serialPort.BaseStream.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing frame: {ex.Message}");
                return false;
            }
        }

        private async Task ReadLoop(CancellationToken ct)
        {
            var cobsBuffer = new List<byte>();
            byte[] readBuf = new byte[1024];

            while(!ct.IsCancellationRequested && _serialPort.IsOpen)
            {
                try
                {
                    int bytesToRead = _serialPort.BytesToRead;
                    if (bytesToRead == 0)
                    {
                        await Task.Delay(10, ct);
                        continue;
                    }

                    int n = _serialPort.Read(readBuf, 0, Math.Min(bytesToRead, readBuf.Length));
                    for (int i = 0; i < n; i++)
                    {
                        byte b = readBuf[i];
                        if (b == 0x00)
                        {
                            if (cobsBuffer.Count > 0)
                            {
                                byte[] decoded = Protocol.CobsCodec.Decode(cobsBuffer.ToArray());
                                cobsBuffer.Clear();

                                var frame = Protocol.BinaryFrame.Parse(decoded);
                                if (frame != null)
                                {
                                    OnFrameReceived?.Invoke(this, frame);
                                }
                            }
                        }
                        else
                        {
                            // Avoid huge memory allocations from malformed unbounded data
                            if (cobsBuffer.Count < 2048)
                            {
                                cobsBuffer.Add(b);
                            }
                            else
                            {
                                cobsBuffer.Clear(); // Buffer overflow, drop malformed frame
                            }
                        }
                    }
                }
                catch (TimeoutException) { }
                catch (OperationCanceledException) { break; }
                catch (IOException) { break; }
                catch (InvalidOperationException) { break; } // Port closed
            }
        }

    }
}
