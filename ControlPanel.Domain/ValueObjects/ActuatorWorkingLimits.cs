using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.ValueObjects
{
    public struct ActuatorWorkingLimits
    {
        private readonly double _minAngle;
        private readonly double _maxAngle;
        private static readonly Tuple<double, double> DefaultLimits = new(0, 180);

        public ActuatorWorkingLimits(double minAngle, double maxAngle)
        {
            if(minAngle == 0 && maxAngle == 0)
            {
                minAngle = DefaultLimits.Item1;
                maxAngle = DefaultLimits.Item2;
            }
            //if ( minAngle > maxAngle)
            //{
            //    double temp = minAngle;
            //    minAngle = maxAngle;
            //    maxAngle = temp;
            //    //throw new ArgumentException("Invalid actuator limits. Min angle must be less than or equal to max angle.");
            //}
            _minAngle = minAngle;
            _maxAngle = maxAngle;
        }

        public double MinAngle => _minAngle;
        public double MaxAngle => _maxAngle;
    }
}
