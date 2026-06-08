using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Entities.UrdfVisualiser
{
    public class ActuatorAndJointNamePair
    {
        public Actuator? Acutator { get; set; }
        public string JointName { get; set; } = "";

        public ActuatorAndJointNamePair(Actuator? actuator, string jointName) {
            Acutator = actuator;
            JointName = jointName;
        }
    }


}
