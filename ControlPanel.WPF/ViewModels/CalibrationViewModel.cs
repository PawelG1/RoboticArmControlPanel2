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
                    NewAcceleration = _selectedActuator.Acceleration;
                }
                ((RelayCommand)CalibrateCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SetLimitsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SetAccelerationCommand).RaiseCanExecuteChanged();
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

        private int _newAcceleration = 100;
        public int NewAcceleration
        {
            get => _newAcceleration;
            set
            {
                int clamped = Math.Clamp(value, 1, 255);
                if (_newAcceleration == clamped)
                {
                    if (value != clamped)
                        NotifyPropertyChanged(nameof(NewAcceleration));
                    return;
                }
                _newAcceleration = clamped;
                NotifyPropertyChanged(nameof(NewAcceleration));
            }
        }

        public ICommand CalibrateCommand { get; }
        public ICommand SetLimitsCommand { get; }
        public ICommand SetAccelerationCommand { get; }

        public CalibrationViewModel(IRobotStateService robotStateService, IRobotControlService robotControlService)
        {
            _robotStateService = robotStateService;
            _robotControlService = robotControlService;

            _robotStateService.RobotConfigured += OnRobotConfigured;

            CalibrateCommand = new RelayCommand(Calibrate, CanExecuteActuatorCommand);
            SetLimitsCommand = new RelayCommand(SetLimits, CanExecuteActuatorCommand);
            SetAccelerationCommand = new RelayCommand(SetAcceleration, CanExecuteActuatorCommand);

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
                    _robotControlService,
                    actuator.GetAcceleration
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
                    NewAcceleration = SelectedActuator.Acceleration;
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

        private async void SetAcceleration(object? obj)
        {
            if (SelectedActuator == null) return;

            try
            {
                int acceleration = Math.Clamp(NewAcceleration, 1, 255);
                await _robotControlService.SetAccelerationActuator(SelectedActuator.Id, acceleration);
                SelectedActuator.Acceleration = acceleration;
                await _robotControlService.GetConfig();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SET ACCELERATION ERROR] {ex.Message}");
            }
        }
    }
}
