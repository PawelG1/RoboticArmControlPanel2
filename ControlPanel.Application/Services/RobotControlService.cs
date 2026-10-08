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

        public RobotControlService(
            MoveActuatorUseCase moveActuatorUseCase,
            StopActuatorUseCase stopActuatorUseCase,
            StopAllActuatorsUseCase stopAllActuatorsUseCase,
            CalibrateActuatorUseCase calibrateActuatorUseCase)
        {
            _moveActuatorUseCase = moveActuatorUseCase;
            _stopActuatorUseCase = stopActuatorUseCase;
            _stopAllActuatorsUseCase = stopAllActuatorsUseCase;
            _calibrateActuatorUseCase = calibrateActuatorUseCase;
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

    }
}
