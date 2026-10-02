using ControlPanel.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class SequenceStepViewModel : INotifyPropertyChanged
    {
        public Step Step { get; }
        public SequenceStepViewModel(Step step)
        {
            this.Step = step;
        }

        private bool _isNext;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsNext
        {
            get => _isNext;
            set
            {
                if (_isNext != value)
                {
                    _isNext = value;
                }
                PropertyChanged?.Invoke(this, new(nameof(IsNext)));
            }
        }
    }
}
