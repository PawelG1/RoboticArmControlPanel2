using ControlPanel.Domain.Entities.UrdfVisualiser;
using ControlPanel.Domain.ValueObjects.UrdfVisualiser;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface IForwardKinematicsService
    {
        IReadOnlyList<LinkTransformation> Compute(UrdfRobot urdfRobot, IReadOnlyList<JointState> jointStates);
    }
}
