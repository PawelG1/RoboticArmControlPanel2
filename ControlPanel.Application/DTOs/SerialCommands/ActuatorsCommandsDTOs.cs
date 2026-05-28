namespace ControlPanel.Application.DTOs.SerialCommands
{

    public record MoveActuatorWireDto() { 
        public string Type => "MOVE";
        public string ManipulatedObject => "ACTUATOR"; 
        public int ObjectIdx { get; set; }
        public double TargetAngle { get; set; }
        public int Speed { get; set; }
    }
    public record StopActuatorWireDto() {
        public string Type => "STOP";
        public string ManipulatedObject => "ACTUATOR";
        public int ObjectIdx { get; set; }
    }
    public record StopAllWireDto()
    {
        public string Type => "STOP";
        public string ManipulatedObject => "ALL";
    }

}
