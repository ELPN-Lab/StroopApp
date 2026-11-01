using StroopApp.Models.Calibration;
using StroopApp.Services.Calibration;
using Xunit;

namespace StroopApp.XUnitTests.Services.Calibration
{
    /// <summary>
    /// Unit tests for the CalibrationCalculator math functions.
    /// </summary>
    public class CalibrationCalculatorTests
    {
        private const double Tolerance = 1e-12;

        /// <summary>
        /// At the threshold, the probability of a correct answer equals the target accuracy.
        /// </summary>
        [Theory]
        [InlineData(140.0, 0.85, 2.0)]
        [InlineData(800.0, 0.85, 2.0)]
        [InlineData(5000.0, 0.85, 2.0)]
        [InlineData(800.0, 0.80, 1.85)]
        public void Probability_AtThreshold_ReturnsTargetAccuracy(double threshold, double targetAccuracy, double beta)
        {
            var parameters = new CalibrationParameters { TargetAccuracy = targetAccuracy, Beta = beta };

            var probability = CalibrationCalculator.Probability(threshold, threshold, parameters);

            Assert.Equal(targetAccuracy, probability, Tolerance);
        }

        /// <summary>
        /// A zero presentation time gives exactly the guess rate.
        /// </summary>
        [Fact]
        public void Probability_ZeroTime_ReturnsGamma()
        {
            var parameters = new CalibrationParameters();

            var probability = CalibrationCalculator.Probability(0, 800, parameters);

            Assert.Equal(parameters.Gamma, probability, Tolerance);
        }

        /// <summary>
        /// A very long presentation time tends to 1 minus the lapse rate.
        /// </summary>
        [Fact]
        public void Probability_VeryLongTime_ReturnsOneMinusDelta()
        {
            var parameters = new CalibrationParameters();

            var probability = CalibrationCalculator.Probability(1e6, 800, parameters);

            Assert.Equal(1 - parameters.Delta, probability, Tolerance);
        }

        /// <summary>
        /// The probability increases with the presentation time.
        /// </summary>
        [Theory]
        [InlineData(140.0, 300.0)]
        [InlineData(300.0, 800.0)]
        [InlineData(800.0, 801.0)]
        [InlineData(2000.0, 5000.0)]
        public void Probability_LongerTime_IsHigher(double shorterTime, double longerTime)
        {
            var parameters = new CalibrationParameters();

            var shorter = CalibrationCalculator.Probability(shorterTime, 800, parameters);
            var longer = CalibrationCalculator.Probability(longerTime, 800, parameters);

            Assert.True(longer > shorter);
        }

        /// <summary>
        /// With default parameters, the threshold at 85 % is about 1.29035 times the Weibull scale.
        /// </summary>
        [Fact]
        public void ThresholdFactor_DefaultTarget_MatchesSpecification()
        {
            var parameters = new CalibrationParameters();

            var factor = CalibrationCalculator.ThresholdFactor(parameters.TargetAccuracy, parameters);

            Assert.Equal(1.29035, factor, 1e-5);
        }

        /// <summary>
        /// A higher accuracy requires a longer time, so a larger factor.
        /// </summary>
        [Theory]
        [InlineData(0.50, 0.85)]
        [InlineData(0.85, 0.90)]
        [InlineData(0.90, 0.98)]
        public void ThresholdFactor_HigherAccuracy_IsLarger(double lowerAccuracy, double higherAccuracy)
        {
            var parameters = new CalibrationParameters();

            var lower = CalibrationCalculator.ThresholdFactor(lowerAccuracy, parameters);
            var higher = CalibrationCalculator.ThresholdFactor(higherAccuracy, parameters);

            Assert.True(higher > lower);
        }

        /// <summary>
        /// With enough correct answers, the starting point is the mean plus the sample standard deviation.
        /// </summary>
        [Fact]
        public void ComputeStartingPoint_EnoughCorrectAnswers_ReturnsMeanPlusSampleStandardDeviation()
        {
            var parameters = new CalibrationParameters();
            var reactionTimes = new List<double> { 400, 500, 600, 700, 800 };

            var startingPoint = CalibrationCalculator.ComputeStartingPoint(reactionTimes, parameters);

            Assert.Equal(600 + Math.Sqrt(25000), startingPoint, Tolerance);
        }

        /// <summary>
        /// Identical reaction times give a zero standard deviation, so the starting point is their value.
        /// </summary>
        [Fact]
        public void ComputeStartingPoint_IdenticalReactionTimes_ReturnsMean()
        {
            var parameters = new CalibrationParameters();
            var reactionTimes = new List<double> { 900, 900, 900, 900, 900 };

            var startingPoint = CalibrationCalculator.ComputeStartingPoint(reactionTimes, parameters);

            Assert.Equal(900, startingPoint, Tolerance);
        }

        /// <summary>
        /// Below the minimum number of correct answers, the starting point is the starting time.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(4)]
        public void ComputeStartingPoint_TooFewCorrectAnswers_ReturnsStartingTime(int count)
        {
            var parameters = new CalibrationParameters { StartingTime = 3000 };
            var reactionTimes = Enumerable.Repeat(500.0, count).ToList();

            var startingPoint = CalibrationCalculator.ComputeStartingPoint(reactionTimes, parameters);

            Assert.Equal(3000, startingPoint);
        }

        /// <summary>
        /// The minimum number of correct answers comes from the parameters.
        /// </summary>
        [Fact]
        public void ComputeStartingPoint_CustomMinimum_UsesParameter()
        {
            var parameters = new CalibrationParameters { MinCorrectPhase1Trials = 3 };
            var reactionTimes = new List<double> { 500, 600, 700 };

            var startingPoint = CalibrationCalculator.ComputeStartingPoint(reactionTimes, parameters);

            Assert.Equal(700, startingPoint, Tolerance);
        }

        /// <summary>
        /// A starting point below the lower bound is raised to the lower bound.
        /// </summary>
        [Fact]
        public void ComputeStartingPoint_BelowLowerBound_ReturnsLowerBound()
        {
            var parameters = new CalibrationParameters();
            var reactionTimes = new List<double> { 100, 100, 100, 100, 100 };

            var startingPoint = CalibrationCalculator.ComputeStartingPoint(reactionTimes, parameters);

            Assert.Equal(parameters.LowerBound, startingPoint);
        }

        /// <summary>
        /// A starting point above the upper bound is lowered to the upper bound.
        /// </summary>
        [Fact]
        public void ComputeStartingPoint_AboveUpperBound_ReturnsUpperBound()
        {
            var parameters = new CalibrationParameters();
            var reactionTimes = new List<double> { 4000, 4500, 5000, 5500, 6000 };

            var startingPoint = CalibrationCalculator.ComputeStartingPoint(reactionTimes, parameters);

            Assert.Equal(parameters.UpperBound, startingPoint);
        }
    }
}
