using ControlPanel.Domain.ValueObjects.UrdfVisualiser;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Domain.Entities.UrdfVisualiser
{
    public class UrdfLink
    {
        public string Name { get; init; } = "";
        public UrdfLinkVisual? Visual { get; init; }

        public UrdfLink(string name, UrdfLinkVisual? visual)
        {
            Name = name;
            Visual = visual;
        }

    }
}
