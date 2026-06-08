using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.ValueObjects.UrdfVisualiser
{
    public class JointAxis
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public JointAxis(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
