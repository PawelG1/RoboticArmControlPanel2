using ControlPanel.Application.Interfaces;
using ControlPanel.Presentation.WPF.Common;
using ControlPanel.WPF.Services.Interfaces;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IUserInteractionService _userInteractionService;
        private readonly ISerialCommunication _serialCommunication;
        private readonly IRobotStateService _robotStateService;
        private readonly IRobotControlService _robotControlService;
        public MainWindowViewModel(IUserInteractionService userInteractionService, ISerialCommunication serialCommunication, IRobotStateService robotStateService, IRobotControlService robotControlService) {
            _userInteractionService = userInteractionService;
            _serialCommunication = serialCommunication;
            _robotStateService = robotStateService;
            _robotControlService = robotControlService;
            OpenConfigurationPageCommand = new RelayCommand(OpenConfigurationPage);
            OpenSimpleControlPageCommand = new RelayCommand(OpenSimpleControlPage);
            _currentView = _userInteractionService.GetView(new HomePageViewModel());
        }

        private string _title = "Main Window";

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand OpenConfigurationPageCommand { get; set; }

        private void OpenConfigurationPage(object _)
        {
            CurrentView = _userInteractionService.GetView(new ConfigurationViewModel(_userInteractionService, _serialCommunication, _robotStateService));
        }

        public ICommand OpenSimpleControlPageCommand { get; set; }
        public void OpenSimpleControlPage(object _) {
            CurrentView = _userInteractionService.GetView(new SimpleControlViewModel(_robotStateService, _robotControlService));
        }

    }
}
