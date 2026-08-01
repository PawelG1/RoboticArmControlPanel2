using ControlPanel.Domain.Enums;

namespace ControlPanel.Domain.ValueObjects
{
    public readonly struct Step
    {
        public StepType StepType { get; init; }
        public RobotPose? Pose { get; init; }
        public TimeSpan? Delay { get; init; }
        public int MovementSpeed { get; init; }

        public Step(StepType stepType, RobotPose pose, int movementSpeed)
        {
            if (stepType == StepType.Wait)
                throw new InvalidOperationException($"Cannot Create Step For this type: {stepType}, with given parameters");
            this.StepType = stepType;
            this.Pose = pose;
            this.MovementSpeed = movementSpeed;
        }

        public Step(StepType stepType, TimeSpan delay)
        {
            if (stepType == StepType.Move)
                throw new InvalidOperationException($"Cannot Create Step For this type: {stepType}, with given parameters");
            this.StepType = stepType;
            this.Delay = delay;
        }
    }
}
