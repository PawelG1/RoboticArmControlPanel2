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
        }

        [Test]
        public void Test1()
        {
            //Arrange
            //Act
            port.SendRequestAsync("Test request from unit test").Wait();    

            //Assert
            Assert.Pass();
        }
    }
}
