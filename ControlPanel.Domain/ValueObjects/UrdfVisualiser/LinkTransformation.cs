
using System.Numerics;

namespace ControlPanel.Domain.ValueObjects.UrdfVisualiser
{
    public record LinkTransformation
    {
        public string LinkName { get; init; } = "";
        public string VisualKey { get; init; } = "";
        public Matrix4x4 WorldMatrix { get; init; }
    }
}
