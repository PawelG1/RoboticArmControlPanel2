using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.UseCases
{
    public class StopActuatorUseCase
    {
        IActuatorRepository _actuatorRepository;
        public StopActuatorUseCase(IActuatorRepository actuatorRepository)
        {
            _actuatorRepository = actuatorRepository;
        }

        public async Task Execute(int actuatorId)
        {
            Actuator? actuator = await _actuatorRepository.GetActuatorById(actuatorId);
            if (actuator is null)
                throw new ArgumentException($"Actuator with ID {actuatorId} not found.");

            await _actuatorRepository.SetActuatorSpeed(actuator, 0);
            double currentAngle = await _actuatorRepository.GetActuatorCurrentAngle(actuator);
            await _actuatorRepository.SetActuatorTargetAngle(actuator, currentAngle);
            await _actuatorRepository.SetRotatingDirection(actuator, Domain.Enums.RotatingDirection.Clockwise);
            await _actuatorRepository.SetActuatorState(actuator, Domain.Enums.ActuatorState.Idle);
        }
    }
}
