using StroopApp.Models.Calibration;

namespace StroopApp.Services.Calibration
{
    /// <summary>
    /// Stateless math of the QUEST+ calibration: Weibull function, Bayesian update and convergence.
    /// </summary>
    public static class CalibrationCalculator
    {
        private const double GridStep = 0.005;

        /// <summary>
        /// Probability of a correct answer at the given presentation time, the threshold being the time that reaches the target accuracy.
        /// </summary>
        public static double Probability(double presentationTime, double threshold, CalibrationParameters parameters)
        {
            double alpha = threshold / ThresholdFactor(parameters.TargetAccuracy, parameters);
            double range = 1 - parameters.Gamma - parameters.Delta;
            return parameters.Gamma + range * (1 - Math.Exp(-Math.Pow(presentationTime / alpha, parameters.Beta)));
        }

        /// <summary>
        /// Ratio between the presentation time that reaches the given accuracy and the Weibull scale alpha.
        /// </summary>
        public static double ThresholdFactor(double accuracy, CalibrationParameters parameters)
        {
            double fraction = (accuracy - parameters.Gamma) / (1 - parameters.Gamma - parameters.Delta);
            return Math.Pow(-Math.Log(1 - fraction), 1 / parameters.Beta);
        }

        /// <summary>
        /// Phase 2 starting point: mean plus sample standard deviation of the correct phase 1 reaction times, or the starting time when there are too few, clamped to the bounds.
        /// </summary>
        public static double ComputeStartingPoint(IReadOnlyList<double> correctReactionTimes, CalibrationParameters parameters)
        {
            if (correctReactionTimes.Count < parameters.MinCorrectPhase1Trials)
                return parameters.StartingTime;

            double mean = correctReactionTimes.Average();
            double variance = correctReactionTimes.Sum(time => (time - mean) * (time - mean)) / (correctReactionTimes.Count - 1);
            return Math.Clamp(mean + Math.Sqrt(variance), parameters.LowerBound, parameters.UpperBound);
        }

        /// <summary>
        /// Candidate thresholds in ms, evenly spaced in log10 from the lower bound to the upper bound included.
        /// </summary>
        public static double[] CreateGrid(CalibrationParameters parameters)
        {
            double logLower = Math.Log10(parameters.LowerBound);
            double logUpper = Math.Log10(parameters.UpperBound);
            int intervals = (int)Math.Ceiling((logUpper - logLower) / GridStep);
            double step = (logUpper - logLower) / intervals;

            var grid = new double[intervals + 1];
            for (int i = 0; i <= intervals; i++)
                grid[i] = Math.Pow(10, logLower + i * step);

            grid[0] = parameters.LowerBound;
            grid[intervals] = parameters.UpperBound;
            return grid;
        }

        /// <summary>
        /// Normalized log weights of a Gaussian prior in log10, centered on the starting point.
        /// </summary>
        public static double[] CreatePrior(double[] grid, double startingPoint, CalibrationParameters parameters)
        {
            double sigma = Math.Log10(parameters.PriorWidthFactor);
            double center = Math.Log10(startingPoint);

            var logWeights = grid.Select(threshold =>
            {
                double distance = (Math.Log10(threshold) - center) / sigma;
                return -0.5 * distance * distance;
            }).ToArray();

            return Normalize(logWeights);
        }

        /// <summary>
        /// Normalized log weights after one phase 2 trial; the input array is left unchanged.
        /// </summary>
        public static double[] Update(double[] logWeights, double[] grid, int presentationTime, bool isCorrect, CalibrationParameters parameters)
        {
            var updated = new double[logWeights.Length];
            for (int i = 0; i < logWeights.Length; i++)
            {
                double probability = Probability(presentationTime, grid[i], parameters);
                updated[i] = logWeights[i] + Math.Log(isCorrect ? probability : 1 - probability);
            }

            return Normalize(updated);
        }

        private static double[] Normalize(double[] logWeights)
        {
            double max = logWeights.Max();
            double logSum = max + Math.Log(logWeights.Sum(logWeight => Math.Exp(logWeight - max)));
            return logWeights.Select(logWeight => logWeight - logSum).ToArray();
        }
    }
}
