using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.DTOs.IncomingMessages
{
    public class ActuatorInfoDto
    {
        public string Type { get; set; } = "";
        public string ManipulatedObject { get; set; } = "";
        public int ObjectIdx { get; set; }
        public ActuatorInfoValuesDto Values { get; set; } = new();
    }
    public class ActuatorInfoValuesDto
    {
        public double CurrentAngle { get; set; }
        public double TargetAngle { get; set; }
        public ActuatorStatusWire Status { get; set; }
    }
    public enum ActuatorStatusWire
    {
        MOVING,
        IDLE,
        FORBIDDEN,
        ESTOP,
        ACCEPTED
    }
}

