namespace ControlPanel.Application.DTOs.IncomingMessages
{
    public class RobotConfigDTO
    {
        public string Type { get; set; } = "";
        public string ManipulatedObject { get; set; } = "";
        public List<ActuatorConfigDTO> Steppers { get; set; } = new();
        public List<ActuatorConfigDTO> Servos { get; set; } = new();
        public Dictionary<string, int> ConfigurablePins { get; set; } = new();
    }

    public class ActuatorConfigDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
