using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Exceptions
{
    internal class IncorrectActuatorSpeedException : Exception
    {
        public IncorrectActuatorSpeedException(double speed)
            : base($"Incorrect actuator speed provided: {speed}. Speed must be in range 1..255.")
        {
        }
    }
}
