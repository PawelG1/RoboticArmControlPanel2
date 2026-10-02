namespace ControlPanel.Domain.ValueObjects
{
    public sealed class RobotPose
    {
        public IReadOnlyList<ActuatorTarget> ActuatorTargets { get; init; } = new List<ActuatorTarget>();

        public RobotPose() { }

        public RobotPose(IEnumerable<ActuatorTarget> actuatorTargets)
        {
            ActuatorTargets = actuatorTargets.ToList().AsReadOnly();
        }
    }
}
