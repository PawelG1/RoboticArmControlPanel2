using ControlPanel.Presentation.WPF.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace ControlPanel.WPF.Services.Interfaces
{
    public interface IUserInteractionService
    {
        void Show(BaseViewModel viewModel);
        void ShowDialog();
        UserControl GetView(BaseViewModel viewModel);
        void ShowError(string message, string title = "Error");
        public string? OpenFileDialog(string filter, string title = "Select Files");
    }
}
