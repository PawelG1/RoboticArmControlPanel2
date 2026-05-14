using ControlPanel.Presentation.WPF.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        public HomePageViewModel() {
        
        }

        private string _title = "Welcome to the Control Panel!";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }
    }
}
