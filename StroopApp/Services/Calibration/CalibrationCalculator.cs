using StroopApp.Models.Calibration;

namespace StroopApp.Services.Calibration
{
    /// <summary>
    /// Stateless math of the QUEST+ calibration: Weibull function, Bayesian update and convergence.
    /// </summary>
    public static class CalibrationCalculator
    {
        private const double GridStep = 0.005;
        private const double TieTolerance = 1e-12;

        /// <summary>
        /// Probability of a correct answer at the given presentation time, the threshold being the time that reaches the target accuracy.
        /// </summary>
        public static double Probability(double presentationTime, double threshold, CalibrationParameters parameters)
        {
            double alpha = threshold / ThresholdFactor(parameters.TargetAccuracy, parameters);
            return Weibull(presentationTime, alpha, parameters);
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

        /// <summary>
        /// QUEST+ choice: the candidate presentation time in ms that minimizes the expected entropy after the answer; ties go to the shortest time.
        /// </summary>
        public static int SelectNextPresentationTime(double[] logWeights, double[] grid, CalibrationParameters parameters)
        {
            double[] weights = logWeights.Select(Math.Exp).ToArray();
            double factor = ThresholdFactor(parameters.TargetAccuracy, parameters);

            int bestTime = 0;
            double bestEntropy = double.MaxValue;
            foreach (int candidate in grid.Select(time => (int)Math.Round(time)).Distinct())
            {
                double entropy = ExpectedEntropy(weights, grid, candidate, factor, parameters);
                if (entropy < bestEntropy - TieTolerance)
                {
                    bestEntropy = entropy;
                    bestTime = candidate;
                }
            }

            return bestTime;
        }

        /// <summary>
        /// Threshold in ms below which lies the given probability of the weights; each candidate's weight is centered on it and the distribution is interpolated linearly in log10.
        /// </summary>
        public static double Quantile(double[] logWeights, double[] grid, double probability)
        {
            double cumulative = 0;
            double previousMiddle = 0;
            double previousLogTime = 0;

            for (int i = 0; i < grid.Length; i++)
            {
                double weight = Math.Exp(logWeights[i]);
                double middle = cumulative + weight / 2;
                cumulative += weight;
                double logTime = Math.Log10(grid[i]);

                if (middle >= probability)
                {
                    if (i == 0)
                        return grid[0];

                    double fraction = (probability - previousMiddle) / (middle - previousMiddle);
                    return Math.Pow(10, previousLogTime + fraction * (logTime - previousLogTime));
                }

                previousMiddle = middle;
                previousLogTime = logTime;
            }

            return grid[^1];
        }

        /// <summary>
        /// Relative width of the credible interval: (t95 - t5) / t50.
        /// </summary>
        public static double ComputeR(double t5, double t50, double t95) => (t95 - t5) / t50;

        /// <summary>
        /// Convergence status for the given R.
        /// </summary>
        public static CalibrationStatus GetStatus(double r, CalibrationParameters parameters)
        {
            if (r <= parameters.ConvergedThreshold)
                return CalibrationStatus.Converged;
            if (r <= parameters.NearlyConvergedThreshold)
                return CalibrationStatus.NearlyConverged;
            return CalibrationStatus.NotConverged;
        }

        /// <summary>
        /// Whether phase 2 stops: maximum reached, or minimum reached with a converged R.
        /// </summary>
        public static bool ShouldStop(int phase2TrialCount, double r, CalibrationParameters parameters)
        {
            return phase2TrialCount >= parameters.MaxPhase2TrialCount
                || (phase2TrialCount >= parameters.MinPhase2TrialCount && r <= parameters.ConvergedThreshold);
        }

        /// <summary>
        /// Presentation time that reaches the given accuracy, read on the curve of the estimated threshold.
        /// </summary>
        public static double PresentationTimeAtAccuracy(double t50, double accuracy, CalibrationParameters parameters)
        {
            double alpha = t50 / ThresholdFactor(parameters.TargetAccuracy, parameters);
            return alpha * ThresholdFactor(accuracy, parameters);
        }

        private static double[] Normalize(double[] logWeights)
        {
            double max = logWeights.Max();
            double logSum = max + Math.Log(logWeights.Sum(logWeight => Math.Exp(logWeight - max)));
            return logWeights.Select(logWeight => logWeight - logSum).ToArray();
        }

        private static double Weibull(double presentationTime, double alpha, CalibrationParameters parameters)
        {
            double range = 1 - parameters.Gamma - parameters.Delta;
            return parameters.Gamma + range * (1 - Math.Exp(-Math.Pow(presentationTime / alpha, parameters.Beta)));
        }

        private static double ExpectedEntropy(double[] weights, double[] grid, int presentationTime, double factor, CalibrationParameters parameters)
        {
            var afterCorrect = new double[weights.Length];
            var afterError = new double[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                double probability = Weibull(presentationTime, grid[i] / factor, parameters);
                afterCorrect[i] = weights[i] * probability;
                afterError[i] = weights[i] * (1 - probability);
            }

            double correctProbability = afterCorrect.Sum();
            double errorProbability = afterError.Sum();
            return correctProbability * Entropy(afterCorrect, correctProbability) + errorProbability * Entropy(afterError, errorProbability);
        }

        private static double Entropy(double[] unnormalizedWeights, double total)
        {
            double entropy = 0;
            foreach (double weight in unnormalizedWeights)
            {
                if (weight <= 0)
                    continue;

                double probability = weight / total;
                entropy -= probability * Math.Log(probability);
            }

            return entropy;
        }
    }
}
