using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StroopApp.Models.Calibration;
using StroopApp.Resources;
using StroopApp.Services.Calibration;
using System.Globalization;
using System.Windows;

namespace StroopApp.ViewModels.Experiment.Experimenter.End
{
    /// <summary>
    /// Calibration result shown on the end page and in its details window, with the slider that reads the curve at another accuracy.
    /// </summary>
    public partial class CalibrationResultViewModel : ObservableObject
    {
        public CalibrationResult Result { get; }

        public string StatusLabel => GetString($"Label_CalibrationStatus_{Result.Status}");

        public string StopReasonLabel => GetString($"Label_CalibrationStopReason_{Result.StopReason}");

        public bool HasEstimate => Result.T50.HasValue;

        /// <summary>
        /// Lowest slider accuracy, in %, just above the guess rate where the curve has no inverse.
        /// </summary>
        public double MinSliderAccuracyPercent => Math.Floor(Result.Parameters.Gamma * 100) + 1;

        /// <summary>
        /// Highest slider accuracy, in %, just below 1 - δ where the curve has no inverse.
        /// </summary>
        public double MaxSliderAccuracyPercent => Math.Ceiling((1 - Result.Parameters.Delta) * 100) - 1;

        /// <summary>
        /// Label of the presentation time read on the curve, naming the slider accuracy.
        /// </summary>
        public string SliderPresentationTimeLabel => string.Format(DisplayCulture, Strings.Label_CalibrationPresentationTimeAt, SliderAccuracyPercent);

        /// <summary>
        /// Result fields in the order of the calibration worksheet, with the current slider reading.
        /// </summary>
        public IReadOnlyList<CalibrationResultField> ResultFields => new[]
        {
            new CalibrationResultField(Strings.Header_CalibrationStatus, StatusLabel),
            new CalibrationResultField(Strings.Header_CalibrationStopReason, StopReasonLabel),
            new CalibrationResultField(Strings.Header_CalibrationT50, Format(Result.T50, "F0")),
            new CalibrationResultField(Strings.Header_CalibrationT5, Format(Result.T5, "F0")),
            new CalibrationResultField(Strings.Header_CalibrationT95, Format(Result.T95, "F0")),
            new CalibrationResultField(Strings.Header_CalibrationR, Format(Result.R, "F2")),
            new CalibrationResultField(Strings.Header_CalibrationPhase1TrialCount, Format(Result.Phase1TrialCount)),
            new CalibrationResultField(Strings.Header_CalibrationPhase2TrialCount, Format(Result.Phase2TrialCount)),
            new CalibrationResultField(Strings.Header_CalibrationStartingPoint, Format(Result.StartingPoint, "F0")),
            new CalibrationResultField(Strings.Header_CalibrationSliderAccuracy, HasEstimate ? Format(SliderAccuracyPercent, "F0") : string.Empty),
            new CalibrationResultField(Strings.Header_CalibrationSliderPresentationTime, Format(SliderPresentationTime, "F0"))
        };

        /// <summary>
        /// Parameters the calibration ran with, in the order of the calibration worksheet, accuracy in %.
        /// </summary>
        public IReadOnlyList<CalibrationResultField> ParameterFields { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SliderPresentationTimeLabel))]
        [NotifyPropertyChangedFor(nameof(ResultFields))]
        private double _sliderAccuracyPercent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ResultFields))]
        private double? _sliderPresentationTime;

        public CalibrationResultViewModel(CalibrationResult result)
        {
            Result = result;
            _sliderAccuracyPercent = (result.SliderAccuracy ?? result.Parameters.TargetAccuracy) * 100;
            _sliderPresentationTime = result.SliderPresentationTime ?? result.T50;
            ParameterFields = BuildParameterFields(result.Parameters);
        }

        /// <summary>
        /// All fields as "label TAB value" lines, pasted by a spreadsheet into two columns.
        /// </summary>
        public string ToClipboardText() => string.Join(Environment.NewLine, ResultFields.Concat(ParameterFields).Select(field => $"{field.Label}\t{field.Value}"));

        [RelayCommand]
        private void CopyAll() => Clipboard.SetText(ToClipboardText());

        [RelayCommand]
        private void CopyField(CalibrationResultField field) => Clipboard.SetText(field.Value);

        [RelayCommand]
        private void CopyPresentationTime() => Clipboard.SetText(Format(SliderPresentationTime, "F0"));

        partial void OnSliderAccuracyPercentChanged(double oldValue, double newValue)
        {
            if (double.IsNaN(newValue))
            {
                SliderAccuracyPercent = oldValue;
                return;
            }

            if (Result.T50 is not double t50)
                return;

            double accuracy = newValue / 100;
            Result.SliderAccuracy = accuracy;
            Result.SliderPresentationTime = CalibrationCalculator.PresentationTimeAtAccuracy(t50, accuracy, Result.Parameters);
            SliderPresentationTime = Result.SliderPresentationTime;
        }

        private static CultureInfo DisplayCulture => Strings.Culture ?? CultureInfo.CurrentCulture;

        private static IReadOnlyList<CalibrationResultField> BuildParameterFields(CalibrationParameters parameters) => new[]
        {
            new CalibrationResultField(Strings.Header_CalibrationStartingTime, Format(parameters.StartingTime)),
            new CalibrationResultField(Strings.Header_CalibrationLowerBound, Format(parameters.LowerBound)),
            new CalibrationResultField(Strings.Header_CalibrationUpperBound, Format(parameters.UpperBound)),
            new CalibrationResultField(Strings.Header_CalibrationTargetAccuracy, Format(parameters.TargetAccuracy * 100, "G10")),
            new CalibrationResultField(Strings.Header_CalibrationGamma, Format(parameters.Gamma, "G10")),
            new CalibrationResultField(Strings.Header_CalibrationDelta, Format(parameters.Delta, "G10")),
            new CalibrationResultField(Strings.Header_CalibrationBeta, Format(parameters.Beta, "G10")),
            new CalibrationResultField(Strings.Header_CalibrationPriorWidthFactor, Format(parameters.PriorWidthFactor, "G10")),
            new CalibrationResultField(Strings.Header_CalibrationMinCorrectPhase1Trials, Format(parameters.MinCorrectPhase1Trials)),
            new CalibrationResultField(Strings.Header_CalibrationMinPhase2TrialCount, Format(parameters.MinPhase2TrialCount)),
            new CalibrationResultField(Strings.Header_CalibrationMaxPhase2TrialCount, Format(parameters.MaxPhase2TrialCount)),
            new CalibrationResultField(Strings.Header_CalibrationConvergedThreshold, Format(parameters.ConvergedThreshold, "G10")),
            new CalibrationResultField(Strings.Header_CalibrationNearlyConvergedThreshold, Format(parameters.NearlyConvergedThreshold, "G10"))
        };

        private static string Format(double? value, string format) => value?.ToString(format, DisplayCulture) ?? string.Empty;

        private static string Format(int value) => value.ToString(DisplayCulture);

        private static string GetString(string key) => Strings.ResourceManager.GetString(key, Strings.Culture) ?? key;
    }
}
