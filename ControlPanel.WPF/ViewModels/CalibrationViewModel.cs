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

        public ObservableCollection<ActuatorControlViewModel> Actuators { get; } = new();

        private ActuatorControlViewModel? _selectedActuator;
        public ActuatorControlViewModel? SelectedActuator
        {
            get => _selectedActuator;
            set
            {
                _selectedActuator = value;
                NotifyPropertyChanged(nameof(SelectedActuator));
                if (_selectedActuator != null)
                {
                    NewMinAngle = _selectedActuator.MinAngle;
                    NewMaxAngle = _selectedActuator.MaxAngle;
                }
                ((RelayCommand)CalibrateCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SetLimitsCommand).RaiseCanExecuteChanged();
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

        private double _newMinAngle;
        public double NewMinAngle
        {
            get => _newMinAngle;
            set { _newMinAngle = value; NotifyPropertyChanged(nameof(NewMinAngle)); }
        }

        private double _newMaxAngle;
        public double NewMaxAngle
        {
            get => _newMaxAngle;
            set { _newMaxAngle = value; NotifyPropertyChanged(nameof(NewMaxAngle)); }
        }

        public ICommand CalibrateCommand { get; }
        public ICommand SetLimitsCommand { get; }

        public CalibrationViewModel(IRobotStateService robotStateService, IRobotControlService robotControlService)
        {
            _robotStateService = robotStateService;
            _robotControlService = robotControlService;

            _robotStateService.RobotConfigured += OnRobotConfigured;

            CalibrateCommand = new RelayCommand(Calibrate, CanExecuteActuatorCommand);
            SetLimitsCommand = new RelayCommand(SetLimits, CanExecuteActuatorCommand);

            if (_robotStateService.Robot.IsConfigured)
            {
                LoadActuators();
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
                LoadActuators();
            });
        }

        private void LoadActuators()
        {
            var oldSelectedId = SelectedActuator?.Id;
            Actuators.Clear();
            var actuators = _robotStateService.Robot.GetAllActuators();

            foreach (var actuator in actuators)
            {
                Actuators.Add(new ActuatorControlViewModel(
                    actuator.GetId,
                    $"Actuator {actuator.GetId}",
                    actuator.GetWorkingLimits.MinAngle,
                    actuator.GetWorkingLimits.MaxAngle,
                    actuator.GetCurrentAngle,
                    _robotControlService
                ));
            }

            if (Actuators.Any())
            {
                if (oldSelectedId.HasValue)
                {
                    SelectedActuator = Actuators.FirstOrDefault(a => a.Id == oldSelectedId.Value) ?? Actuators.First();
                }
                else
                {
                    SelectedActuator = Actuators.First();
                }

                if (SelectedActuator != null)
                {
                    NewMinAngle = SelectedActuator.MinAngle;
                    NewMaxAngle = SelectedActuator.MaxAngle;
                }
            }
        }

        private bool CanExecuteActuatorCommand(object? obj)
        {
            return SelectedActuator != null;
        }

        private void Calibrate(object? obj)
        {
            if (SelectedActuator == null) return;

            ControlPanel.Domain.Enums.CalibrationOptions options = ControlPanel.Domain.Enums.CalibrationOptions.None;
            
            if (SetZero) options |= ControlPanel.Domain.Enums.CalibrationOptions.SetZero;
            if (InvertDir) options |= ControlPanel.Domain.Enums.CalibrationOptions.InvertDir;
            if (WriteInvertDir) options |= ControlPanel.Domain.Enums.CalibrationOptions.WriteInvertDir;

            _robotControlService.CalibrateActuator(SelectedActuator.Id, (byte)options);
            _robotControlService.GetConfig();
        }

        private void SetLimits(object? obj)
        {
            if (SelectedActuator == null) return;

            _robotControlService.SetLimitsActuator(SelectedActuator.Id, NewMinAngle, NewMaxAngle);
            _robotControlService.GetConfig();
        }
    }
}
