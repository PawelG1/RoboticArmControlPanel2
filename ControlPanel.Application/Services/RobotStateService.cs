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
            if (payload.Length < 50) return;

            var steppers = new List<Actuator>
            {
                CreateActuatorFromConfig(payload, ControlPanel.Domain.Constants.ActuatorIds.Stepper1, 0),
                CreateActuatorFromConfig(payload, ControlPanel.Domain.Constants.ActuatorIds.Stepper2, 10)
            };

            var servos = new List<Actuator>
            {
                CreateActuatorFromConfig(payload, ControlPanel.Domain.Constants.ActuatorIds.Servo1, 20),
                CreateActuatorFromConfig(payload, ControlPanel.Domain.Constants.ActuatorIds.Servo2, 30),
                CreateActuatorFromConfig(payload, ControlPanel.Domain.Constants.ActuatorIds.Servo3, 40)
            };

            var config = new RobotConfig(steppers, servos, new Dictionary<string, int>());

            _configRequested = false;
            Robot.ApplyConfiguration(config);
            RobotConfigured?.Invoke(this, EventArgs.Empty);
        }

        private static Actuator CreateActuatorFromConfig(byte[] payload, byte id, int offset)
        {
            double min = BitConverter.ToInt32(payload, offset) / 100.0;
            double max = BitConverter.ToInt32(payload, offset + 4) / 100.0;
            byte flags = payload[offset + 8];
            int acceleration = payload[offset + 9];
            if (acceleration < 1)
                acceleration = 100;

            var actuator = new Actuator(id, new ActuatorWorkingLimits(min, max), acceleration);
            if ((flags & 0x02) != 0)
                actuator.SetRotatingDirection(RotatingDirection.CounterClockwise);
            return actuator;
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

            ActuatorState DetermineStatus(Actuator actuator, bool isMoving)
            {
                if (isMoving) return ActuatorState.Moving;
                if (actuator.GetState == ActuatorState.Moving) return ActuatorState.Idle;
                return actuator.GetState;
            }

            byte steppersEnabled = payload[22];

            // Stepper 0
            double stepper0Angle = BitConverter.ToInt32(payload, 6) / 100.0;
            Actuator? stepper0 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Stepper1);
            if (stepper0 != null) Robot.UpdateActuator(stepper0, currentAngle: stepper0Angle, state: DetermineStatus(stepper0, (steppersEnabled & (1 << 0)) != 0));

            // Stepper 1
            double stepper1Angle = BitConverter.ToInt32(payload, 14) / 100.0;
            Actuator? stepper1 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Stepper2);
            if (stepper1 != null) Robot.UpdateActuator(stepper1, currentAngle: stepper1Angle, state: DetermineStatus(stepper1, (steppersEnabled & (1 << 1)) != 0));

            // Servo 0 (offset 23)
            double servo0Angle = BitConverter.ToInt32(payload, 23) / 100.0;
            Actuator? servo0 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Servo1);
            if (servo0 != null) Robot.UpdateActuator(servo0, currentAngle: servo0Angle, state: DetermineStatus(servo0, payload[31] != 0));

            // Servo 1 (offset 32)
            double servo1Angle = BitConverter.ToInt32(payload, 32) / 100.0;
            Actuator? servo1 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Servo2);
            if (servo1 != null) Robot.UpdateActuator(servo1, currentAngle: servo1Angle, state: DetermineStatus(servo1, payload[40] != 0));

            // Servo 2 (offset 41)
            double servo2Angle = BitConverter.ToInt32(payload, 41) / 100.0;
            Actuator? servo2 = Robot.GetActuatorById(ControlPanel.Domain.Constants.ActuatorIds.Servo3);
            if (servo2 != null) Robot.UpdateActuator(servo2, currentAngle: servo2Angle, state: DetermineStatus(servo2, payload[49] != 0));

            StateUpdated?.Invoke(this, EventArgs.Empty);
        }

    }
}
