using ControlPanel.Application.Interfaces;
using ControlPanel.Application.UseCases;


namespace ControlPanel.Application.Services
{
    public class RobotControlService : IRobotControlService
    {
        private readonly MoveActuatorUseCase _moveActuatorUseCase;
        private readonly StopActuatorUseCase _stopActuatorUseCase;
        private readonly StopAllActuatorsUseCase _stopAllActuatorsUseCase;
        private readonly CalibrateActuatorUseCase _calibrateActuatorUseCase;
        private readonly SetLimitsActuatorUseCase _setLimitsActuatorUseCase;
        private readonly SetAccelerationActuatorUseCase _setAccelerationActuatorUseCase;
        private readonly GetConfigUseCase _getConfigUseCase;

        public RobotControlService(
            MoveActuatorUseCase moveActuatorUseCase,
            StopActuatorUseCase stopActuatorUseCase,
            StopAllActuatorsUseCase stopAllActuatorsUseCase,
            CalibrateActuatorUseCase calibrateActuatorUseCase,
            SetLimitsActuatorUseCase setLimitsActuatorUseCase,
            SetAccelerationActuatorUseCase setAccelerationActuatorUseCase,
            GetConfigUseCase getConfigUseCase)
        {
            _moveActuatorUseCase = moveActuatorUseCase;
            _stopActuatorUseCase = stopActuatorUseCase;
            _stopAllActuatorsUseCase = stopAllActuatorsUseCase;
            _calibrateActuatorUseCase = calibrateActuatorUseCase;
            _setLimitsActuatorUseCase = setLimitsActuatorUseCase;
            _setAccelerationActuatorUseCase = setAccelerationActuatorUseCase;
            _getConfigUseCase = getConfigUseCase;
        }

        public Task MoveActuator(int actuatorId, double targetAngle, int speed)
        {
            return _moveActuatorUseCase.Execute(actuatorId, targetAngle, speed);
        }

        public Task StopActuator(int actuatorId)
        {
            return _stopActuatorUseCase.Execute(actuatorId);
        }

        public Task StopAllActuators() {
            return _stopAllActuatorsUseCase.Execute();
        }

        public Task CalibrateActuator(int actuatorId, byte options)
        {
            return _calibrateActuatorUseCase.Execute(actuatorId, options);
        }

        public Task SetLimitsActuator(int actuatorId, double minAngle, double maxAngle)
        {
            return _setLimitsActuatorUseCase.Execute(actuatorId, minAngle, maxAngle);
        }

        public Task SetAccelerationActuator(int actuatorId, int acceleration)
        {
            return _setAccelerationActuatorUseCase.Execute(actuatorId, acceleration);
        }

        public Task GetConfig()
        {
            return _getConfigUseCase.Execute();
        }

    }
}
