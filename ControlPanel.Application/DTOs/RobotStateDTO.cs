using ControlPanel.Application.DTOs.IncomingMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.DTOs
{
    public record RobotStateDTO
    {
        public List<ActuatorDto> Actuators { get; set; } = new();
        public bool IsConfigured;
    }
}
