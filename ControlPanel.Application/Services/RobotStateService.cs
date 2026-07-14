using ControlPanel.Application.DTOs;
using ControlPanel.Application.DTOs.IncomingMessages;
using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Mapping;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ControlPanel.Application.Services
{
    public class RobotStateService : IRobotStateService
    {
        private readonly ISerialCommunication _serialCommunication;

     
        public Robot Robot { get; }

        public event EventHandler? StateUpdated;
        public event EventHandler? RobotConfigured;
        private bool _configRequested = false;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter<ActuatorStatusWire>() }
        };


        public RobotStateService(ISerialCommunication serialCommunication, Robot robot)
        {
            _serialCommunication = serialCommunication;
            Robot = robot;
        }

        public void StartListening()
        {
            _serialCommunication.MessageReceived += OnMessageReceived;
        }
        public void StopListening()
        {
            _serialCommunication.MessageReceived -= OnMessageReceived;
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
                //in future add maybe more props
            };
        }

        private async Task RequestRobotConfig()
        {   
            var request = new { Type = "GET_CONFIG", ManipulatedObject = "SYSTEM"};
            string json = JsonSerializer.Serialize(request);
            await _serialCommunication.SendJsonLineAsync(json);
            
        }
        private async void OnMessageReceived(object? sender, string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var type = doc.RootElement.GetProperty("Type").GetString();

                switch (type)
                {
                    case "CONFIG":
                        HandleConfig(json);
                        break;
                    case "HEARTBEAT":
                        await HandleHeartbeat(json);
                        break;
                    case "INFO":
                        HandleActuatorInfo(json);
                        break;
                }
            }
            catch (JsonException)
            {
                Debug.WriteLine("Received invalid JSON: " + json);
            }
        }

        private void HandleConfig(string json)
        {
            var dto = JsonSerializer.Deserialize<RobotConfigDTO>(json, JsonOptions);
            if (dto == null)
                return;

            var steppers = dto.Steppers.Select(s => new Actuator(s.Id, new ActuatorWorkingLimits(s.RangeStart, s.RangeEnd)));
            var servos = dto.Servos.Select(s => new Actuator(s.Id, new ActuatorWorkingLimits(s.RangeStart, s.RangeEnd)));
            var config = new RobotConfig(steppers, servos, dto.UserPins.AsReadOnly());

            _configRequested = false;
            Robot.ApplyConfiguration(config);
            RobotConfigured?.Invoke(this, EventArgs.Empty);
        }

        private void HandleActuatorInfo(string json)
        {
            var dto = JsonSerializer.Deserialize<ActuatorDto>(json, JsonOptions);
            if(dto == null) 
                return;

            string manipulatedObject = dto.ManipulatedObject;
            if(manipulatedObject == "ACTUATOR")
            {
                Actuator? actuator = Robot.GetActuatorById(dto.ObjectIdx);
                if (actuator != null)
                {
                    Robot.UpdateActuator(actuator, dto.ActuatorState.CurrentAngle, dto.ActuatorState.Status.ToDomain());
                    StateUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private async Task HandleHeartbeat(string json)
        {
            var dto = JsonSerializer.Deserialize<HeartbeatDTO>(json, JsonOptions);
            if(dto == null) 
                return;

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

            foreach(var encoder in dto.Values.Encoders)
            {
                Actuator? actuator = Robot.GetActuatorById(encoder.Id);
                if(actuator == null)
                    continue;
                Robot.UpdateActuator(actuator, encoder.JointAngle);
            }

            StateUpdated?.Invoke(this, EventArgs.Empty);
        }

    }
}
