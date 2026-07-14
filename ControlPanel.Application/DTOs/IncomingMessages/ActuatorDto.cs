using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.DTOs.IncomingMessages
{
    public class ActuatorDto
    {
        public string ManipulatedObject { get; set; } = "";
        public int ObjectIdx { get; set; }
        public ActuatorStateDto ActuatorState { get; set; } = new();
    }
    public class ActuatorStateDto
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
    }
}

