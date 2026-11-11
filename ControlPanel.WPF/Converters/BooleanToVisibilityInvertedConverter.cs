using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ControlPanel.Presentation.WPF.Converters
{
    class EmptyStringToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.IsNullOrWhiteSpace(((string)value))
                ? Visibility.Visible
                : Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
