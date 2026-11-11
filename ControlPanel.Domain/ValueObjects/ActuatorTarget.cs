namespace ControlPanel.Domain.ValueObjects
{
    public readonly struct ActuatorTarget
    {
        public int ActuatorId { get; init; }
        public double TargetAngle { get; init; }
        public int Speed { get; init; }

        public ActuatorTarget(int actuatorId, double targetAngle, int speed = 0)
        {
            ActuatorId = actuatorId;
            TargetAngle = targetAngle;
            Speed = speed < 1 ? 0 : Math.Min(speed, 255);
        }
    }
}
