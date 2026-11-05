using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.ViewModels.Configuration.Profile;
using StroopApp.XUnitTests.TestDummies;
using System.Collections.ObjectModel;
using Xunit;

namespace StroopApp.XUnitTests.ViewModels.Configuration
{
    /// <summary>
    /// Unit tests for the calibration part of the Stroop profile editor.
    /// </summary>
    public class StroopProfileEditorViewModelTests
    {
        private sealed class TestableStroopProfileEditorViewModel : StroopProfileEditorViewModel
        {
            public TestableStroopProfileEditorViewModel(ExperimentProfile profile)
                : base(profile, new ObservableCollection<ExperimentProfile>(), new DummyProfileService()) { }

            public List<string> Errors { get; } = new();

            protected override Task ShowErrorDialogAsync(string message)
            {
                Errors.Add(message);
                return Task.CompletedTask;
            }
        }

        private static StroopProfile CalibrationProfile()
        {
            return new StroopProfile { ProfileName = "calib", IsCalibration = true, CalculationMode = CalculationMode.TaskDuration };
        }

        /// <summary>
        /// A calibration profile forces the word count mode and the maximum number of calibration trials.
        /// </summary>
        [Fact]
        public void Constructor_CalibrationProfile_SyncsTrialCount()
        {
            var profile = CalibrationProfile();

            var viewModel = new TestableStroopProfileEditorViewModel(profile);

            Assert.Equal(CalculationMode.WordCount, viewModel.CalculationMode);
            Assert.Equal(120, viewModel.StimulusCount);
            Assert.Equal(120, profile.WordCount);
        }

        /// <summary>
        /// Changing a phase trial count updates the word count of the profile.
        /// </summary>
        [Fact]
        public void TrialCounts_Changed_UpdateWordCount()
        {
            var profile = CalibrationProfile();
            var viewModel = new TestableStroopProfileEditorViewModel(profile);

            viewModel.Phase1TrialCount = 10;
            viewModel.MaxPhase2TrialCount = 50;

            Assert.Equal(60, viewModel.StimulusCount);
            Assert.Equal(60, profile.WordCount);
            Assert.Equal(10, profile.CalibrationParameters.Phase1TrialCount);
            Assert.Equal(50, profile.CalibrationParameters.MaxPhase2TrialCount);
        }

        /// <summary>
        /// Switching calibration on applies the trial count of the calibration parameters.
        /// </summary>
        [Fact]
        public void IsCalibration_SwitchedOn_SyncsTrialCount()
        {
            var profile = new StroopProfile { ProfileName = "standard", WordCount = 30, CalculationMode = CalculationMode.WordCount };
            var viewModel = new TestableStroopProfileEditorViewModel(profile);

            viewModel.IsCalibration = true;

            Assert.True(profile.IsCalibration);
            Assert.Equal(120, profile.WordCount);
        }

        /// <summary>
        /// The target accuracy is edited as a percentage and stored as a fraction.
        /// </summary>
        [Fact]
        public void TargetAccuracyPercent_Set_StoresFraction()
        {
            var profile = CalibrationProfile();
            var viewModel = new TestableStroopProfileEditorViewModel(profile);

            viewModel.TargetAccuracyPercent = 75;

            Assert.Equal(0.75, profile.CalibrationParameters.TargetAccuracy, 6);
        }

        /// <summary>
        /// The group size and max reaction time checks do not block a calibration profile.
        /// </summary>
        [Fact]
        public async Task SaveAsync_Calibration_SkipsFixedDurationChecks()
        {
            var profile = CalibrationProfile();
            profile.GroupSize = 7;
            profile.MaxReactionTime = 0;
            var viewModel = new TestableStroopProfileEditorViewModel(profile);

            await viewModel.SaveAsync();

            Assert.Empty(viewModel.Errors);
            Assert.True(viewModel.DialogResult);
        }

        /// <summary>
        /// Outside calibration, a group size that does not divide the trial count still blocks the save.
        /// </summary>
        [Fact]
        public async Task SaveAsync_Standard_KeepsGroupSizeCheck()
        {
            var profile = new StroopProfile { ProfileName = "standard", WordCount = 120, GroupSize = 7, CalculationMode = CalculationMode.WordCount };
            var viewModel = new TestableStroopProfileEditorViewModel(profile);

            await viewModel.SaveAsync();

            Assert.Equal(new[] { Strings.Error_GroupSizeInvalid }, viewModel.Errors);
            Assert.NotEqual(true, viewModel.DialogResult);
        }

        /// <summary>
        /// Invalid calibration parameters block the save with one localized line per broken rule.
        /// </summary>
        [Fact]
        public async Task SaveAsync_InvalidCalibrationParameters_ShowsEachError()
        {
            var profile = CalibrationProfile();
            profile.CalibrationParameters.LowerBound = 6000;
            profile.CalibrationParameters.Beta = 0;
            var viewModel = new TestableStroopProfileEditorViewModel(profile);

            await viewModel.SaveAsync();

            string expected = string.Join(Environment.NewLine, Strings.Error_Calibration_LowerBoundNotBelowUpperBound, Strings.Error_Calibration_StartingTimeOutOfBounds, Strings.Error_Calibration_BetaNotPositive);
            Assert.Equal(new[] { expected }, viewModel.Errors);
            Assert.NotEqual(true, viewModel.DialogResult);
        }
    }
}
