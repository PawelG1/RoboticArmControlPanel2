namespace ControlPanel.Application.DTOs.SerialCommands
{

    public record MoveActuatorWireDTO() { 
        public string Type => "MOVE";
        public string ManipulatedObject => "ACTUATOR"; 
        public int ObjectIdx { get; set; }
        public double TargetAngle { get; set; }
        public int Speed { get; set; }
    }
    public record StopActuatorWireDTO() {
        public string Type => "STOP";
        public string ManipulatedObject => "ACTUATOR";
        public int ObjectIdx { get; set; }
    }
    public record StopAllWireDTO()
    {
        public string Type => "STOP";
        public string ManipulatedObject => "ALL";
    }

}
