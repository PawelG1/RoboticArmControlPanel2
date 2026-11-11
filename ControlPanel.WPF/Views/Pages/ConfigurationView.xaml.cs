using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ControlPanel.Presentation.WPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigurationView.xaml
    /// </summary>
    public partial class ConfigurationView : UserControl
    {
        private readonly Dictionary<TextBox, bool> _logAutoScrollStates = new();

        public ConfigurationView()
        {
            InitializeComponent();
            AddHandler(
                ScrollViewer.ScrollChangedEvent,
                new ScrollChangedEventHandler(LogTextBox_ScrollChanged));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox &&
                (!_logAutoScrollStates.TryGetValue(textBox, out bool autoScroll) || autoScroll))
            {
                textBox.ScrollToEnd();
            }
        }

        private void LogTextBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0 && FindParent<TextBox>(e.OriginalSource as DependencyObject) is TextBox textBox)
            {
                _logAutoScrollStates[textBox] =
                    e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - 1;
            }
        }

        private static T? FindParent<T>(DependencyObject? element)
            where T : DependencyObject
        {
            while (element is not null)
            {
                if (element is T result)
                {
                    return result;
                }

                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }
    }
}
