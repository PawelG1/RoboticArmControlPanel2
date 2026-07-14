using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Presentation.WPF.Common;
using ControlPanel.Presentation.WPF.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class SimpleRobotControlViewModel : BaseViewModel
    {

        protected readonly IRobotStateService _robotStateService;
        protected readonly IRobotControlService _robotControlService;

        public ObservableCollection<ActuatorControlViewModel>? Actuators { get; } = new();
        public RobotVisualiser3DViewModel RobotVisualiser3DVM {get;}
        public UserControl RobotVisualiserView { get; set; }
        public SimpleRobotControlViewModel(IRobotStateService robotStateService, IRobotControlService robotControlService, RobotVisualiser3DViewModel robotVisualiser3DVM)
        {
            _robotStateService = robotStateService;
            _robotControlService = robotControlService;

            _robotStateService.RobotConfigured += OnRobotConfigured;
            _robotStateService.StateUpdated += OnStateUpdated;

            StopAllCommand = new RelayCommand(StopAll);
            if (_robotStateService.Robot.IsConfigured)
            {
                AssignAvailableActuators();
            }

            RobotVisualiser3DVM = robotVisualiser3DVM;
            RobotVisualiserView = new RobotVisualiser3DView() {
            DataContext = RobotVisualiser3DVM
            };
        }

        public override void Dispose()
        {
            _robotStateService.RobotConfigured -= OnRobotConfigured;
            _robotStateService.StateUpdated -= OnStateUpdated;
            if (Actuators != null) {
                foreach (var actuatorVm in Actuators)
                {
                    actuatorVm.Dispose();
                }
            }
            base.Dispose();
        }

        public ICommand StopAllCommand { get; }
        public void StopAll(object _)
        {
            _robotControlService.StopAllActuators();
        }

        protected void OnRobotConfigured(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                AssignAvailableActuators();
            });
        }

        protected void AssignAvailableActuators()
        {
            Actuators?.Clear();
            var actuators = _robotStateService.Robot.GetAllActuators();
            if (actuators != null)
            {
                foreach (var actuator in actuators)
                {
                    string actuatorName = $"Actuator {actuator.GetId}";
                    var actuatorVm = new ActuatorControlViewModel(
                        actuator.GetId,
                        actuatorName,
                        actuator.GetWorkingLimits.MinAngle,
                        actuator.GetWorkingLimits.MaxAngle,
                        actuator.GetCurrentAngle,
                        _robotControlService
                        );
                    Actuators?.Add(
                       actuatorVm
                    );
                }
            }
        }

        protected void OnStateUpdated(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Delegate)(() =>
            {
                if(Actuators != null)
                {
                    foreach (var actuatorVm in Actuators)
                    {
                        var actuator = _robotStateService.Robot.GetActuatorById(actuatorVm.Id);
                        if (actuator != null)
                        {
                            actuatorVm.CurrentAngle = actuator.GetCurrentAngle;
                        
                        }
                    }
                }
            }));
        }
    }
}
