using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlPanel.Infrastructure.Hardware
{
    /// <summary> kontrakt na rozmiar i ew. znak konca wiadomosci t.j. \n </summary>
    public enum SerialMessageFraming
    {
        /// <summary> Jedna linia zakończona jak <see cref="SerialPort.NewLine"/> (domyślnie \n). Nie nadaje się do JSON z realnymi enterami w środku. </summary>
        Newline,
        /// <summary> 4 bajty długości payloadu (uint32, little-endian) + bajty UTF-8 — pełna ramka, JSON może zawierać \n. Wymaga tego samego formatu po stronie robota. </summary>
        LengthPrefixUInt32LittleEndian
    }

    public class SerialPortController
    {
        private readonly string _portName;
        private readonly Int32 _baudRate;
        private readonly Parity _parity;
        private int _dataBits = 0;
        private readonly StopBits _stopBits;
        private readonly Handshake _handshake;
        private readonly int _readTimeout = 500; //default read timeout in milliseconds
        private readonly int _writeTimeout = 500; //default write timeout in milliseconds

        private readonly SerialPort _serialPort;
        
        private readonly SemaphoreSlim _requestGate = new(1, 1);

        private const int MaxFramePayloadBytes = 1024 * 1024;

        public SerialPortController(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake = Handshake.None)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _handshake = handshake;

            _serialPort = new SerialPort();
            ConfigureSerialPort();
        }

        public void SetReadTimeout(int timeout)
        {
            _serialPort.ReadTimeout = timeout;
        }

        public void SetWriteTimeout(int timeout)
        {
            _serialPort.WriteTimeout = timeout;
        }

        public bool OpenSerialPort()
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }
                return true;
            }
            catch (Exception ex)//TODO: move exception handling to a higher level and log the exception instead of writing it to the consol
            {
                Console.WriteLine($"Error opening serial port: {ex.Message}");
                return false;
            }
        }

        public bool CloseSerialPort()
        {
            try
            {
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

        public async Task<bool> WriteToSerialPort(string data)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    await Task.Run(() => _serialPort.WriteLine(data));
                    return true;
                }
                else
                {
                    Console.WriteLine("Serial port is not open.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to serial port: {ex.Message}");
                return false;
            }
        }

        public string? ReadFromSerialPort()
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    return _serialPort.ReadLine();
                }
                else
                {
                    Console.WriteLine("Serial port is not open.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from serial port: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Wysyła ramkę i czyta odpowiedź tą samą konwencją na puli wątków, więc UI może robić <c>await</c> bez blokowania.
        /// Domyślnie: koniec linii (<see cref="SerialMessageFraming.Newline"/>). Dla „pełnego JSON-a” bez względu na entery ustaw
        /// <see cref="SerialMessageFraming.LengthPrefixUInt32LittleEndian"/> — musi tak samo działać firmware.
        /// </summary>
        public async Task<string?> SendRequestAsync(string data, SerialMessageFraming framing = SerialMessageFraming.Newline)
        {
            var entered = await _requestGate.WaitAsync(0).ConfigureAwait(false);
            if (!entered)
            {
                Console.WriteLine("Serial request already in progress.");
                return null;
            }

            try
            {
                return await Task.Run(async () =>
                {
                    if (!await WriteRequest(data, framing))
                        return null;
                    return ReadResponse(framing);
                }).ConfigureAwait(false);
            }
            finally
            {
                _requestGate.Release();
            }
        }

        private async Task<bool> WriteRequest(string data, SerialMessageFraming framing)
        {
            return framing switch
            {
                SerialMessageFraming.Newline => await WriteToSerialPort(data),
                SerialMessageFraming.LengthPrefixUInt32LittleEndian => await WriteLengthPrefixedUtf8(data),
                _ => false
            };
        }

        private string? ReadResponse(SerialMessageFraming framing)
        {
            return framing switch
            {
                SerialMessageFraming.Newline => ReadFromSerialPort(),
                SerialMessageFraming.LengthPrefixUInt32LittleEndian => ReadLengthPrefixedUtf8(),
                _ => null
            };
        }

        private async Task<bool> WriteLengthPrefixedUtf8(string data)
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    Console.WriteLine("Serial port is not open.");
                    return false;
                }

                var payload = Encoding.UTF8.GetBytes(data);
                if (payload.Length > MaxFramePayloadBytes)
                {
                    Console.WriteLine("Payload exceeds MaxFramePayloadBytes.");
                    return false;
                }

                var n = (uint)payload.Length;
                var header = new byte[4];
                header[0] = (byte)(n & 0xff);
                header[1] = (byte)((n >> 8) & 0xff);
                header[2] = (byte)((n >> 16) & 0xff);
                header[3] = (byte)((n >> 24) & 0xff);

                await _serialPort.BaseStream.WriteAsync(header, 0, 4);
                await _serialPort.BaseStream.WriteAsync(payload, 0, payload.Length);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing length-prefixed frame: {ex.Message}");
                return false;
            }
        }

        private string? ReadLengthPrefixedUtf8()
        {
            try
            {
                if (!_serialPort.IsOpen)
                {
                    Console.WriteLine("Serial port is not open.");
                    return null;
                }

                var header = new byte[4];
                ReadExact(header, 0, 4);
                var length =
                    (uint)header[0]
                    | ((uint)header[1] << 8)
                    | ((uint)header[2] << 16)
                    | ((uint)header[3] << 24);

                if (length > MaxFramePayloadBytes)
                {
                    Console.WriteLine("Declared frame length exceeds MaxFramePayloadBytes.");
                    return null;
                }

                if (length == 0)
                    return string.Empty;

                var payload = new byte[length];
                ReadExact(payload, 0, (int)length);
                return Encoding.UTF8.GetString(payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading length-prefixed frame: {ex.Message}");
                return null;
            }
        }

        private void ReadExact(byte[] buffer, int offset, int count)
        {
            var read = 0;
            while (read < count)
            {
                var n = _serialPort.Read(buffer, offset + read, count - read);
                if (n == 0)
                    throw new EndOfStreamException("Serial read returned 0 bytes before completing frame.");
                read += n;
            }
        }

        private void ConfigureSerialPort()
        {
            _serialPort.PortName = _portName;
            _serialPort.BaudRate = _baudRate;
            _serialPort.Parity = _parity;
            _serialPort.DataBits = _dataBits;
            _serialPort.StopBits = _stopBits;
            _serialPort.Handshake = _handshake;
            _serialPort.ReadTimeout = _readTimeout;
            _serialPort.WriteTimeout = _writeTimeout;
        }
    }
}
