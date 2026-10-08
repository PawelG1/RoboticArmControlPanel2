using ControlPanel.Infrastructure.Hardware;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPanel.Tests.UnitTests
{
    public class Tests
    {
        SerialPortController port;

        [SetUp]
        public void Setup()
        {
            
            port = new SerialPortController(
                portName: "COM3",
                baudRate: 9600,
                parity: System.IO.Ports.Parity.None,
                dataBits: 8,
                stopBits: System.IO.Ports.StopBits.One
            );
            port.OpenSerialPort();
        }

        [TearDown]
        public void TearDown()
        {
            port.CloseSerialPort();
            port.Dispose();
        }

        [Test]
        public void Test1()
        {
            //Arrange
            //Act
            port.WriteFrameAsync(new ControlPanel.Infrastructure.Hardware.Protocol.BinaryFrame() { MessageType = 0x01, Sequence = 1, Payload = new byte[] { 0xFF } }).Wait();

            //Assert
            Assert.Pass();
        }
    }
}
