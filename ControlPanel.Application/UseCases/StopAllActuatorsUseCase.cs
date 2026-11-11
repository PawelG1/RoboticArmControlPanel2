using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Constants;
using System.Threading.Tasks;

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
            byte messageType = ProtocolMessageTypes.STOP;
            byte[] payload = new byte[] { ActuatorIds.All };
            
            await _serialCommunication.SendCommandAsync(messageType, payload);

            foreach (var actuator in _robot.GetAllActuators())
            {
                actuator.SetState(Domain.Enums.ActuatorState.Idle);
            }
        }

    }
}
