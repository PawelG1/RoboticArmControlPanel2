using ControlPanel.Application.DTOs;
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
            ExportFileCommand = new RelayCommand(ExportFile);

            LoadAllRecordedSteps();//TODO: initialize Steps
        }

        //TODO: private List<Step> steps = new();//Czy nie wartaloby uzyc Observable Collection?
        public ObservableCollection<Step> Steps { get; } = new();

        public ICommand RecordStepCommand { get; }
        public void RecordStep(object _)
        {
            RobotStateDTO robotStateDTO = _robotStateService.GetRobotState();
            if (!robotStateDTO.IsConfigured)
                return;
            Step recordedStep = _robotSequenceService.RecordStep(StepType.Move, robotStateDTO);
            Steps.Add(recordedStep);
        }

        public ICommand ExportFileCommand { get; }
        public void ExportFile(object _)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = $"Sequence_{DateTime.Now.ToShortDateString()}";
            dialog.DefaultExt = ".json";
            dialog.Filter = "Json files (*.json)|*.json"; //TODO:maybe store these in SequenceService
        
            bool? result = dialog.ShowDialog();
            if(result == true && dialog.CheckPathExists)
            {
                _robotSequenceService.ExportSequence(dialog.FileName);
            }
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
