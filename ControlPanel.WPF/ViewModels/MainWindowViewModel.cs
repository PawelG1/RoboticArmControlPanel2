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
        private readonly ISerialCommunicationService _serialCommunication;
        private readonly IRobotStateService _robotStateService;
        private readonly IRobotControlService _robotControlService;
        private readonly IRobotSequenceService _robotSequenceService;
        private readonly RobotVisualiser3DViewModel robotVisualiser3DViewModel;
        public MainWindowViewModel(
            IUserInteractionService userInteractionService, 
            ISerialCommunicationService serialCommunication, 
            IRobotStateService robotStateService, 
            IRobotControlService robotControlService,
            IRobotSequenceService robotSequenceService,
            RobotVisualiser3DViewModel robotVisualiser3DViewModel
            ){
            _userInteractionService = userInteractionService;
            _serialCommunication = serialCommunication;
            _robotStateService = robotStateService;
            _robotControlService = robotControlService;
            _robotSequenceService = robotSequenceService;
            OpenConfigurationPageCommand = new RelayCommand(OpenConfigurationPage);
            OpenSimpleControlPageCommand = new RelayCommand(OpenSimpleControlPage);
            OpenSequenceManagmentPageCommand = new RelayCommand(OpenSequenceManagmentPage);
            _currentView = _userInteractionService.GetView(new HomePageViewModel());
            this.robotVisualiser3DViewModel = robotVisualiser3DViewModel;
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
            CurrentView = _userInteractionService.GetView(new SimpleRobotControlViewModel(_robotStateService, _robotControlService, robotVisualiser3DViewModel));
        }

        public ICommand OpenSequenceManagmentPageCommand { get; set; }
        public void OpenSequenceManagmentPage(object _)
        {
            CurrentView = _userInteractionService.GetView(new SequenceManagmentViewModel(_userInteractionService ,_robotSequenceService ,_robotStateService, _robotControlService, robotVisualiser3DViewModel));
        }

    }
}
