using ControlPanel.Application.DTOs;
using ControlPanel.Application.DTOs.IncomingMessages;
using ControlPanel.Application.Interfaces;
using ControlPanel.Application.Services;
using ControlPanel.Presentation.WPF.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;



namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class SequenceManagmentViewModel : SimpleRobotControlViewModel
    {
        private readonly IRobotSequenceService _robotSequenceService;

        public SequenceManagmentViewModel(IRobotSequenceService robotSequenceService, IRobotStateService robotStateService, IRobotControlService robotControlService, RobotVisualiser3DViewModel robotVisualiser3DViewModel)
            : base(robotStateService, robotControlService, robotVisualiser3DViewModel)
        {
            _robotSequenceService = robotSequenceService;

            RecordStepCommand = new RelayCommand(RecordStep);

            LoadAllRecordedSteps();
        }

        //private List<Step> steps = new();//Czy nie wartaloby uzyc Observable Collection?
        public ObservableCollection<Step> Steps;// = new();

        public ICommand RecordStepCommand { get; }
        public void RecordStep(object _)
        {
            RobotStateDTO robotStateDTO = _robotStateService.GetRobotState();
            if (!robotStateDTO.IsConfigured)
                return;
            Step recordedStep = _robotSequenceService.RecordStep(StepType.Move, robotStateDTO);
            Steps.Add(recordedStep);
        }

        private void LoadAllRecordedSteps()
        {
            IEnumerable<Step> steps = _robotSequenceService.GetAllSteps();
            foreach (Step step in steps) {
                Steps.Add(step);
            }

        }

    }
}
