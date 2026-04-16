using ControlPanel.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Exceptions
{
    internal class IncorrectWorkingLimitsExceptionsException: Exception
    {
        public IncorrectWorkingLimitsExceptionsException(ActuatorWorkingLimits workingLimits) 
            : base($"Incorrect working limits provided: min angle {workingLimits.MinAngle} cannot be greater than max angle {workingLimits.MaxAngle}.")
        {
        }
    }
}
