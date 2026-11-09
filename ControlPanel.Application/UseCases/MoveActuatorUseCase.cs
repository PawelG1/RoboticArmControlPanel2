using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.Constants;
using System;
using System.Threading.Tasks;

//TODO: verify if we really need usecases bc there are quite simple
namespace ControlPanel.Application.UseCases
{
    public class MoveActuatorUseCase
    {
        private readonly ISerialCommunicationService _serialCommunication;
        private readonly Robot _robot;

        public MoveActuatorUseCase(Robot robot, ISerialCommunicationService serialCommunication)
        {
            _serialCommunication = serialCommunication;
            _robot = robot;
        }

        public async Task Execute(int actuatorId, double targetAngle, int speed)
        {
            Actuator? actuator = _robot.GetActuatorById(actuatorId);

            if (actuator == null)
            {
                throw new ArgumentException($"Actuator with ID {actuatorId} not found.");
            }

            int angleInt = (int)Math.Round(targetAngle * 100);
            int clampedSpeed = Math.Clamp(speed, 1, 255);
            ushort speedUshort = (ushort)clampedSpeed;

            actuator.SetSpeed(clampedSpeed);
            actuator.SetTargetAngle(targetAngle); // Maintain internal state as normal degrees

            byte[] payload = new byte[7];
            payload[0] = (byte)actuatorId;
            BitConverter.GetBytes(angleInt).CopyTo(payload, 1);
            BitConverter.GetBytes(speedUshort).CopyTo(payload, 5);

            await _serialCommunication.SendCommandAsync(ProtocolMessageTypes.MOVE, payload);
        }
    }
}
