using ControlPanel.Application.DTOs;
using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Services
{
    public class RobotSequenceService : IRobotSequenceService
    {
        private readonly ISequenceRepository _sequenceRepository;
        private readonly IRobotControlService _robotControlService;
        private List<Step> _steps = new();
        public event EventHandler? OnExecuteNextStep;

        public RobotSequenceService(ISequenceRepository sequenceRepository, IRobotControlService robotControlService)
        {
            _sequenceRepository = sequenceRepository;
            _robotControlService = robotControlService;
            NextStepIdx = 0;
        }

        private int _nextStepIdx;
        public int NextStepIdx
        {
            get
            {
                return _nextStepIdx;
            }
            private set
            {
                if (value > _steps.Count-1)
                    value = 0;
                _nextStepIdx = value;
                OnExecuteNextStep?.Invoke(this, new EventArgs());
            }
        }

        public Step RecordStep(StepType stepType, RobotStateDTO robotState, int movementSpeed)
        {
            RobotPose pose = ToPose(robotState);
            Step newStep = new Step(stepType, pose, movementSpeed);
            _steps.Add(newStep);
            return newStep;
        }

        private static RobotPose ToPose(RobotStateDTO robotState)
        {
            IEnumerable<ActuatorTarget> targets = robotState.Actuators
                .Where(actuator => actuator?.ActuatorState != null)
                .Select(actuator => new ActuatorTarget(actuator.ObjectIdx, actuator.ActuatorState.TargetAngle));
            return new RobotPose(targets);
        }

        public Step RecordStep(StepType stepType, TimeSpan time)
        {
            Step newStep = new Step(stepType, time);
            _steps.Add(newStep);
            return newStep;
        }
        public IEnumerable<Step> GetAllSteps()
        {
            return _steps;
        }

        public Step GetStep(int idx)
        {
            return _steps.ElementAt(idx);
        }

        public void RemoveStep(int idx)
        {
            throw new NotImplementedException();
        }

        public void ExecuteSequence()
        {
            throw new NotImplementedException();
        }

        public void ExecuteNextStep()
        {
            Step nextStep = GetStep(NextStepIdx);
            int movementSpeed = nextStep.MovementSpeed;
            if (nextStep.Pose == null)
                return;
            foreach (ActuatorTarget target in nextStep.Pose.ActuatorTargets)
            {
                _robotControlService.MoveActuator(target.ActuatorId, target.TargetAngle, movementSpeed);
            }
            NextStepIdx++;
        }

        public void ClearSequence()
        {
            _steps.Clear();
            NextStepIdx = 0;
        }

        public void ExportSequence(string filePath)
        {
            if (_steps.Count == 0)
                throw new ArgumentException("There are no recorded steps to export.");
            _sequenceRepository.ExportSequenceFile(_steps, filePath);
        }

        public void ImportSequence(string filePath)
        {
            _steps = _sequenceRepository.ImportSequenceFile(filePath);
            NextStepIdx = 0;
        }

    }
}
