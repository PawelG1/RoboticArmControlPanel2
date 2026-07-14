using ControlPanel.Application.DTOs;
using ControlPanel.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Interfaces
{
    public interface IRobotSequenceService
    {
        public Step RecordStep(StepType stepType, RobotStateDTO robotState);
        public Step RecordStep(StepType stepType, TimeSpan time);
        public IEnumerable<Step> GetAllSteps();
        public void RemoveStep(int idx);
        public Task MoveToStep(int idx);

        public Task ExecuteSequence();
        public void ClearSequence();
        public void SaveSequence();
        public void ImportSequence();
    }
}
