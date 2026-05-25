using ControlPanel.Application.DTOs.IncomingMessages;
using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.ValueObjects;
using System.Diagnostics;
using System.Text.Json;

namespace ControlPanel.Application.Services
{
    public class RobotStateService : IRobotStateService
    {
        private readonly ISerialCommunication _serialCommunication;

     
        public Robot Robot { get; }

        public event EventHandler? StateUpdated;
        public event EventHandler? RobotConfigured;
        private bool _configRequested = false;

        private static readonly ActuatorWorkingLimits DefaultStepperLimits = new(0, 360);   
        private static readonly ActuatorWorkingLimits DefaultServoLimits = new(0, 180);   

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
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

            var steppers = dto.Steppers.Select(s => new Actuator(s.Id, DefaultStepperLimits));
            var servos = dto.Servos.Select(s => new Actuator(s.Id, DefaultServoLimits));
            var config = new RobotConfig(steppers, servos, dto.ConfigurablePins.AsReadOnly());

            _configRequested = false;
            Robot.ApplyConfiguration(config);
            RobotConfigured?.Invoke(this, EventArgs.Empty);
        }

        private async Task RequestRobotConfig()
        {   
            var request = new { Type = "GET_CONFIG", ManipulatedObject = "SYSTEM"};
            string json = JsonSerializer.Serialize(request);
            await _serialCommunication.SendJsonLineAsync(json);
            
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
                Robot.UpdateActuator(actuator, encoder.Angle);
            }

            StateUpdated?.Invoke(this, EventArgs.Empty);
        }

    }
}
