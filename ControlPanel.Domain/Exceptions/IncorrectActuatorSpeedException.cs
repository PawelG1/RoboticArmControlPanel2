using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Exceptions
{
    internal class IncorrectActuatorSpeedException : Exception
    {
        public IncorrectActuatorSpeedException(double speed)
            : base($"Incorrect actuator speed provided: {speed}. Speed must be a non-negative value. " +
                  $"\n if the direction should be changed, please change the direction paramter instead of the speed.")
        {
        }
    }
}
