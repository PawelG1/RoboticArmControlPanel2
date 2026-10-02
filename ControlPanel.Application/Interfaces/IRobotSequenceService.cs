using ControlPanel.Application.DTOs;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface IRobotSequenceService
    {
        public int NextStepIdx { get;}
        public event EventHandler? OnExecuteNextStep;
        public Step RecordStep(StepType stepType, RobotStateDTO robotState, int movementSpeed);
        public Step RecordStep(StepType stepType, TimeSpan time);
        public IEnumerable<Step> GetAllSteps();
        public Step GetStep(int idx);
        public void RemoveStep(int idx);
        public void ExecuteSequence();
        public void ExecuteNextStep();
        public void ClearSequence();
        public void ExportSequence(string filePath);
        public void ImportSequence(string filePath);
    }
}
