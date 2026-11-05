using StroopApp.ViewModels.Experiment.Experimenter.End;
using System.Windows;

namespace StroopApp.Views.Experiment.Experimenter.End
{
    /// <summary>
    /// Details of a calibration result; closes with a true dialog result when the experimenter asks for the export.
    /// </summary>
    public partial class CalibrationDetailsWindow : Window
    {
        public CalibrationDetailsWindow(CalibrationResultViewModel calibrationResult)
        {
            InitializeComponent();
            DataContext = calibrationResult;
        }

        private void Export_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
