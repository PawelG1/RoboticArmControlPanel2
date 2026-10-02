using ControlPanel.Presentation.WPF.Common;
using ControlPanel.Presentation.WPF.ViewModels;
using ControlPanel.Presentation.WPF.Views.Controls;
using ControlPanel.Presentation.WPF.Views.Pages;
using ControlPanel.WPF.Services.Interfaces;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace ControlPanel.WPF.Services
{
    public class UserInteractionService : IUserInteractionService
    {

        private Window GetViewMatchingViewModel(BaseViewModel viewModel)
        {
            switch (viewModel)
            {
                case MainWindowViewModel mainWindowViewModel:
                    return new MainWindow()
                    {
                        DataContext = mainWindowViewModel
                    };
                default:
                    throw new ArgumentException($"No view found for view model of type {viewModel.GetType().Name}");
            }
        }
        public void Show(BaseViewModel viewModel)
        {
            Window window = GetViewMatchingViewModel(viewModel);
            window.Show();
        }

        public void ShowDialog()
        {
            throw new NotImplementedException();
        }

        public UserControl GetView(BaseViewModel viewModel)
        {
            switch (viewModel)
            {
                case HomePageViewModel homeWindowViewModel:
                    return new HomeView()
                    {
                        DataContext = homeWindowViewModel
                    };
                case ConfigurationViewModel configurationViewModel:
                    return new ConfigurationView()
                    {
                        DataContext = configurationViewModel
                    };
                case SequenceManagmentViewModel sequenceManagmentViewModel:
                    return new SequenceManagmentView()
                    {
                        DataContext = sequenceManagmentViewModel
                    };
                case SimpleRobotControlViewModel simpleControlViewModel:
                    return new SimpleControlView()
                    {
                        DataContext = simpleControlViewModel
                    };
                default:
                    throw new ArgumentException($"No page found for view model of type {viewModel.GetType().Name}");
            }
        }

        public void ShowError(string message, string title = "An error occured")
        {
            var view = new ErrorMessageBox(message, title)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            view.ShowDialog();
        }

        public string? OpenFileDialog(string filter, string title = "Select Files")
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                CheckFileExists = true,
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
