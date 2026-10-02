using System.Windows;

namespace ControlPanel.Presentation.WPF.Views.Controls
{
    public partial class ErrorMessageBox : Window
    {
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(ErrorMessageBox),
                new PropertyMetadata(string.Empty));

        public ErrorMessageBox(string message, string title)
        {
            InitializeComponent();
            Title = title;
            Message = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
