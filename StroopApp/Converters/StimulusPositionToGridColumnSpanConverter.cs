using StroopApp.Models.Simon;
using System.Globalization;
using System.Windows.Data;

namespace StroopApp.Converters
{
    public class StimulusPositionToGridColumnSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is StimulusPosition.Center ? 3 : 1;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
