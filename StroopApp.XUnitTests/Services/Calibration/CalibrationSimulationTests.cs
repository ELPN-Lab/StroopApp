using StroopApp.Models.Calibration;
using StroopApp.Services.Calibration;
using Xunit;
using Xunit.Abstractions;

namespace StroopApp.XUnitTests.Services.Calibration
{
    /// <summary>
    /// Runs full phase 2 calibrations against simulated participants whose true threshold is known.
    /// </summary>
    public class CalibrationSimulationTests
    {
        private const int RunsPerCase = 50;

        private readonly ITestOutputHelper _output;

        public CalibrationSimulationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private sealed record SimulationResult(double T5, double T50, double T95, double R, int Phase2TrialCount, bool Converged);

        private static SimulationResult Simulate(double trueThreshold, double trueBeta, double startingPoint, int seed)
        {
            var parameters = new CalibrationParameters();
            var participant = new CalibrationParameters { Beta = trueBeta };
            var random = new Random(seed);
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var candidates = CalibrationCalculator.CreateCandidates(grid);
            var probabilityTable = CalibrationCalculator.CreateProbabilityTable(grid, candidates, parameters);
            var logWeights = CalibrationCalculator.CreatePrior(grid, Math.Clamp(startingPoint, parameters.LowerBound, parameters.UpperBound), parameters);

            int trialCount = 0;
            double t5, t50, t95, r;
            do
            {
                int time = CalibrationCalculator.SelectNextPresentationTime(logWeights, candidates, probabilityTable);
                bool isCorrect = random.NextDouble() < CalibrationCalculator.Probability(time, trueThreshold, participant);
                logWeights = CalibrationCalculator.Update(logWeights, grid, time, isCorrect, parameters);
                trialCount++;

                t5 = CalibrationCalculator.Quantile(logWeights, grid, 0.05);
                t50 = CalibrationCalculator.Quantile(logWeights, grid, 0.5);
                t95 = CalibrationCalculator.Quantile(logWeights, grid, 0.95);
                r = CalibrationCalculator.ComputeR(t5, t50, t95);
            }
            while (!CalibrationCalculator.ShouldStop(trialCount, r, parameters));

            return new SimulationResult(t5, t50, t95, r, trialCount, r <= parameters.ConvergedThreshold);
        }

        private static SimulationResult[] SimulateMany(double trueThreshold, double trueBeta, double startingPoint, int runs, int baseSeed)
        {
            var results = new SimulationResult[runs];
            Parallel.For(0, runs, i => results[i] = Simulate(trueThreshold, trueBeta, startingPoint, baseSeed + i));
            return results;
        }

        private static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(value => value).ToArray();
            return (sorted[(sorted.Length - 1) / 2] + sorted[sorted.Length / 2]) / 2;
        }

        private void Report(string label, double trueThreshold, SimulationResult[] results)
        {
            double medianError = Median(results.Select(result => Math.Abs(result.T50 - trueThreshold) / trueThreshold));
            double coverage = results.Count(result => result.T5 <= trueThreshold && trueThreshold <= result.T95) / (double)results.Length;
            _output.WriteLine($"{label} | median error {medianError:P1} | coverage {coverage:P0} | mean trials {results.Average(result => result.Phase2TrialCount):F1} | converged {results.Count(result => result.Converged)}/{results.Length} | median R {Median(results.Select(result => result.R)):F2}");
        }

        /// <summary>
        /// For several true thresholds, the median estimate error stays below 10 %.
        /// </summary>
        [Theory]
        [InlineData(300.0)]
        [InlineData(600.0)]
        [InlineData(1200.0)]
        [InlineData(3000.0)]
        public void Simulate_KnownThreshold_MedianErrorBelowTenPercent(double trueThreshold)
        {
            var results = SimulateMany(trueThreshold, 2.0, trueThreshold * 1.5, RunsPerCase, 1000);
            Report($"threshold {trueThreshold} ms", trueThreshold, results);

            double medianError = Median(results.Select(result => Math.Abs(result.T50 - trueThreshold) / trueThreshold));

            Assert.True(medianError < 0.10, $"Median error {medianError:P1}");
        }

        /// <summary>
        /// Across thresholds, the true threshold lies within the 90 % interval in 80 % to 97 % of the runs.
        /// </summary>
        [Fact]
        public void Simulate_SeveralThresholds_NinetyPercentIntervalCoverage()
        {
            var thresholds = new[] { 300.0, 600.0, 1200.0, 3000.0 };
            var covered = thresholds
                .SelectMany(threshold => SimulateMany(threshold, 2.0, threshold * 1.5, RunsPerCase, 2000)
                    .Select(result => result.T5 <= threshold && threshold <= result.T95))
                .ToList();

            double coverage = covered.Count(isCovered => isCovered) / (double)covered.Count;
            _output.WriteLine($"coverage {coverage:P1} over {covered.Count} runs");

            Assert.InRange(coverage, 0.80, 0.97);
        }

        /// <summary>
        /// A starting point wrong by a factor 3, in either direction, still converges near the true threshold.
        /// </summary>
        [Theory]
        [InlineData(3.0)]
        [InlineData(1.0 / 3.0)]
        public void Simulate_StartingPointWrongByFactorThree_StillConverges(double startFactor)
        {
            const double trueThreshold = 800;

            var results = SimulateMany(trueThreshold, 2.0, trueThreshold * startFactor, RunsPerCase, 3000);
            Report($"start x{startFactor:F2}", trueThreshold, results);

            double medianError = Median(results.Select(result => Math.Abs(result.T50 - trueThreshold) / trueThreshold));

            Assert.True(medianError < 0.15, $"Median error {medianError:P1}");
        }

        /// <summary>
        /// A true threshold close to a bound produces finite estimates within the bounds.
        /// </summary>
        [Theory]
        [InlineData(150.0)]
        [InlineData(4800.0)]
        public void Simulate_ThresholdNearBound_StaysWithinBounds(double trueThreshold)
        {
            var parameters = new CalibrationParameters();

            var results = SimulateMany(trueThreshold, 2.0, trueThreshold * 1.5, 10, 4000);
            Report($"near bound {trueThreshold} ms", trueThreshold, results);

            Assert.All(results, result =>
            {
                Assert.InRange(result.T50, parameters.LowerBound, parameters.UpperBound);
                Assert.True(double.IsFinite(result.R));
            });
        }

        /// <summary>
        /// A participant with the slope estimated by Thomas (1.85) is still estimated within 15 % by a calculator using 2.
        /// </summary>
        [Fact]
        public void Simulate_ParticipantSlope185_MedianErrorBelowFifteenPercent()
        {
            const double trueThreshold = 800;

            var results = SimulateMany(trueThreshold, 1.85, trueThreshold * 1.5, RunsPerCase, 5000);
            Report("participant beta 1.85", trueThreshold, results);

            double medianError = Median(results.Select(result => Math.Abs(result.T50 - trueThreshold) / trueThreshold));

            Assert.True(medianError < 0.15, $"Median error {medianError:P1}");
        }
    }
}
