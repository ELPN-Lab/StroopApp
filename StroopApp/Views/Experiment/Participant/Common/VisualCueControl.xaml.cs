using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StroopApp.Views.Experiment.Participant.Common
{
    public partial class VisualCueControl : UserControl
    {
        public static readonly DependencyProperty CueGeometryProperty =
            DependencyProperty.Register(nameof(CueGeometry), typeof(Geometry), typeof(VisualCueControl), new PropertyMetadata(null));

        public Geometry? CueGeometry
        {
            get => (Geometry?)GetValue(CueGeometryProperty);
            set => SetValue(CueGeometryProperty, value);
        }
        public VisualCueControl()
        {
            InitializeComponent();
        }
    }
}
