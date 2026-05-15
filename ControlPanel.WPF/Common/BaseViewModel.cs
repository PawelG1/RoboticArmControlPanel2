using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ControlPanel.Presentation.WPF.Common
{
    public class BaseViewModel : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual void Dispose() { }

        public void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
