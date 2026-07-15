using ControlPanel.Application.DTOs;
using ControlPanel.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Application.Services
{
    public enum StepType
    {
        Move,
        Wait,
    }

    public readonly struct Step { //TODO: trzeba chyba przeniesc w odpowiednie miejsca Step i StepType, czy do domain?
        public StepType StepType { get; }
        public RobotStateDTO? RobotStateDTO {  get; }
        public TimeSpan? Delay { get; }
        public Step(StepType stepType, RobotStateDTO robotStateDTO){
            if (stepType == StepType.Wait)
                throw new InvalidOperationException($"Cannot Create Step For this type: {stepType}, with given parameters");
            this.StepType = stepType;
            this.RobotStateDTO = robotStateDTO;
        }

        public Step( StepType stepType, TimeSpan delay)
        {
            if (stepType == StepType.Move)
                throw new InvalidOperationException($"Cannot Create Step For this type: {stepType}, with given parameters");
            this.StepType = stepType;
            this.Delay = delay;
        }
    }

    public class RobotSequenceService : IRobotSequenceService
    {
        private readonly ISequenceRepository _sequenceRepository;
        private List<Step> _steps = new();
        public RobotSequenceService(ISequenceRepository sequenceRepository) {
            _sequenceRepository = sequenceRepository;
        }

        public Step RecordStep(StepType stepType, RobotStateDTO robotState)
        {
            Step newStep = new Step(stepType, robotState);
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

        public void RemoveStep(int idx)
        {
            throw new NotImplementedException();
        }

        public Task MoveToStep(int idx)
        {
            throw new NotImplementedException();
        }


        public Task ExecuteSequence()
        {
            throw new NotImplementedException();
        }

        public void ClearSequence()
        {
            throw new NotImplementedException();
        }

        public void ExportSequence(string filePath)
        {
            _sequenceRepository.ExportSequenceFile(_steps, filePath);
        }

        public void ImportSequence()
        {
            throw new NotImplementedException();
        }

    }
}
