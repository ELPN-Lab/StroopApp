using CommunityToolkit.Mvvm.ComponentModel;
using StroopApp.Core;
using StroopApp.Models.Simon;
using System.Windows.Media;

namespace StroopApp.ViewModels.Experiment.Participant.Simon
{
    public partial class SimonStimulusViewModel : ViewModelBase
    {
        [ObservableProperty]
        private StimulusPosition _position;

        [ObservableProperty]
        private SimonStimulusShape _shape;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ForegroundBrush))]
        private string _color = "";

        [ObservableProperty]
        private SimonStimulusShape? _aroundShape = null;

        public Brush ForegroundBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));
                }
                catch
                {
                    return Brushes.White;
                }
            }
        }

        public SimonStimulusViewModel(StimulusPosition position, string color, 
                                      SimonStimulusShape shape,
                                      SimonStimulusShape? aroundShape=null)
        {
            Position = position;
            Color = color;
            Shape = shape;
            AroundShape = aroundShape;
        }
    }
}
