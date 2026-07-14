using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.ValueObjects.UrdfVisualiser
{
    public class UrdfLinkVisual
    {
        public string MeshFileAbsolutePath { get; init; } = "";
        public UrdfOrigin Origin { get; init; } = UrdfOrigin.Default;
    }
}
