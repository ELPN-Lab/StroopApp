using Xunit;

namespace StroopApp.XUnitTests.Models
{
    /// <summary>
    /// Unit tests for the shared "mean of correct answers" rule.
    /// </summary>
    public class ResponseTimeStatisticsTests
    {
        private static double? Mean(params (bool? IsValidResponse, double? ReactionTime)[] trials)
            => StroopApp.Models.ResponseTimeStatistics.MeanOfCorrectAnswers(trials);

        /// <summary>
        /// Only the correct trials count: wrong answers do not move the mean.
        /// </summary>
        [Fact]
        public void MeanOfCorrectAnswers_MixedAnswers_AveragesCorrectTrialsOnly()
        {
            var mean = Mean((true, 100.0), (false, 900.0), (true, 300.0), (false, 700.0));

            Assert.Equal(200.0, mean);
        }

        /// <summary>
        /// A trial with no recorded validity (null) is not a correct answer.
        /// </summary>
        [Fact]
        public void MeanOfCorrectAnswers_NullValidity_IsIgnored()
        {
            var mean = Mean((true, 100.0), (null, 900.0));

            Assert.Equal(100.0, mean);
        }

        /// <summary>
        /// A correct trial without a time, or with a NaN time, is skipped instead of poisoning the mean.
        /// </summary>
        [Fact]
        public void MeanOfCorrectAnswers_CorrectTrialsWithoutValidTime_AreSkipped()
        {
            var mean = Mean((true, 100.0), (true, null), (true, double.NaN), (true, 200.0));

            Assert.Equal(150.0, mean);
        }

        /// <summary>
        /// With no correct trial at all, the mean is null and not zero.
        /// </summary>
        [Fact]
        public void MeanOfCorrectAnswers_OnlyWrongAnswers_ReturnsNull()
        {
            Assert.Null(Mean((false, 400.0), (false, 500.0), (null, 600.0)));
        }

        /// <summary>
        /// No trial gives a null mean.
        /// </summary>
        [Fact]
        public void MeanOfCorrectAnswers_NoTrial_ReturnsNull()
        {
            Assert.Null(Mean());
        }

        /// <summary>
        /// Correct trials whose time is missing or NaN leave nothing to average: the mean is null.
        /// </summary>
        [Fact]
        public void MeanOfCorrectAnswers_CorrectTrialsAllWithoutValidTime_ReturnsNull()
        {
            Assert.Null(Mean((true, null), (true, double.NaN)));
        }
    }
}
