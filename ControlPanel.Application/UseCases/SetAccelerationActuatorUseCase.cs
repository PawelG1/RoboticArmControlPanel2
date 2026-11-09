using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Constants;
using ControlPanel.Domain.Entities;

namespace ControlPanel.Application.UseCases
{
    public class SetAccelerationActuatorUseCase
    {
        private readonly ISerialCommunicationService _serialCommunication;
        private readonly Robot _robot;

        public SetAccelerationActuatorUseCase(Robot robot, ISerialCommunicationService serialCommunication)
        {
            _serialCommunication = serialCommunication;
            _robot = robot;
        }

        public async Task Execute(int actuatorId, int acceleration)
        {
            Actuator? actuator = _robot.GetActuatorById(actuatorId);

            if (actuator == null)
            {
                throw new ArgumentException($"Actuator with ID {actuatorId} not found.");
            }

            if (acceleration < 1 || acceleration > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(acceleration), acceleration, "Acceleration must be in range 1..255.");
            }

            actuator.SetAcceleration(acceleration);

            byte[] payload = new byte[2] { (byte)actuatorId, (byte)acceleration };
            await _serialCommunication.SendCommandAsync(ProtocolMessageTypes.SET_ACCELERATION, payload);
        }
    }
}
