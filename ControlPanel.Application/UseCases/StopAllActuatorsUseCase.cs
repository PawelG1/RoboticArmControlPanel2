using ControlPanel.Application.DTOs.SerialCommands;
using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Serialization;
using ControlPanel.Domain.Entities;

namespace ControlPanel.Application.UseCases
{ 
    public class StopAllActuatorsUseCase
    {
        private readonly ISerialCommunicationService _serialCommunication;
        private readonly Robot _robot;

        public StopAllActuatorsUseCase(Robot robot, ISerialCommunicationService serialCommunication)
        {
            _robot = robot;
            _serialCommunication = serialCommunication;
        }

        public async Task Execute()
        {
            var dto = new StopAllWireDTO();
            string json = JsonCommandSerializer.ToJson(dto);
            await _serialCommunication.SendJsonLineAsync(json);

            foreach (var actuator in _robot.GetAllActuators())
            {
                actuator.SetState(Domain.Enums.ActuatorState.Idle);
            }
        }

    }
}
