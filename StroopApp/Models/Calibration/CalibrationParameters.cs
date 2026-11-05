namespace StroopApp.Models.Calibration
{
    /// <summary>
    /// Parameters of the QUEST+ calibration procedure, stored in the Stroop profile.
    /// </summary>
    public class CalibrationParameters
    {
        /// <summary>
        /// Phase 1 presentation time, in ms.
        /// </summary>
        public int StartingTime { get; set; } = 2000;

        /// <summary>
        /// Lowest presentation time, in ms.
        /// </summary>
        public int LowerBound { get; set; } = 140;

        /// <summary>
        /// Highest presentation time, in ms.
        /// </summary>
        public int UpperBound { get; set; } = 5000;

        /// <summary>
        /// Target accuracy, as a fraction (0.85 for 85 %).
        /// </summary>
        public double TargetAccuracy { get; set; } = 0.85;

        /// <summary>
        /// Guess rate, fixed by the four possible answers.
        /// </summary>
        public double Gamma { get; } = 0.25;

        /// <summary>
        /// Lapse rate.
        /// </summary>
        public double Delta { get; set; } = 0.01;

        /// <summary>
        /// Slope of the Weibull function.
        /// </summary>
        public double Beta { get; set; } = 2.0;

        public int Phase1TrialCount { get; set; } = 20;

        /// <summary>
        /// Below this number of correct phase 1 answers, phase 2 starts from the starting time.
        /// </summary>
        public int MinCorrectPhase1Trials { get; set; } = 5;

        public int MinPhase2TrialCount { get; set; } = 40;

        public int MaxPhase2TrialCount { get; set; } = 100;

        /// <summary>
        /// One prior standard deviation equals this time factor.
        /// </summary>
        public double PriorWidthFactor { get; set; } = 2.0;

        public double ConvergedThreshold { get; set; } = 0.20;

        public double NearlyConvergedThreshold { get; set; } = 0.40;

        /// <summary>
        /// Returns an independent copy of these parameters.
        /// </summary>
        public CalibrationParameters Clone() => (CalibrationParameters)MemberwiseClone();

        /// <summary>
        /// Returns the validation rules broken by these parameters, or an empty list.
        /// </summary>
        public IReadOnlyList<CalibrationParametersError> GetValidationErrors()
        {
            var errors = new List<CalibrationParametersError>();

            if (LowerBound <= 0)
                errors.Add(CalibrationParametersError.LowerBoundNotPositive);
            if (LowerBound >= UpperBound)
                errors.Add(CalibrationParametersError.LowerBoundNotBelowUpperBound);
            if (StartingTime < LowerBound || StartingTime > UpperBound)
                errors.Add(CalibrationParametersError.StartingTimeOutOfBounds);
            if (TargetAccuracy <= Gamma || TargetAccuracy >= 1 - Delta)
                errors.Add(CalibrationParametersError.TargetAccuracyOutOfRange);
            if (Beta <= 0)
                errors.Add(CalibrationParametersError.BetaNotPositive);
            if (Delta <= 0 || Delta >= 1 - Gamma)
                errors.Add(CalibrationParametersError.DeltaOutOfRange);
            if (PriorWidthFactor <= 1)
                errors.Add(CalibrationParametersError.PriorWidthFactorNotAboveOne);
            if (MinCorrectPhase1Trials < 2 || MinCorrectPhase1Trials > Phase1TrialCount)
                errors.Add(CalibrationParametersError.MinCorrectOutOfRange);
            if (MaxPhase2TrialCount <= 0)
                errors.Add(CalibrationParametersError.MaxPhase2NotPositive);
            if (MinPhase2TrialCount > MaxPhase2TrialCount)
                errors.Add(CalibrationParametersError.MinPhase2AboveMax);
            if (ConvergedThreshold > NearlyConvergedThreshold)
                errors.Add(CalibrationParametersError.ConvergedAboveNearlyConverged);

            return errors;
        }
    }
}
