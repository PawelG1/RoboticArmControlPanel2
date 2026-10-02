namespace ControlPanel.Domain.ValueObjects
{
    public readonly struct ActuatorTarget
    {
        public int ActuatorId { get; init; }
        public double TargetAngle { get; init; }

        public ActuatorTarget(int actuatorId, double targetAngle)
        {
            ActuatorId = actuatorId;
            TargetAngle = targetAngle;
        }
    }
}
