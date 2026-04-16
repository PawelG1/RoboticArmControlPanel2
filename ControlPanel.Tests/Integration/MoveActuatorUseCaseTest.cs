using ControlPanel.Application.Interfaces;
using ControlPanel.Application.UseCases;
using ControlPanel.DI;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.ValueObjects;
using ControlPanel.Infrastructure.Hardware;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPanel.Tests.Integration
{
    public class Tests
    {
        ServiceProvider serviceProvider;

        [SetUp]
        public async Task Setup()
        {
            var services = new ServiceCollection();
            services.AddControlPanelProductionCollection();
            serviceProvider = services.BuildServiceProvider();
            SerialPortController port = serviceProvider.GetRequiredService<SerialPortController>();
            port.OpenSerialPort();

        }

        [TearDown]
        public void TearDown()
        {
            SerialPortController port = serviceProvider.GetRequiredService<SerialPortController>();
            port.CloseSerialPort();
            serviceProvider.Dispose();
        }

        [Test]
        public async Task Test1()
        {
            //Arrange

            //Resolve the MoveActuatorUseCase from the service provider
            ActuatorWorkingLimits actuatorWorkingLimits = new ActuatorWorkingLimits(minAngle: 0, maxAngle: 180);
            Actuator actuator = new Actuator(
                id: 1,
                workingLimits: actuatorWorkingLimits
            );
            await serviceProvider.GetRequiredService<IActuatorRepository>().AddActuator(actuator);
            var moveActuatorUseCase = serviceProvider.GetRequiredService<MoveActuatorUseCase>();
            
            //Act

            await moveActuatorUseCase.Execute(actuatorId: 1, targetAngle: 90, speed: 10);
            //Assert

            Assert.That(actuator, Is.Not.Null);
            var targetAngle = await serviceProvider.GetRequiredService<IActuatorRepository>().GetActuatorTargetAngle(actuator);
            Assert.That(targetAngle, Is.EqualTo(90));

        }
    }
}
