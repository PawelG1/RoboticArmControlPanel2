using ControlPanel.Application.Interfaces;
using ControlPanel.DI;
using ControlPanel.Presentation.WPF.ViewModels;
using ControlPanel.WPF.Services;
using ControlPanel.WPF.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ControlPanel.WPF
{
    public partial class App : System.Windows.Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            services.AddControlPanelProductionCollection();
            AddPresentationLayerServices(services);

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            var navigationService = _serviceProvider.GetRequiredService<IUserInteractionService>();
            var mainVm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            //var actuatorRepo = _serviceProvider.GetRequiredService<IActuatorRepository>();
            
            var robotState = _serviceProvider.GetRequiredService<IRobotStateService>();
            robotState.StartListening();

            navigationService.Show(mainVm);
        }

        protected void AddPresentationLayerServices(IServiceCollection services)
        {
            services.AddSingleton<IUserInteractionService, UserInteractionService>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<HomePageViewModel>();
            services.AddTransient<ConfigurationViewModel>();
        }
    }
}
