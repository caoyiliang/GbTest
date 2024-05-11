using Config;
using System.Globalization;
using System.Windows.Data;

namespace GbTest.Converter
{
    internal class SelectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (CommunicationType)value;
        }
    }
}
