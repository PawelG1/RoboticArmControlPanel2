using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;

namespace ControlPanel.Domain.Entities
{
    public class Robot
    {
        private readonly List<Actuator> _actuators = new();
        public DateTime? TimeOfLastHeartbeat { get; private set; }
        private readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(3);
        public RobotConfig? Config { get; private set; }
        public bool IsConfigured => Config != null;
        public bool IsConnected => (TimeOfLastHeartbeat != null 
            &&(DateTime.Now - TimeOfLastHeartbeat) < _connectionTimeout);

        public void RegisterHeartbeat()
        {
            TimeOfLastHeartbeat = DateTime.Now;
        }

        public void ApplyConfiguration(RobotConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            Config = config;
            _actuators.Clear();
            foreach (var actuator in config.AllActuators)
            {
                _actuators.Add(actuator);
            }
        }

        public Actuator? GetActuatorById(int id)
        {
            return _actuators.Find(actuator => actuator.GetId == id);
        }

        public IEnumerable<Actuator> GetAllActuators() 
        {
            return _actuators; 
        }

        public void UpdateActuator(Actuator actuator, double? currentAngle = null, ActuatorState? state = null)
        {
            if (currentAngle != null) {
                actuator.SetCurrentAngle((double)currentAngle);
            }
            if (state != null)
            {
                actuator.SetState((ActuatorState)state);
            }
        }
    }
}
