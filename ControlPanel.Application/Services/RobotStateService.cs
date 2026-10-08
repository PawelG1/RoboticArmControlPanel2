using ControlPanel.Application.DTOs.SerialCommands;
using ControlPanel.Application.DTOs;
using ControlPanel.Application.DTOs.IncomingMessages;
using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Mapping;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ControlPanel.Application.Services
{
    public class RobotStateService : IRobotStateService
    {
        private readonly ISerialCommunicationService _serialCommunication;

     
        public Robot Robot { get; }

        public event EventHandler? StateUpdated;
        public event EventHandler? RobotConfigured;
        private bool _configRequested = false;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter<ActuatorStatusWire>() }
        };


        public RobotStateService(ISerialCommunicationService serialCommunication, Robot robot)
        {
            _serialCommunication = serialCommunication;
            Robot = robot;
        }

        public void StartListening()
        {
            _serialCommunication.FrameReceived += OnFrameReceived;
        }
        public void StopListening()
        {
            _serialCommunication.FrameReceived -= OnFrameReceived;
        }

        public RobotStateDTO GetRobotState()
        {
            IEnumerable<Actuator> actuators = Robot.GetAllActuators();
            HashSet<ActuatorDto> actuatorsDTOs = new(); 
            foreach (Actuator actuator in actuators)
            {
                actuatorsDTOs.Add(new ActuatorDto
                {
                    ObjectIdx = actuator.GetId,
                    ActuatorState = new ActuatorStateDto()
                    {
                        CurrentAngle = actuator.GetCurrentAngle,
                        TargetAngle = actuator.GetTargetAngle(),
                        Status = actuator.GetState.ToWire()
                    }
                });
            }

            return new RobotStateDTO()
            {
                Actuators = actuatorsDTOs.ToList(),
                IsConfigured = Robot.IsConfigured
                //TODO:in future add maybe more props
            };
        }

        private async Task RequestRobotConfig()
        {   
            await _serialCommunication.SendCommandAsync(ControlPanel.Domain.Constants.ProtocolMessageTypes.GET_CONFIG, Array.Empty<byte>());
        }
        private async void OnFrameReceived(object? sender, BinaryMessage frame)
        {
            try
            {
                switch (frame.MessageType)
                {
                    case ControlPanel.Domain.Constants.ProtocolMessageTypes.CONFIG:
                        HandleConfig(frame.Payload);
                        break;
                    case ControlPanel.Domain.Constants.ProtocolMessageTypes.HEARTBEAT:
                        await HandleHeartbeat(frame.Payload);
                        break;
                    case ControlPanel.Domain.Constants.ProtocolMessageTypes.INFO:
                        HandleActuatorInfo(frame.Payload);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error processing binary frame: " + ex.Message);
            }
        }

        private void HandleConfig(byte[] payload)
        {
            if (payload.Length < 45) return;

            var steppers = new List<Actuator>
            {
                new Actuator(ControlPanel.Domain.Constants.ActuatorIds.Stepper1, new ActuatorWorkingLimits(BitConverter.ToInt32(payload, 0) / 100.0, BitConverter.ToInt32(payload, 4) / 100.0)),
                new Actuator(ControlPanel.Domain.Constants.ActuatorIds.Stepper2, new ActuatorWorkingLimits(BitConverter.ToInt32(payload, 9) / 100.0, BitConverter.ToInt32(payload, 13) / 100.0))
            };

            var servos = new List<Actuator>
            {
                new Actuator(ControlPanel.Domain.Constants.ActuatorIds.Servo1, new ActuatorWorkingLimits(BitConverter.ToInt32(payload, 18) / 100.0, BitConverter.ToInt32(payload, 22) / 100.0)),
                new Actuator(ControlPanel.Domain.Constants.ActuatorIds.Servo2, new ActuatorWorkingLimits(BitConverter.ToInt32(payload, 27) / 100.0, BitConverter.ToInt32(payload, 31) / 100.0)),
                new Actuator(ControlPanel.Domain.Constants.ActuatorIds.Servo3, new ActuatorWorkingLimits(BitConverter.ToInt32(payload, 36) / 100.0, BitConverter.ToInt32(payload, 40) / 100.0))
            };

            var config = new RobotConfig(steppers, servos, new Dictionary<string, int>());

            _configRequested = false;
            Robot.ApplyConfiguration(config);
            RobotConfigured?.Invoke(this, EventArgs.Empty);
        }

        private void HandleActuatorInfo(byte[] payload)
        {
            if (payload.Length < 10) return;

            byte actuatorId = payload[0];
            double currentAngle = BitConverter.ToInt32(payload, 1) / 100.0;
            byte statusByte = payload[9];

            ActuatorState status = statusByte switch
            {
                0 => ActuatorState.Idle,
                1 => ActuatorState.Moving,
                2 => ActuatorState.Forbidden,
                3 => ActuatorState.EStop,
                4 => ActuatorState.Moving,
                5 => ActuatorState.Idle,
                _ => ActuatorState.Error
            };

            Actuator? actuator = Robot.GetActuatorById(actuatorId);
            if (actuator != null)
            {
                Robot.UpdateActuator(actuator, currentAngle, status);
                StateUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task HandleHeartbeat(byte[] payload)
        {
            if (payload.Length < 50) return;

            Robot.RegisterHeartbeat();

            if (!Robot.IsConfigured)
            {
                if (!_configRequested)
                {
                    _configRequested = true;
                    await RequestRobotConfig();
                }
                return;
            }

            // Stepper 0
            double stepper0Angle = BitConverter.ToInt32(payload, 6) / 100.0;
            Actuator? stepper0 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Stepper1);
            if (stepper0 != null) Robot.UpdateActuator(stepper0, currentAngle: stepper0Angle);

            // Stepper 1
            double stepper1Angle = BitConverter.ToInt32(payload, 14) / 100.0;
            Actuator? stepper1 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Stepper2);
            if (stepper1 != null) Robot.UpdateActuator(stepper1, currentAngle: stepper1Angle);

            // Servo 0 (offset 23)
            double servo0Angle = BitConverter.ToInt32(payload, 23) / 100.0;
            Actuator? servo0 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Servo1);
            if (servo0 != null) Robot.UpdateActuator(servo0, currentAngle: servo0Angle);

            // Servo 1 (offset 32)
            double servo1Angle = BitConverter.ToInt32(payload, 32) / 100.0;
            Actuator? servo1 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Servo2);
            if (servo1 != null) Robot.UpdateActuator(servo1, currentAngle: servo1Angle);

            // Servo 2 (offset 41)
            double servo2Angle = BitConverter.ToInt32(payload, 41) / 100.0;
            Actuator? servo2 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Servo3);
            if (servo2 != null) Robot.UpdateActuator(servo2, currentAngle: servo2Angle);

            StateUpdated?.Invoke(this, EventArgs.Empty);
        }

    }
}
