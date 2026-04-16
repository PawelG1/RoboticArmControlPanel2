using ControlPanel.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Exceptions
{
    internal class IncorrectTargetAngleParsedException: Exception
    {
        public IncorrectTargetAngleParsedException(double angle, ActuatorWorkingLimits workLimits) 
            : base($"Incorrect target angle parsed: {angle}, the angle must be within the valid range {workLimits.MinAngle} to {workLimits.MaxAngle}.")
        {
        }
    }
}
