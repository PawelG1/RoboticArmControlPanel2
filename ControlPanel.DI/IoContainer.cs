using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Services;
using ControlPanel.Application.UseCases;
using ControlPanel.Domain.Entities;
using ControlPanel.Infrastructure;
using ControlPanel.Infrastructure.Hardware;
using ControlPanel.Infrastructure.Persistence.InMemory;
using ControlPanel.Infrastructure.UrdfVisualiser;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPanel.DI
{
    public static class IoContainer
    {
        /// <summary> Dependency Injection Container </summary>

        public static IServiceCollection AddControlPanelProductionCollection(
               this IServiceCollection services)
        {

            //infrastructure
            services.AddSingleton<IUrdfLoader, UrdfFileLoader>();
            services.AddSingleton<IForwardKinematicsService, ForwardKinematicsService>();
            //repositories

            //hardware
            services.AddSingleton<ISerialCommunication, SerialCommunicationService>();
            services.AddSingleton(new SerialPortController(
                portName: "",
                baudRate: 115200,
                parity: System.IO.Ports.Parity.None,
                dataBits: 8,
                stopBits: System.IO.Ports.StopBits.One
                ));

            //application
            RegisterApplicationServices(services);

            services.AddSingleton<Robot>();
            services.AddSingleton<IRobotStateService, RobotStateService>();
            services.AddSingleton<IRobotSequenceService, RobotSequenceService>();

            return services;
        }

        /// <summary>
        /// Rejestruje Use Cases - wspólne dla wszystkich konfiguracji
        /// </summary>
        private static void RegisterApplicationServices(IServiceCollection services)
        {
            // Use Cases
            services.AddTransient<IRobotControlService, RobotControlService>();
            services.AddTransient<MoveActuatorUseCase>();
            services.AddTransient<StopActuatorUseCase>();
            services.AddTransient<StopAllActuatorsUseCase>();
        }

    }
}
