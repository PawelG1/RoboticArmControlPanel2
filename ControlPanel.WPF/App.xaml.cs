using ControlPanel.DI;
using ControlPanel.WPF.Services;
using ControlPanel.WPF.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ControlPanel.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {   
        IServiceCollection services;
        App() : base() {
            services = new ServiceCollection();
            services = IoContainer.AddControlPanelProductionCollection(services);
            services.AddSingleton<IUserInteractionService, UserInteractionService>();
            services.BuildServiceProvider();

            var navigationService = IoContainer.GetRequiredService<IUserInteractionService>(services);
            navigationService.Show(new Presentation.WPF.ViewModels.MainWindowViewModel(navigationService));
        }
    }

}
