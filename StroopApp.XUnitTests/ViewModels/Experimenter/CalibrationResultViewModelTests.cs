using StroopApp.Models.Calibration;
using StroopApp.Resources;
using StroopApp.Services.Calibration;
using System.Globalization;
using StroopApp.ViewModels.Experiment.Experimenter.End;
using Xunit;

namespace StroopApp.XUnitTests.ViewModels.Experimenter
{
    /// <summary>
    /// Unit tests for the calibration result card of the end page.
    /// </summary>
    public class CalibrationResultViewModelTests
    {
        private static CalibrationResult Result(double? t50 = 800)
        {
            return new CalibrationResult
            {
                Status = CalibrationStatus.Converged,
                StopReason = CalibrationStopReason.Converged,
                T5 = 700,
                T50 = t50,
                T95 = 900,
                R = 0.25,
                Parameters = new CalibrationParameters()
            };
        }

        /// <summary>
        /// The slider starts at the target accuracy and shows t50, without writing the result.
        /// </summary>
        [Fact]
        public void Constructor_NewResult_StartsAtTargetAccuracy()
        {
            var result = Result();

            var viewModel = new CalibrationResultViewModel(result);

            Assert.Equal(85, viewModel.SliderAccuracyPercent, 6);
            Assert.Equal(800, viewModel.SliderPresentationTime);
            Assert.Null(result.SliderAccuracy);
            Assert.Null(result.SliderPresentationTime);
        }

        /// <summary>
        /// The slider stays strictly between γ and 1 - δ, where the curve can be inverted.
        /// </summary>
        [Fact]
        public void SliderBounds_DefaultParameters_StayInsideTheCurve()
        {
            var viewModel = new CalibrationResultViewModel(Result());

            Assert.Equal(26, viewModel.MinSliderAccuracyPercent);
            Assert.Equal(98, viewModel.MaxSliderAccuracyPercent);
        }

        /// <summary>
        /// Moving the slider writes the accuracy and the time read on the curve into the result.
        /// </summary>
        [Fact]
        public void SliderAccuracyPercent_Moved_StoresAccuracyAndCurveTime()
        {
            var result = Result();
            var viewModel = new CalibrationResultViewModel(result);

            viewModel.SliderAccuracyPercent = 60;

            double expected = CalibrationCalculator.PresentationTimeAtAccuracy(800, 0.6, result.Parameters);
            Assert.Equal(0.6, result.SliderAccuracy!.Value, 6);
            Assert.Equal(expected, result.SliderPresentationTime!.Value, 6);
            Assert.Equal(expected, viewModel.SliderPresentationTime!.Value, 6);
            Assert.True(expected < 800);
        }

        /// <summary>
        /// Moving the slider back to the target accuracy gives t50 again.
        /// </summary>
        [Fact]
        public void SliderAccuracyPercent_BackToTarget_GivesT50()
        {
            var viewModel = new CalibrationResultViewModel(Result());

            viewModel.SliderAccuracyPercent = 60;
            viewModel.SliderAccuracyPercent = 85;

            Assert.Equal(800, viewModel.SliderPresentationTime!.Value, 6);
        }

        /// <summary>
        /// Without t50, moving the slider leaves the result untouched.
        /// </summary>
        [Fact]
        public void SliderAccuracyPercent_NoT50_LeavesResultEmpty()
        {
            var result = Result(t50: null);
            var viewModel = new CalibrationResultViewModel(result);

            viewModel.SliderAccuracyPercent = 60;

            Assert.Null(result.SliderAccuracy);
            Assert.Null(result.SliderPresentationTime);
        }

        /// <summary>
        /// Clearing the accuracy input restores the last valid accuracy and keeps its time.
        /// </summary>
        [Fact]
        public void SliderAccuracyPercent_Cleared_RestoresLastValue()
        {
            var result = Result();
            var viewModel = new CalibrationResultViewModel(result);
            viewModel.SliderAccuracyPercent = 60;
            double expected = result.SliderPresentationTime!.Value;

            viewModel.SliderAccuracyPercent = double.NaN;

            Assert.Equal(60, viewModel.SliderAccuracyPercent, 6);
            Assert.Equal(0.6, result.SliderAccuracy!.Value, 6);
            Assert.Equal(expected, result.SliderPresentationTime!.Value, 6);
            Assert.Equal(expected, viewModel.SliderPresentationTime!.Value, 6);
        }

