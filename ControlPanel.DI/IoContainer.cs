using ControlPanel.Application.Interfaces;
using ControlPanel.Application.UseCases;
using ControlPanel.Infrastructure;
using ControlPanel.Infrastructure.Hardware;
using ControlPanel.Infrastructure.Persistence.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace ControlPanel.DI
{
    public static class IoContainer
    {
        /// <summary> Dependency Injection Container </summary>

        public static IServiceCollection AddControlPanelProductionCollection(
               this IServiceCollection services
            ) {

            //infrastructure

            //repositories
            services.AddSingleton<IActuatorRepository, ActuatorInMemoryRepository>();
            
            //hardware
            services.AddSingleton<ISerialCommunication, SerialCommunicationService>();
            services.AddSingleton(new SerialPortController(
                portName: "COM3",
                baudRate: 9600,
                parity: System.IO.Ports.Parity.None,
                dataBits: 8,
                stopBits: System.IO.Ports.StopBits.One
                ));

            //application
            RegisterApplicationServices(services);

            return services;
        }

        /// <summary>
        /// Rejestruje Use Cases - wspólne dla wszystkich konfiguracji
        /// </summary>
        private static void RegisterApplicationServices(IServiceCollection services)
        {
            // Use Cases
            services.AddScoped<MoveActuatorUseCase>();
            services.AddScoped<StopActuatorUseCase>();
        }
    }
}
