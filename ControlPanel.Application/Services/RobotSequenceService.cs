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
        private readonly IRobotStateService _robotStateService;
        private List<Step> _steps = new();
        public event EventHandler? OnExecuteNextStep;

        public RobotSequenceService(ISequenceRepository sequenceRepository, IRobotControlService robotControlService, IRobotStateService robotStateService)
        {
            _sequenceRepository = sequenceRepository;
            _robotControlService = robotControlService;
            _robotStateService = robotStateService;
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

        public async Task ExecuteSequence()
        {
            int count = _steps.Count;
            if (count == 0) return;
            
            // If we are at the end, wrap to start
            if (NextStepIdx >= count) NextStepIdx = 0;
            
            int startIdx = NextStepIdx;
            do
            {
                await ExecuteNextStepAsync();
            } while (NextStepIdx != 0 && NextStepIdx != startIdx);
        }

        public async Task ExecuteNextStepAsync()
        {
            if (_steps.Count == 0 || NextStepIdx >= _steps.Count) return;

            Step nextStep = GetStep(NextStepIdx);
            
            if (nextStep.StepType == StepType.Wait && nextStep.Delay.HasValue)
            {
                await Task.Delay(nextStep.Delay.Value);
            }
            else if (nextStep.StepType == StepType.Move && nextStep.Pose != null)
            {
                int movementSpeed = nextStep.MovementSpeed;
                if (movementSpeed > 255) movementSpeed = 200; // firmware limit

                foreach (ActuatorTarget target in nextStep.Pose.ActuatorTargets)
                {
                    await _robotControlService.MoveActuator(target.ActuatorId, target.TargetAngle, movementSpeed);
                }

                // Wait for movement to finish
                // First wait a bit for the firmware to start moving and send INFO (MOVING)
                await Task.Delay(200);

                // Wait for movement to finish
                await Task.Delay(200);

                bool isMoving = true;
                while (isMoving)
                {
                    await Task.Delay(100);
                    isMoving = false;
                    
                    var robotState = _robotStateService.GetRobotState();
                    foreach (var actuator in robotState.Actuators)
                    {
                        if (actuator.ActuatorState.Status == ControlPanel.Application.DTOs.IncomingMessages.ActuatorStatusWire.MOVING)
                        {
                            isMoving = true;
                            break;
                        }
                    }
                }
            }

            NextStepIdx++;
        }

        public void ExecuteNextStep()
        {
            _ = ExecuteNextStepAsync();
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
