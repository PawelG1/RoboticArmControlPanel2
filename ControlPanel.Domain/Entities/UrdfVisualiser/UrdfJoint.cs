using ControlPanel.Domain.Enums.UrdfVisualiser;
using ControlPanel.Domain.ValueObjects.UrdfVisualiser;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Entities.UrdfVisualiser
{
    public class UrdfJoint
    {
        public string Name { get; init; } = "";
        public JointType Type { get; init; }
        public string ParentLinkName { get; init; } = "";
        public string ChildLinkName { get; init; } = "";
        public UrdfOrigin Origin { get; init; } = UrdfOrigin.Default;
        
        public JointAxis JointAxis { get; init; } = new JointAxis(0, 0, 0);
        public double LimitLowerDeg { get; set; } = 0;
        public double LimitUpperDeg { get; set; } = 0;

        public bool IsMovable => Type is JointType.Revolute 
                                    or JointType.Prismatic
                                    or JointType.Continuous;

        public UrdfJoint(string name, JointType type, string parentLinkName, string childLinkName, UrdfOrigin origin, JointAxis jointAxis)
        {
            Name = name;
            Type = type;
            ParentLinkName = parentLinkName;
            ChildLinkName = childLinkName;
            Origin = origin;
            JointAxis = jointAxis;
        }

    }
}
