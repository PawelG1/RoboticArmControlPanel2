using ControlPanel.Application.Interfaces;
using ControlPanel.Presentation.WPF.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using System.Linq;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class CalibrationViewModel : BaseViewModel
    {
        private readonly IRobotStateService _robotStateService;
        private readonly IRobotControlService _robotControlService;

        public ObservableCollection<ActuatorControlViewModel> Steppers { get; } = new();

        private ActuatorControlViewModel? _selectedStepper;
        public ActuatorControlViewModel? SelectedStepper
        {
            get => _selectedStepper;
            set
            {
                _selectedStepper = value;
                NotifyPropertyChanged(nameof(SelectedStepper));
                ((RelayCommand)CalibrateCommand).RaiseCanExecuteChanged();
            }
        }

        private bool _setZero = true;
        public bool SetZero
        {
            get => _setZero;
            set { _setZero = value; NotifyPropertyChanged(nameof(SetZero)); }
        }

        private bool _invertDir;
        public bool InvertDir
        {
            get => _invertDir;
            set { _invertDir = value; NotifyPropertyChanged(nameof(InvertDir)); }
        }

        private bool _writeInvertDir;
        public bool WriteInvertDir
        {
            get => _writeInvertDir;
            set { _writeInvertDir = value; NotifyPropertyChanged(nameof(WriteInvertDir)); }
        }

        public ICommand CalibrateCommand { get; }

        public CalibrationViewModel(IRobotStateService robotStateService, IRobotControlService robotControlService)
        {
            _robotStateService = robotStateService;
            _robotControlService = robotControlService;

            _robotStateService.RobotConfigured += OnRobotConfigured;

            CalibrateCommand = new RelayCommand(Calibrate, CanCalibrate);

            if (_robotStateService.Robot.IsConfigured)
            {
                LoadSteppers();
            }
        }

        public override void Dispose()
        {
            _robotStateService.RobotConfigured -= OnRobotConfigured;
            base.Dispose();
        }

        private void OnRobotConfigured(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LoadSteppers();
            });
        }

        private void LoadSteppers()
        {
            Steppers.Clear();
            var actuators = _robotStateService.Robot.GetAllActuators()
                .Where(a => a.GetId == 0 || a.GetId == 1); // Steppers are 0 and 1

            foreach (var actuator in actuators)
            {
                Steppers.Add(new ActuatorControlViewModel(
                    actuator.GetId,
                    $"Stepper {actuator.GetId}",
                    actuator.GetWorkingLimits.MinAngle,
                    actuator.GetWorkingLimits.MaxAngle,
                    actuator.GetCurrentAngle,
                    _robotControlService
                ));
            }
            if (Steppers.Any())
            {
                SelectedStepper = Steppers.First();
            }
        }

        private bool CanCalibrate(object? obj)
        {
            return SelectedStepper != null;
        }

        private void Calibrate(object? obj)
        {
            if (SelectedStepper == null) return;

            ControlPanel.Domain.Enums.CalibrationOptions options = ControlPanel.Domain.Enums.CalibrationOptions.None;
            
            if (SetZero) options |= ControlPanel.Domain.Enums.CalibrationOptions.SetZero;
            if (InvertDir) options |= ControlPanel.Domain.Enums.CalibrationOptions.InvertDir;
            if (WriteInvertDir) options |= ControlPanel.Domain.Enums.CalibrationOptions.WriteInvertDir;

            _robotControlService.CalibrateActuator(SelectedStepper.Id, (byte)options);
        }
    }
}
