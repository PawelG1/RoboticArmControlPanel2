using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.ValueObjects.UrdfVisualiser
{
    public record UrdfOrigin
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double Z { get; init; }
        public double Roll { get; init; }
        public double Pitch { get; init; }
        public double Yaw { get; init;}

        public static UrdfOrigin Default=> new UrdfOrigin
        {
            X = 0,
            Y = 0,
            Z = 0,
            Roll = 0,
            Pitch = 0,
            Yaw = 0
        };
    }
}
