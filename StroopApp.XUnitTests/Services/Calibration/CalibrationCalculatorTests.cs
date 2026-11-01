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

        private static double MeanLogThreshold(double[] logWeights, double[] grid)
        {
            return logWeights.Select((logWeight, i) => Math.Exp(logWeight) * Math.Log10(grid[i])).Sum();
        }

        private static double TotalWeight(double[] logWeights)
        {
            return logWeights.Sum(Math.Exp);
        }

        private static int SelectNextPresentationTime(double[] logWeights, double[] grid, CalibrationParameters parameters)
        {
            var candidates = CalibrationCalculator.CreateCandidates(grid);
            var probabilityTable = CalibrationCalculator.CreateProbabilityTable(grid, candidates, parameters);
            var entropyTable = CalibrationCalculator.CreateEntropyTable(probabilityTable);
            return CalibrationCalculator.SelectNextPresentationTime(logWeights, candidates, probabilityTable, entropyTable);
        }

        /// <summary>
        /// Candidates are the grid rounded to the ms, distinct and ascending, from 140 ms to 5000 ms.
        /// </summary>
        [Fact]
        public void CreateCandidates_DefaultGrid_RoundsDistinctAscending()
        {
            var grid = CalibrationCalculator.CreateGrid(new CalibrationParameters());

            var candidates = CalibrationCalculator.CreateCandidates(grid);

            Assert.Equal(140, candidates[0]);
            Assert.Equal(5000, candidates[^1]);
            Assert.Equal(candidates.Distinct().Count(), candidates.Length);
            Assert.Equal(candidates.OrderBy(time => time), candidates);
            Assert.All(candidates, time => Assert.Contains(time, grid.Select(candidate => (int)Math.Round(candidate))));
        }

        /// <summary>
        /// Each table entry equals the probability of a correct answer for that candidate time and threshold.
        /// </summary>
        [Fact]
        public void CreateProbabilityTable_Default_MatchesProbability()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var candidates = CalibrationCalculator.CreateCandidates(grid);

            var table = CalibrationCalculator.CreateProbabilityTable(grid, candidates, parameters);

            Assert.Equal(candidates.Length, table.GetLength(0));
            Assert.Equal(grid.Length, table.GetLength(1));
            foreach (var (candidate, i) in new[] { (0, 0), (50, 200), (200, 50), (candidates.Length - 1, grid.Length - 1) })
                Assert.Equal(CalibrationCalculator.Probability(candidates[candidate], grid[i], parameters), table[candidate, i], Tolerance);
        }

        /// <summary>
        /// Each entropy table entry equals -p ln p - (1 - p) ln(1 - p) for the matching probability.
        /// </summary>
        [Fact]
        public void CreateEntropyTable_Default_MatchesBinaryEntropy()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var candidates = CalibrationCalculator.CreateCandidates(grid);
            var probabilityTable = CalibrationCalculator.CreateProbabilityTable(grid, candidates, parameters);

            var entropyTable = CalibrationCalculator.CreateEntropyTable(probabilityTable);

            Assert.Equal(probabilityTable.GetLength(0), entropyTable.GetLength(0));
            Assert.Equal(probabilityTable.GetLength(1), entropyTable.GetLength(1));
            foreach (var (candidate, i) in new[] { (0, 0), (50, 200), (200, 50), (candidates.Length - 1, grid.Length - 1) })
            {
                double p = probabilityTable[candidate, i];
                Assert.Equal(-p * Math.Log(p) - (1 - p) * Math.Log(1 - p), entropyTable[candidate, i], Tolerance);
            }
        }

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

        /// <summary>
        /// With default bounds, the grid holds 312 points from 140 ms to 5000 ms.
        /// </summary>
        [Fact]
        public void CreateGrid_DefaultBounds_Spans140To5000With312Points()
        {
            var parameters = new CalibrationParameters();

            var grid = CalibrationCalculator.CreateGrid(parameters);

            Assert.Equal(312, grid.Length);
            Assert.Equal(140, grid[0]);
            Assert.Equal(5000, grid[^1]);
        }

        /// <summary>
        /// Grid points are evenly spaced in log10, with a step no larger than 0.005.
        /// </summary>
        [Fact]
        public void CreateGrid_DefaultBounds_IsEvenlySpacedInLog()
        {
            var parameters = new CalibrationParameters();

            var grid = CalibrationCalculator.CreateGrid(parameters);
            var steps = grid.Skip(1).Select((time, i) => Math.Log10(time) - Math.Log10(grid[i])).ToList();

            Assert.All(steps, step => Assert.Equal(steps[0], step, 1e-9));
            Assert.True(steps[0] <= 0.005);
        }

        /// <summary>
        /// The prior weights sum to 1.
        /// </summary>
        [Fact]
        public void CreatePrior_Default_IsNormalized()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);

            var logWeights = CalibrationCalculator.CreatePrior(grid, 800, parameters);

            Assert.Equal(1, TotalWeight(logWeights), 1e-9);
        }

        /// <summary>
        /// The prior peaks at the grid point closest to the starting point.
        /// </summary>
        [Theory]
        [InlineData(300.0)]
        [InlineData(800.0)]
        [InlineData(2500.0)]
        public void CreatePrior_PeaksAtClosestGridPoint(double startingPoint)
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var closestIndex = Array.IndexOf(grid, grid.MinBy(time => Math.Abs(Math.Log10(time) - Math.Log10(startingPoint))));

            var logWeights = CalibrationCalculator.CreatePrior(grid, startingPoint, parameters);

            Assert.Equal(closestIndex, Array.IndexOf(logWeights, logWeights.Max()));
        }

        /// <summary>
        /// Centered on a grid point, the prior is symmetric in log10 around it.
        /// </summary>
        [Fact]
        public void CreatePrior_CenteredOnGridPoint_IsSymmetric()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            const int center = 150;

            var logWeights = CalibrationCalculator.CreatePrior(grid, grid[center], parameters);

            for (int offset = 1; offset <= 50; offset++)
                Assert.Equal(logWeights[center - offset], logWeights[center + offset], 1e-9);
        }

        /// <summary>
        /// A correct answer moves the estimate down, an error moves it up.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Update_Answer_MovesEstimateInExpectedDirection(bool isCorrect)
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var prior = CalibrationCalculator.CreatePrior(grid, 800, parameters);

            var posterior = CalibrationCalculator.Update(prior, grid, 800, isCorrect, parameters);

            if (isCorrect)
                Assert.True(MeanLogThreshold(posterior, grid) < MeanLogThreshold(prior, grid));
            else
                Assert.True(MeanLogThreshold(posterior, grid) > MeanLogThreshold(prior, grid));
        }

        /// <summary>
        /// The updated weights sum to 1 and the input array is left unchanged.
        /// </summary>
        [Fact]
        public void Update_KeepsNormalizationAndInputUnchanged()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var prior = CalibrationCalculator.CreatePrior(grid, 800, parameters);
            var priorCopy = (double[])prior.Clone();

            var posterior = CalibrationCalculator.Update(prior, grid, 600, true, parameters);

            Assert.Equal(1, TotalWeight(posterior), 1e-9);
            Assert.Equal(priorCopy, prior);
        }

        /// <summary>
        /// A long run of identical extreme answers produces neither NaN nor infinity.
        /// </summary>
        [Theory]
        [InlineData(5000, false)]
        [InlineData(140, false)]
        [InlineData(5000, true)]
        [InlineData(140, true)]
        public void Update_HundredIdenticalAnswers_StaysFinite(int presentationTime, bool isCorrect)
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = CalibrationCalculator.CreatePrior(grid, 800, parameters);

            for (int trial = 0; trial < 100; trial++)
                logWeights = CalibrationCalculator.Update(logWeights, grid, presentationTime, isCorrect, parameters);

            Assert.All(logWeights, logWeight => Assert.False(double.IsNaN(logWeight) || double.IsPositiveInfinity(logWeight)));
            Assert.Equal(1, TotalWeight(logWeights), 1e-9);
        }

        /// <summary>
        /// When all the weight is on one candidate, the median is that candidate.
        /// </summary>
        [Fact]
        public void Quantile_AllWeightOnOnePoint_MedianIsThatPoint()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = Enumerable.Repeat(double.NegativeInfinity, grid.Length).ToArray();
            logWeights[100] = 0;

            var median = CalibrationCalculator.Quantile(logWeights, grid, 0.5);

            Assert.Equal(grid[100], median, 1e-9);
        }

        /// <summary>
        /// With uniform weights, quantiles follow the grid linearly in log10.
        /// </summary>
        [Theory]
        [InlineData(0.05)]
        [InlineData(0.5)]
        [InlineData(0.95)]
        public void Quantile_UniformWeights_IsLinearInLog(double probability)
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = Enumerable.Repeat(-Math.Log(grid.Length), grid.Length).ToArray();
            double position = (probability * grid.Length - 0.5) / (grid.Length - 1);
            double expected = Math.Pow(10, Math.Log10(140) + position * (Math.Log10(5000) - Math.Log10(140)));

            var quantile = CalibrationCalculator.Quantile(logWeights, grid, probability);

            Assert.Equal(expected, quantile, 1e-6);
        }

        /// <summary>
        /// On a prior narrow enough not to be cut by the bounds, the median is the center and the 5 % and 95 % quantiles sit 1.645 standard deviations away in log10.
        /// </summary>
        [Fact]
        public void Quantile_NarrowPrior_MatchesGaussianQuantiles()
        {
            var parameters = new CalibrationParameters { PriorWidthFactor = 1.2 };
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = CalibrationCalculator.CreatePrior(grid, grid[150], parameters);
            double sigma = Math.Log10(parameters.PriorWidthFactor);

            var t5 = CalibrationCalculator.Quantile(logWeights, grid, 0.05);
            var t50 = CalibrationCalculator.Quantile(logWeights, grid, 0.5);
            var t95 = CalibrationCalculator.Quantile(logWeights, grid, 0.95);

            Assert.Equal(grid[150], t50, 1e-6);
            Assert.Equal(1.645 * sigma, Math.Log10(t95) - Math.Log10(t50), 1e-2);
            Assert.Equal(1.645 * sigma, Math.Log10(t50) - Math.Log10(t5), 1e-2);
        }

        /// <summary>
        /// Thomas's examples: R values and their status.
        /// </summary>
        [Theory]
        [InlineData(228.0, 250.0, 273.0, 0.18, CalibrationStatus.Converged)]
        [InlineData(205.0, 250.0, 300.0, 0.38, CalibrationStatus.NearlyConverged)]
        [InlineData(170.0, 250.0, 340.0, 0.68, CalibrationStatus.NotConverged)]
        public void ComputeR_ThomasExamples_MatchExpectedValueAndStatus(double t5, double t50, double t95, double expectedR, CalibrationStatus expectedStatus)
        {
            var parameters = new CalibrationParameters();

            var r = CalibrationCalculator.ComputeR(t5, t50, t95);

            Assert.Equal(expectedR, r, Tolerance);
            Assert.Equal(expectedStatus, CalibrationCalculator.GetStatus(r, parameters));
        }

        /// <summary>
        /// Status limits are inclusive: 0.20 is converged and 0.40 is nearly converged.
        /// </summary>
        [Theory]
        [InlineData(0.0, CalibrationStatus.Converged)]
        [InlineData(0.20, CalibrationStatus.Converged)]
        [InlineData(0.2000001, CalibrationStatus.NearlyConverged)]
        [InlineData(0.40, CalibrationStatus.NearlyConverged)]
        [InlineData(0.4000001, CalibrationStatus.NotConverged)]
        public void GetStatus_Limits_AreInclusive(double r, CalibrationStatus expectedStatus)
        {
            var parameters = new CalibrationParameters();

            Assert.Equal(expectedStatus, CalibrationCalculator.GetStatus(r, parameters));
        }

        /// <summary>
        /// Phase 2 stops from 40 trials with a converged R, or at 100 trials whatever R.
        /// </summary>
        [Theory]
        [InlineData(39, 0.10, false)]
        [InlineData(40, 0.10, true)]
        [InlineData(40, 0.20, true)]
        [InlineData(40, 0.21, false)]
        [InlineData(99, 0.90, false)]
        [InlineData(100, 0.90, true)]
        public void ShouldStop_TrialCountAndR_FollowsStopRule(int phase2TrialCount, double r, bool expected)
        {
            var parameters = new CalibrationParameters();

            Assert.Equal(expected, CalibrationCalculator.ShouldStop(phase2TrialCount, r, parameters));
        }

        /// <summary>
        /// At the target accuracy, the time read on the curve is the estimated threshold.
        /// </summary>
        [Fact]
        public void PresentationTimeAtAccuracy_TargetAccuracy_ReturnsThreshold()
        {
            var parameters = new CalibrationParameters();

            var time = CalibrationCalculator.PresentationTimeAtAccuracy(1075, parameters.TargetAccuracy, parameters);

            Assert.Equal(1075, time, 1e-9);
        }

        /// <summary>
        /// The time read for an accuracy gives back that accuracy on the curve.
        /// </summary>
        [Theory]
        [InlineData(0.50)]
        [InlineData(0.70)]
        [InlineData(0.95)]
        public void PresentationTimeAtAccuracy_RoundTripsThroughProbability(double accuracy)
        {
            var parameters = new CalibrationParameters();

            var time = CalibrationCalculator.PresentationTimeAtAccuracy(1075, accuracy, parameters);

            Assert.Equal(accuracy, CalibrationCalculator.Probability(time, 1075, parameters), 1e-9);
        }

        /// <summary>
        /// The same weights always give the same presentation time.
        /// </summary>
        [Fact]
        public void SelectNextPresentationTime_SameInput_ReturnsSameTime()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = CalibrationCalculator.CreatePrior(grid, 800, parameters);

            var first = SelectNextPresentationTime(logWeights, grid, parameters);
            var second = SelectNextPresentationTime(logWeights, grid, parameters);

            Assert.Equal(first, second);
        }

        /// <summary>
        /// The chosen time is one of the grid candidates rounded to the ms, so within the bounds.
        /// </summary>
        [Theory]
        [InlineData(140.0)]
        [InlineData(800.0)]
        [InlineData(5000.0)]
        public void SelectNextPresentationTime_AnyPrior_ReturnsRoundedGridCandidate(double startingPoint)
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = CalibrationCalculator.CreatePrior(grid, startingPoint, parameters);

            var time = SelectNextPresentationTime(logWeights, grid, parameters);

            Assert.Contains(time, grid.Select(candidate => (int)Math.Round(candidate)));
            Assert.InRange(time, parameters.LowerBound, parameters.UpperBound);
        }

        /// <summary>
        /// When all the weight is on one candidate, every time gives the same expected entropy, so the shortest wins.
        /// </summary>
        [Fact]
        public void SelectNextPresentationTime_AllCandidatesTied_ReturnsShortestTime()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = Enumerable.Repeat(double.NegativeInfinity, grid.Length).ToArray();
            logWeights[100] = 0;

            var time = SelectNextPresentationTime(logWeights, grid, parameters);

            Assert.Equal(parameters.LowerBound, time);
        }

        /// <summary>
        /// On the default prior centered on 800 ms, the chosen time lies within the prior's 90 % interval.
        /// </summary>
        [Fact]
        public void SelectNextPresentationTime_DefaultPrior_IsWithinNinetyPercentInterval()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var logWeights = CalibrationCalculator.CreatePrior(grid, 800, parameters);

            var time = SelectNextPresentationTime(logWeights, grid, parameters);

            Assert.InRange(time, CalibrationCalculator.Quantile(logWeights, grid, 0.05), CalibrationCalculator.Quantile(logWeights, grid, 0.95));
        }

        /// <summary>
        /// A lower estimate leads to a shorter chosen time.
        /// </summary>
        [Fact]
        public void SelectNextPresentationTime_LowerEstimate_ReturnsShorterTime()
        {
            var parameters = new CalibrationParameters();
            var grid = CalibrationCalculator.CreateGrid(parameters);

            var fast = SelectNextPresentationTime(CalibrationCalculator.CreatePrior(grid, 400, parameters), grid, parameters);
            var slow = SelectNextPresentationTime(CalibrationCalculator.CreatePrior(grid, 2000, parameters), grid, parameters);

            Assert.True(fast < slow);
        }
    }
}
