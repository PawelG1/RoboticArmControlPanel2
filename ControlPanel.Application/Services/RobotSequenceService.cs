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

        public Step RecordStep(StepType stepType, IEnumerable<ActuatorTarget> targets)
        {
            ActuatorTarget[] recordedTargets = targets.ToArray();
            int legacySpeed = recordedTargets.Select(target => target.Speed).FirstOrDefault(speed => speed is >= 1 and <= 255);
            if (legacySpeed == 0)
                legacySpeed = 200;

            Step newStep = new Step(stepType, new RobotPose(recordedTargets), legacySpeed);
            _steps.Add(newStep);
            return newStep;
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

        public async Task ExecuteSequence(bool loop = false, CancellationToken cancellationToken = default)
        {
            int count = _steps.Count;
            if (count == 0) return;
            
            // If we are at the end, wrap to start
            if (NextStepIdx >= count) NextStepIdx = 0;
            
            int startIdx = NextStepIdx;
            do
            {
                if (cancellationToken.IsCancellationRequested) break;
                await ExecuteNextStepAsync(cancellationToken);
            } while (loop || (NextStepIdx != 0 && NextStepIdx != startIdx));
        }

        public async Task ExecuteNextStepAsync(CancellationToken cancellationToken = default)
        {
            if (_steps.Count == 0 || NextStepIdx >= _steps.Count) return;

            Step nextStep = GetStep(NextStepIdx);
            
            if (nextStep.StepType == StepType.Wait && nextStep.Delay.HasValue)
            {
                await Task.Delay(nextStep.Delay.Value, cancellationToken);
            }
            else if (nextStep.StepType == StepType.Move && nextStep.Pose != null)
            {
                foreach (ActuatorTarget target in nextStep.Pose.ActuatorTargets)
                {
                    int speed = target.Speed is >= 1 and <= 255
                        ? target.Speed
                        : nextStep.MovementSpeed;
                    if (speed < 1 || speed > 255)
                        speed = 200;

                    await _robotControlService.MoveActuator(target.ActuatorId, target.TargetAngle, speed);
                }

                // Wait for movement to finish
                // First wait a bit for the firmware to start moving and send INFO (MOVING)
                await Task.Delay(200, cancellationToken);

                // Wait for movement to finish
                await Task.Delay(200, cancellationToken);

                bool isMoving = true;
                while (isMoving)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await Task.Delay(100, cancellationToken);
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

            if (!cancellationToken.IsCancellationRequested)
            {
                NextStepIdx++;
            }
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
