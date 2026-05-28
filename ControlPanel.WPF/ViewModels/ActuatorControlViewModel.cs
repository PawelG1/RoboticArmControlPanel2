using ControlPanel.Application.Interfaces;
using ControlPanel.Presentation.WPF.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class ActuatorControlViewModel : BaseViewModel
    {
        private readonly IRobotControlService _robotControlService;
        private CancellationTokenSource? _debounceCts;

        public int Id { get; }
        public string Name { get; }
        public double MinAngle { get; }
        public double MaxAngle { get; }

        public ActuatorControlViewModel(int id, string name, double minAngle, double maxAngle,double initialAngle, IRobotControlService robotControlService)
        {
            Id = id;
            Name = name;
            MinAngle = minAngle;
            MaxAngle = maxAngle;
            CurrentAngle = initialAngle;
            _targetAngle = initialAngle;
            NotifyPropertyChanged(nameof(TargetAngle));
            _robotControlService = robotControlService;
        }

        public override void Dispose()
        {
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            base.Dispose();
        }

        private double _currentAngle;
        public double CurrentAngle
        {
            get {
                return _currentAngle; 
            }
            set {
                _currentAngle = value;
                NotifyPropertyChanged();
            }
        }

        private double _targetAngle;
        public double TargetAngle
        {
            get
            {
                return _targetAngle;
            }
            set
            {
                _targetAngle = value;
                NotifyPropertyChanged();
                OnTargetAngleChanged();
            }
        }

        private int _speed = 200;
        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
                NotifyPropertyChanged();
            }
        }

        private async void OnTargetAngleChanged()
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(300, _debounceCts.Token);
                await _robotControlService.MoveActuator(Id, TargetAngle, Speed);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MOVE ERROR] {ex.Message}");
            }

        }

        


    }
}
