using StroopApp.Models.Simon;
using StroopApp.Resources;
using System.Globalization;
using System.Windows.Data;

namespace StroopApp.Converters
{
    public class SimonShapeToGeometryConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            SimonStimulusShape.Circle => ShapesGeometries.Circle,
            SimonStimulusShape.Square => ShapesGeometries.Square,
            SimonStimulusShape.Triangle => ShapesGeometries.Triangle,
            SimonStimulusShape.LeftArrow => ShapesGeometries.LeftArrow,
            SimonStimulusShape.RightArrow => ShapesGeometries.RightArrow,
            _ => null,
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