        /// <summary>
        /// The presentation time label names the accuracy currently read on the curve.
        /// </summary>
        [Fact]
        public void SliderPresentationTimeLabel_SliderMoved_NamesNewAccuracy()
        {
            var viewModel = new CalibrationResultViewModel(Result());

            viewModel.SliderAccuracyPercent = 60;

            Assert.Equal(string.Format(Strings.Label_CalibrationPresentationTimeAt, 60), viewModel.SliderPresentationTimeLabel);
        }

        /// <summary>
        /// The result fields follow the calibration worksheet order and show the current slider reading.
        /// </summary>
        [Fact]
        public void ResultFields_SliderMoved_FollowWorksheetOrder()
        {
            var viewModel = new CalibrationResultViewModel(Result());

            viewModel.SliderAccuracyPercent = 60;

            var expectedLabels = new[]
            {
                Strings.Header_CalibrationStatus, Strings.Header_CalibrationStopReason, Strings.Header_CalibrationT50, Strings.Header_CalibrationT5,
                Strings.Header_CalibrationT95, Strings.Header_CalibrationR, Strings.Header_CalibrationPhase1TrialCount, Strings.Header_CalibrationPhase2TrialCount,
                Strings.Header_CalibrationStartingPoint, Strings.Header_CalibrationSliderAccuracy, Strings.Header_CalibrationSliderPresentationTime
            };
            Assert.Equal(expectedLabels, viewModel.ResultFields.Select(field => field.Label));
            Assert.Equal("800", viewModel.ResultFields[2].Value);
            Assert.Equal("60", viewModel.ResultFields[9].Value);
            Assert.Equal(viewModel.SliderPresentationTime!.Value.ToString("F0", CultureInfo.CurrentCulture), viewModel.ResultFields[10].Value);
        }

        /// <summary>
        /// The parameter fields list every calibration parameter of the worksheet, accuracy in percent.
        /// </summary>
        [Fact]
        public void ParameterFields_DefaultParameters_ListWorksheetParameters()
        {
            var viewModel = new CalibrationResultViewModel(Result());

            Assert.Equal(13, viewModel.ParameterFields.Count);
            Assert.Equal(Strings.Header_CalibrationStartingTime, viewModel.ParameterFields[0].Label);
            Assert.Equal("2000", viewModel.ParameterFields[0].Value);
            Assert.Equal(Strings.Header_CalibrationTargetAccuracy, viewModel.ParameterFields[3].Label);
            Assert.Equal("85", viewModel.ParameterFields[3].Value);
        }

        /// <summary>
        /// The copy of all fields gives one "label TAB value" line per field, ready for two spreadsheet columns.
        /// </summary>
        [Fact]
        public void ToClipboardText_AllFields_OneTabSeparatedLinePerField()
        {
            var viewModel = new CalibrationResultViewModel(Result());

            string[] lines = viewModel.ToClipboardText().Split(Environment.NewLine);

            Assert.Equal(viewModel.ResultFields.Count + viewModel.ParameterFields.Count, lines.Length);
            Assert.Equal($"{Strings.Header_CalibrationStatus}\t{viewModel.StatusLabel}", lines[0]);
            Assert.Equal($"{Strings.Header_CalibrationStartingTime}\t2000", lines[viewModel.ResultFields.Count]);
        }

        /// <summary>
        /// A result without estimate hides the estimate part of the card.
        /// </summary>
        [Fact]
        public void HasEstimate_NoT50_IsFalse()
        {
            var viewModel = new CalibrationResultViewModel(Result(t50: null));

            Assert.False(viewModel.HasEstimate);
        }
    }
}
