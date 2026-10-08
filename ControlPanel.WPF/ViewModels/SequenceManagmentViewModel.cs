using ControlPanel.Application.DTOs;
using ControlPanel.Application.Interfaces;
using ControlPanel.Domain.Entities;
using ControlPanel.Domain.Enums;
using ControlPanel.Domain.ValueObjects;
using ControlPanel.Presentation.WPF.Common;
using ControlPanel.WPF.Services;
using ControlPanel.WPF.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;



namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class SequenceManagmentViewModel : SimpleRobotControlViewModel
    {
        private readonly IRobotSequenceService _robotSequenceService;
        private readonly IUserInteractionService _userInteractionService;

        public SequenceManagmentViewModel(IUserInteractionService userInteractionService, IRobotSequenceService robotSequenceService, IRobotStateService robotStateService,
            IRobotControlService robotControlService, RobotVisualiser3DViewModel robotVisualiser3DViewModel)
            : base(robotStateService, robotControlService, robotVisualiser3DViewModel)
        {
            _robotSequenceService = robotSequenceService;
            _userInteractionService = userInteractionService;

            RecordStepCommand = new RelayCommand(RecordStep);
            ExportSequenceFileCommand = new RelayCommand(ExportSequenceFile);
            ImportSequenceFileCommand = new RelayCommand(ImportSequenceFile);
            ClearSequenceCommand = new RelayCommand(ClearSequence);
            ExecuteNextStepCommand = new RelayCommand(ExecuteNextStep);
            ExecuteSequenceCommand = new RelayCommand(ExecuteSequence);
            StopSequenceCommand = new RelayCommand(StopSequence);
            AddDelayStepCommand = new RelayCommand(AddDelayStep);

            LoadAllRecordedSteps();

            _robotSequenceService.OnExecuteNextStep += OnNextStepChanged;
        }

        public override void Dispose()
        {
            _robotSequenceService.OnExecuteNextStep -= OnNextStepChanged;
            base.Dispose();
        }

        public ObservableCollection<SequenceStepViewModel> Steps { get; } = new();

        private void OnNextStepChanged(object? sender, EventArgs e)
        {
            for (int i = 0; i < Steps.Count; i++) {
                Steps[i].IsNext = (i == _robotSequenceService.NextStepIdx);
            }
        }

        public ICommand RecordStepCommand { get; }
        public void RecordStep(object _)
        {
            RobotStateDTO robotStateDTO = _robotStateService.GetRobotState();
            if (!robotStateDTO.IsConfigured)
                return;
            //TODO: get speed set by user on UI
            int defaultSpeed = 200;
            Step recordedStep = _robotSequenceService.RecordStep(StepType.Move, robotStateDTO, defaultSpeed);
            Steps.Add(new SequenceStepViewModel(recordedStep));
        }

        public ICommand ExportSequenceFileCommand { get; }
        public void ExportSequenceFile(object _)
        {
            if (_robotSequenceService.GetAllSteps().Count() == 0)
            {
                _userInteractionService.ShowError("There are no recorded steps to export.");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = $"Sequence_{DateTime.Now.ToShortDateString().Replace("/","_")}";
            dialog.DefaultExt = ".json";
            dialog.Filter = "Json files (*.json)|*.json"; //TODO:maybe store these in SequenceService
            dialog.CheckPathExists = true;
        
            bool? result = dialog.ShowDialog();
            if(result == true && dialog.CheckPathExists)
            {
                try
                {
                    _robotSequenceService.ExportSequence(dialog.FileName);
                }catch(Exception ex)
                {
                    _userInteractionService.ShowError(ex.ToString());
                }
            }
        }
        public ICommand ImportSequenceFileCommand {  get; }
        public void ImportSequenceFile(object _)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".json";
            dialog.Filter = "Json files (*.json)|*.json";
            dialog.CheckFileExists = true;

            bool? result = dialog.ShowDialog();
            if( result == true)
            {
                try
                {
                    _robotSequenceService.ImportSequence(dialog.FileName);
                    LoadAllRecordedSteps(); 
                }
                catch (Exception ex)
                {
                    _userInteractionService.ShowError(ex.ToString());
                }
            }
        }

        public ICommand ClearSequenceCommand { get; }

        public void ClearSequence(object _)
        {
            _robotSequenceService.ClearSequence();
            LoadAllRecordedSteps();
        }

        public ICommand ExecuteNextStepCommand { get; }
        public async void ExecuteNextStep(object _)
        {
            try
            {
                await _robotSequenceService.ExecuteNextStepAsync();
            }
            catch(Exception ex)
            {
                _userInteractionService.ShowError(ex.ToString());
            }
        }

        private CancellationTokenSource? _sequenceCancellationTokenSource;
        private bool _isExecutingSequence;
        public bool IsExecutingSequence
        {
            get => _isExecutingSequence;
            set
            {
                _isExecutingSequence = value;
                NotifyPropertyChanged(nameof(IsExecutingSequence));
            }
        }

        private bool _isLooping;
        public bool IsLooping
        {
            get => _isLooping;
            set
            {
                _isLooping = value;
                NotifyPropertyChanged(nameof(IsLooping));
            }
        }

        public ICommand ExecuteSequenceCommand { get; }
        public ICommand StopSequenceCommand { get; }

        public void StopSequence(object _)
        {
            _sequenceCancellationTokenSource?.Cancel();
        }

        public async void ExecuteSequence(object _)
        {
            if (IsExecutingSequence) return;
            try
            {
                IsExecutingSequence = true;
                _sequenceCancellationTokenSource = new CancellationTokenSource();
                await _robotSequenceService.ExecuteSequence(IsLooping, _sequenceCancellationTokenSource.Token);
            }
            catch (TaskCanceledException) { }
            catch(Exception ex)
            {
                _userInteractionService.ShowError(ex.ToString());
            }
            finally
            {
                IsExecutingSequence = false;
                _sequenceCancellationTokenSource?.Dispose();
                _sequenceCancellationTokenSource = null;
            }
        }

        private double _delaySeconds = 1.0;
        public double DelaySeconds
        {
            get => _delaySeconds;
            set
            {
                _delaySeconds = value;
                NotifyPropertyChanged(nameof(DelaySeconds));
            }
        }

        public ICommand AddDelayStepCommand { get; }
        public void AddDelayStep(object _)
        {
            Step recordedStep = _robotSequenceService.RecordStep(StepType.Wait, TimeSpan.FromSeconds(DelaySeconds));
            Steps.Add(new SequenceStepViewModel(recordedStep));
        }

        private void LoadAllRecordedSteps()
        {
            IEnumerable<Step> steps = _robotSequenceService.GetAllSteps();
            Steps.Clear();
            foreach (Step step in steps) {
                SequenceStepViewModel stepVM = new SequenceStepViewModel(step);
                Steps.Add(stepVM);
            }
            OnNextStepChanged(this, EventArgs.Empty);
        }

    }
}
