using StroopApp.Models;
using StroopApp.Resources;
using System.Globalization;
using System.Windows.Data;

namespace StroopApp.Converters
{
    public class VisualCueTypeToGeometryConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            VisualCueType.Round => ShapesGeometries.Circle,
            VisualCueType.Square => ShapesGeometries.Square,
            VisualCueType.None => null,
            _ => null,
        };


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
