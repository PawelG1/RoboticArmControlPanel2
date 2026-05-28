using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Presentation.WPF.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class SimpleControlViewModel : BaseViewModel
    {

        private readonly IRobotStateService _robotStateService;
        private readonly IRobotControlService _robotControlService;

        public ObservableCollection<ActuatorControlViewModel>? Actuators { get; } = new();
        public SimpleControlViewModel(IRobotStateService robotStateService, IRobotControlService robotControlService)
        {
            _robotStateService = robotStateService;
            _robotControlService = robotControlService;

            _robotStateService.RobotConfigured += OnRobotConfigured;
            _robotStateService.StateUpdated += OnStateUpdated;

            StopAllCommand = new RelayCommand(StopAll);
            if (_robotStateService.Robot.IsConfigured) {
                AssignAvailableActuators();
            }
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

        private void OnRobotConfigured(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                AssignAvailableActuators();
            });
        }

        private void AssignAvailableActuators()
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

        private void OnStateUpdated(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
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
            });
        }
    }
}
