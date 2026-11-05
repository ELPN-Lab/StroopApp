namespace StroopApp.Models.Calibration
{
    /// <summary>
    /// Validation rules of <see cref="CalibrationParameters"/>.
    /// </summary>
    public enum CalibrationParametersError
    {
        LowerBoundNotPositive,
        LowerBoundNotBelowUpperBound,
        StartingTimeOutOfBounds,
        TargetAccuracyOutOfRange,
        BetaNotPositive,
        DeltaOutOfRange,
        PriorWidthFactorNotAboveOne,
        MinCorrectOutOfRange,
        MaxPhase2NotPositive,
        MinPhase2AboveMax,
        ConvergedAboveNearlyConverged
    }
}
