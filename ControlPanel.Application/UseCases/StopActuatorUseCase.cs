using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

            byte messageType = ProtocolMessageTypes.STOP;
            byte[] payload = new byte[] { (byte)actuatorId };
            
            await _serialCommunication.SendCommandAsync(messageType, payload);
            
            actuator.SetState(Domain.Enums.ActuatorState.Idle);
        }
    }
}
