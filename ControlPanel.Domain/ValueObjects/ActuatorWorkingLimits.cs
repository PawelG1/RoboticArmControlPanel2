using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.ValueObjects
{
    public struct ActuatorWorkingLimits
    {
        private readonly double _minAngle;
        private readonly double _maxAngle;

        public ActuatorWorkingLimits(double minAngle, double maxAngle)
        {
            if ( minAngle > maxAngle)
            {
                throw new ArgumentException("Invalid actuator limits. Min angle must be less than or equal to max angle.");
            }
            _minAngle = minAngle;
            _maxAngle = maxAngle;
        }

        public double MinAngle => _minAngle;
        public double MaxAngle => _maxAngle;
    }
}
