using ControlPanel.Presentation.WPF.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class ConfigurationViewModel : BaseViewModel
    {
        public ConfigurationViewModel() {
            _title = "Configuration Window";
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }
    }
}
