using ControlPanel.Domain.Enums;
using ControlPanel.Domain.Exceptions;
using ControlPanel.Domain.ValueObjects;

namespace ControlPanel.Domain.Entities
{
    public class Actuator
    {
        protected readonly int _id;
        protected double _currentAngle;
        protected ActuatorWorkingLimits _workingLimits;
        protected double _targetAngle;
        protected double _speed;
        protected int _acceleration = 100;
        protected ActuatorState _state;
        protected RotatingDirection _rotatingDirection;

        public Actuator(int id, ActuatorWorkingLimits workingLimits, int acceleration = 100)
        {
            _id = id;
            SetWorkingLimits(workingLimits);
            SetAcceleration(acceleration);
            SetSpeed(100);
            SetRotatingDirection(RotatingDirection.Clockwise);
            SetState(ActuatorState.Idle);
        }

        public int GetId => _id;
        public double GetCurrentAngle => _currentAngle;

        public void SetCurrentAngle(double readAngle) => _currentAngle = readAngle;


        public void SetWorkingLimits(ActuatorWorkingLimits readLimits)
        {
            _workingLimits = readLimits;
        }

        public ActuatorWorkingLimits GetWorkingLimits => _workingLimits;

        public void SetTargetAngle(double target) {
            if (_workingLimits.MaxAngle < target || target < _workingLimits.MinAngle)
            {
                throw new IncorrectTargetAngleParsedException(target, _workingLimits);
            }
            _targetAngle = target;
        }

        public double GetTargetAngle() => _targetAngle;

        public int GetAcceleration => _acceleration;

        public void SetAcceleration(int acceleration)
        {
            if (acceleration < 1 || acceleration > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(acceleration), acceleration, "Acceleration must be in range 1..255.");
            }
            _acceleration = acceleration;
        }

        public double GetSpeed() => _speed;
        public void SetSpeed(double speed) {
            if (speed < 1 || speed > 255)
            {
                throw new IncorrectActuatorSpeedException(speed);
            }
            _speed = speed;
        }

        public ActuatorState GetState => _state;
        public void SetState(ActuatorState state) {
            if(state == _state) 
                return;

            if(state == ActuatorState.Moving) {
                // Additional logic for when the actuator is ordered to move
                switch (_state) {
                    case ActuatorState.Error:
                        throw new InvalidOperationException("Cannot move actuator in error state.");
                }

                if (GetSpeed() == 0) {
                    throw new InvalidOperationException("Cannot move actuator with speed 0.");
                }
            }
            _state = state;   
        }

        public RotatingDirection GetRotatingDirection => _rotatingDirection;
        public void SetRotatingDirection(RotatingDirection direction) {
            if(GetState == ActuatorState.Moving)
            {
                throw new InvalidOperationException("Cannot change the rotating direction when actuator is moving.");
            }
            _rotatingDirection = direction;
        }

    }
}
