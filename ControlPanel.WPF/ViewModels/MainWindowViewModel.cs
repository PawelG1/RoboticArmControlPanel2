using ControlPanel.Presentation.WPF.Common;
using ControlPanel.WPF.Services;
using ControlPanel.WPF.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlPanel.Presentation.WPF.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IUserInteractionService _userInteractionService;
        public MainWindowViewModel(IUserInteractionService userInteractionService) {
            _userInteractionService = userInteractionService;
            OpenConfigurationWindowCommand = new RelayCommand(OpenConfigurationWindow);
            CurrentView = _userInteractionService.GetView(new HomePageViewModel());
        }

        private string _title = "Main Window";

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand OpenConfigurationWindowCommand { get; set; }

        private void OpenConfigurationWindow(object _)
        {
            CurrentView = _userInteractionService.GetView(new ConfigurationViewModel());
        }

    }
}
