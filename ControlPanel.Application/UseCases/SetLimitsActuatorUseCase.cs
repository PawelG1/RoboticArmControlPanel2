using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Constants;
using System;
using System.Threading.Tasks;

namespace ControlPanel.Application.UseCases
{
    public class SetLimitsActuatorUseCase
    {
        private readonly ISerialCommunicationService _serialCommunication;
        private readonly Robot _robot;

        public SetLimitsActuatorUseCase(Robot robot, ISerialCommunicationService serialCommunication)
        {
            _serialCommunication = serialCommunication;
            _robot = robot;
        }

        public async Task Execute(int actuatorId, double minAngle, double maxAngle)
        {
            Actuator? actuator = _robot.GetActuatorById(actuatorId);

            if (actuator == null)
            {
                throw new ArgumentException($"Actuator with ID {actuatorId} not found.");
            }

            int minAngleInt = (int)Math.Round(minAngle * 100);
            int maxAngleInt = (int)Math.Round(maxAngle * 100);

            byte[] payload = new byte[9];
            payload[0] = (byte)actuatorId;
            BitConverter.GetBytes(minAngleInt).CopyTo(payload, 1);
            BitConverter.GetBytes(maxAngleInt).CopyTo(payload, 5);

            await _serialCommunication.SendCommandAsync(ProtocolMessageTypes.SET_LIMITS, payload);
        }
    }
}
