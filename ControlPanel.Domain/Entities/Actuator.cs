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
        protected ActuatorState _state;
        protected RotatingDirection _rotatingDirection;

        public Actuator(int id, ActuatorWorkingLimits workingLimits)
        {
            _id = id;
            SetWorkingLimits(workingLimits);
            SetSpeed(0);
            SetRotatingDirection(RotatingDirection.Clockwise);
            SetState(ActuatorState.Idle);
        }

        public int GetId => _id;
        public double GetCurrentAngle => _currentAngle;

        public void SetCurrentAngle(double readAngle) => _currentAngle = readAngle;


        public void SetWorkingLimits(ActuatorWorkingLimits readLimits)
        {
            if (readLimits.MinAngle > readLimits.MaxAngle)
            {
                throw new ArgumentException("Invalid actuator limits. Min angle must be less than or equal to max angle.");
            }
            _workingLimits = readLimits;
        }

        public ActuatorWorkingLimits GetWorkingLimits() => _workingLimits;


        public void SetTargetAngle(double target) {
            if (_workingLimits.MaxAngle < target || target < _workingLimits.MinAngle)
            {
                throw new IncorrectTargetAngleParsedException(target, _workingLimits);
            }
            _targetAngle = target;
        }

        public double GetTargetAngle() => _targetAngle;

        public double GetSpeed() => _speed;
        public void SetSpeed(double speed) {
            if (speed < 0)
            {
                throw new IncorrectActuatorSpeedException(speed);
            }
            _speed = speed;
        }

        public ActuatorState GetState => _state;
        public void SetState(ActuatorState state) {
            if(state == ActuatorState.Moving) {
                // Additional logic for when the actuator is ordered to move
                switch (_state) {
                    case ActuatorState.Error:
                        throw new InvalidOperationException("Cannot move actuator in error state.");
                    case ActuatorState.Moving:
                         throw new InvalidOperationException("Actuator is already moving.");
                }

                if (GetRotatingDirection == RotatingDirection.Clockwise && _targetAngle < _currentAngle)
                {
                    throw new InvalidOperationException("Cannot move clockwise to a smaller angle.");
                }else if (GetRotatingDirection == RotatingDirection.CounterClockwise && _targetAngle > _currentAngle)
                {
                    throw new InvalidOperationException("Cannot move counterclockwise to a larger angle.");
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
