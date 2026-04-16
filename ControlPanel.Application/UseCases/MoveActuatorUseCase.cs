using ControlPanel.Application.DTOs.SerialCommands;
using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Serialization;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;

namespace ControlPanel.Application.UseCases
{
    public class MoveActuatorUseCase
    {
        private readonly IActuatorRepository _actuatorRepository;
        private readonly ISerialCommunication _serialCommunication;

        public MoveActuatorUseCase(IActuatorRepository actuatorRepository, ISerialCommunication serialCommunication)
        {
            _actuatorRepository = actuatorRepository;
            _serialCommunication = serialCommunication;
        }

        public async Task Execute(int actuatorId, double targetAngle, double speed)
        {
            Actuator? actuator = await _actuatorRepository.GetActuatorById(actuatorId);

            if (actuator == null)
            {
                throw new ArgumentException($"Actuator with ID {actuatorId} not found.");
            }
            

            double currentAngle = actuator.GetCurrentAngle;

            //Determine the direction to rotate based on the current angle and target angle
            //we assume that the an angle need to rise to reach the target angle, then the direction is clockwise, otherwise it's counterclockwise
            RotatingDirection direction = ( currentAngle < targetAngle ) ? RotatingDirection.Clockwise : RotatingDirection.CounterClockwise;
            await _actuatorRepository.SetRotatingDirection(actuator, direction);
            await _actuatorRepository.SetActuatorTargetAngle(actuator, targetAngle);
            await _actuatorRepository.SetActuatorSpeed(actuator, speed);
            await _actuatorRepository.SetActuatorState(actuator, ActuatorState.Moving);


            //build the DTO to send to the serial communication
            MoveActuatorWireDto CommandWireDto = new MoveActuatorWireDto(
                id: actuatorId,
                targetAngle: targetAngle,
                direction: direction
            );

            string json = JsonCommandSerializer.ToJson(CommandWireDto);
            await _serialCommunication.SendJsonLineAsync(json);
        }
    }
}
