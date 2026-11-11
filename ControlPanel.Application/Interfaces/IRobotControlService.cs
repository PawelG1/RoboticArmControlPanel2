
namespace ControlPanel.Application.Interfaces
{
    public interface IRobotControlService
    {
        Task MoveActuator(int actuatorId, double targetAngle, int speed);
        Task StopActuator(int actuatorId);
        Task StopAllActuators();
        Task CalibrateActuator(int actuatorId, byte options);
        Task SetLimitsActuator(int actuatorId, double minAngle, double maxAngle);
        Task SetAccelerationActuator(int actuatorId, int acceleration);
        Task GetConfig();
    }
}
