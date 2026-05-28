using ControlPanel.Application.DTOs.SerialCommands;
using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Serialization;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;

namespace ControlPanel.Application.UseCases
{
    public class MoveActuatorUseCase
    {
        private readonly ISerialCommunication _serialCommunication;
        private readonly Robot _robot;

        public MoveActuatorUseCase(Robot robot, ISerialCommunication serialCommunication)
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
            targetAngle = Math.Round(targetAngle);

            actuator.SetSpeed(speed);
            actuator.SetTargetAngle(targetAngle);


            var dto = new MoveActuatorWireDto(
            )
            {
                ObjectIdx = actuatorId,
                TargetAngle = targetAngle,
                Speed = speed
            };
            string json = JsonCommandSerializer.ToJson(dto);
            await _serialCommunication.SendJsonLineAsync(json);
        }
    }
}
