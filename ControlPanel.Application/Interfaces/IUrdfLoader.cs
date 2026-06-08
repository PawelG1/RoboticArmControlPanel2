using ControlPanel.Domain.Entities.UrdfVisualiser;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface IUrdfLoader
    {
        UrdfRobot Load(string filePath);
    }
}
