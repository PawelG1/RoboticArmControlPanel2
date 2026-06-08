using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.ValueObjects.UrdfVisualiser
{
    public record JointState
    {
        public string JointName { get; init; } = "";
        public double AngleDegrees { get; init; }
    }
}
