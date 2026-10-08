using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Constants;
using System;
using System.Threading.Tasks;

namespace ControlPanel.Application.UseCases
{
    public class CalibrateActuatorUseCase
    {
        private readonly ISerialCommunicationService _serialCommunication;
        private readonly Robot _robot;

        public CalibrateActuatorUseCase(Robot robot, ISerialCommunicationService serialCommunication)
        {
            _serialCommunication = serialCommunication;
            _robot = robot;
        }

        public async Task Execute(int actuatorId, byte options)
        {
            Actuator? actuator = _robot.GetActuatorById(actuatorId);

            if (actuator == null)
            {
                throw new ArgumentException($"Actuator with ID {actuatorId} not found.");
            }

            byte[] payload = new byte[2];
            payload[0] = (byte)actuatorId;
            payload[1] = options;

            await _serialCommunication.SendCommandAsync(ProtocolMessageTypes.CALIBRATE, payload);
        }
    }
}

