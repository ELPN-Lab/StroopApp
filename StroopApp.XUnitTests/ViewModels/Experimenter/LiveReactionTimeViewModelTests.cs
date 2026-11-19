using StroopApp.Models;
using StroopApp.ViewModels.Experiment.Experimenter;
using StroopApp.ViewModels.State;
using Xunit;

namespace StroopApp.XUnitTests.ViewModels.Experimenter
{
    /// <summary>
    /// Unit tests for the "Average times by interval" groups.
    /// </summary>
    public class LiveReactionTimeViewModelTests
    {
        private static (LiveReactionTimeViewModel ViewModel, ExperimentSettingsViewModel Settings) Create(int groupSize)
        {
            var model = new ExperimentSettings
            {
                CurrentProfile = new StroopProfile { GroupSize = groupSize }
            };
            var settings = new ExperimentSettingsViewModel(model);
            return (new LiveReactionTimeViewModel(settings), settings);
        }

        private static void AddPoints(ExperimentSettingsViewModel settings, params (bool? IsValid, double? Time)[] points)
        {
            int next = settings.ExperimentContext.ReactionPoints.Count + 1;
            foreach (var (isValid, time) in points)
                settings.ExperimentContext.ReactionPoints.Add(new ReactionTimePoint(next++, time, isValid));
        }

        /// <summary>
        /// The mean of an interval averages the correct answers only.
        /// </summary>
        [Fact]
        public void GroupAverages_IntervalWithWrongAnswers_AveragesCorrectAnswersOnly()
        {
            var (viewModel, settings) = Create(4);

            AddPoints(settings, (true, 400.0), (false, 1000.0), (true, 600.0), (false, 800.0));

            var group = Assert.Single(viewModel.GroupAverages);
            Assert.Equal(500.0, group.Average);
        }

        /// <summary>
        /// Each interval has its own correct-answers mean, and the correct count still covers the whole interval.
        /// </summary>
        [Fact]
        public void GroupAverages_TwoIntervals_EachHasItsOwnCorrectMean()
        {
            var (viewModel, settings) = Create(2);

            AddPoints(settings, (true, 300.0), (false, 900.0), (true, 200.0), (true, 400.0));

            Assert.Equal(2, viewModel.GroupAverages.Count);
            Assert.Equal(300.0, viewModel.GroupAverages[0].Average);
            Assert.Equal(1, viewModel.GroupAverages[0].CorrectCount);
            Assert.Equal(300.0, viewModel.GroupAverages[1].Average);
            Assert.Equal(2, viewModel.GroupAverages[1].CorrectCount);
            Assert.Equal(2, viewModel.GroupAverages[1].TotalCount);
        }

        /// <summary>
        /// Intervals already present when the view model is created are also averaged over correct answers only.
        /// </summary>
        [Fact]
        public void Constructor_ExistingPoints_AveragesCorrectAnswersOnly()
        {
            var model = new ExperimentSettings
            {
                CurrentProfile = new StroopProfile { GroupSize = 3 }
            };
            var settings = new ExperimentSettingsViewModel(model);
            AddPoints(settings, (true, 200.0), (false, 1000.0), (true, 400.0));

            var viewModel = new LiveReactionTimeViewModel(settings);

            Assert.Equal(300.0, Assert.Single(viewModel.GroupAverages).Average);
        }

        /// <summary>
        /// An interval without any correct answer shows "NA".
        /// </summary>
        [Fact]
        public void GroupAverages_IntervalWithoutCorrectAnswer_ShowsNA()
        {
            var (viewModel, settings) = Create(3);

            AddPoints(settings, (false, 500.0), (false, 600.0), (null, 700.0));

            var group = Assert.Single(viewModel.GroupAverages);
            Assert.Null(group.Average);
            Assert.Equal("NA", group.AverageDisplay);
        }
    }
}
