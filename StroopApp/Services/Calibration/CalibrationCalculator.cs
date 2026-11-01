using StroopApp.Models.Calibration;

namespace StroopApp.Services.Calibration
{
    /// <summary>
    /// Stateless math of the QUEST+ calibration: Weibull function, Bayesian update and convergence.
    /// </summary>
    public static class CalibrationCalculator
    {
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
    }
}
