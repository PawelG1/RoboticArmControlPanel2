using ControlPanel.Application.DTOs.SerialCommands;
using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Serialization;
using ControlPanel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.UseCases
{
    public class StopActuatorUseCase
    {
        private readonly ISerialCommunicationService _serialCommunication;
        private readonly Robot _robot;
        public StopActuatorUseCase(Robot robot, ISerialCommunicationService serialCommunication)
        {
           _robot = robot;
           _serialCommunication = serialCommunication;
        }

        public async Task Execute(int actuatorId)
        {
            Actuator? actuator = _robot.GetActuatorById(actuatorId);
            if (actuator is null)
                throw new ArgumentException($"Actuator with ID {actuatorId} not found.");

            var dto = new StopActuatorWireDTO()
            {
                ObjectIdx = actuatorId
            };
            string json = JsonCommandSerializer.ToJson(dto);
            await _serialCommunication.SendJsonLineAsync(json);
            actuator.SetState(Domain.Enums.ActuatorState.Idle);
        }
    }
}
