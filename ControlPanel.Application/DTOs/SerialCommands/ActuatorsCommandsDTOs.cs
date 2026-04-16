using ControlPanel.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.DTOs.SerialCommands
{
    public record RunActuatorWireDto(int id, int speed, RotatingDirection direction);
    public record MoveActuatorWireDto(int id, double targetAngle, RotatingDirection direction);
    public record StopActuatorWireDto(int id);

}
