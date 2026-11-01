namespace StroopApp.Models.Calibration
{
    /// <summary>
    /// Outcome of a calibration block, shown on the end page and exported.
    /// </summary>
    public class CalibrationResult
    {
        public CalibrationStatus Status { get; init; }

        public CalibrationStopReason StopReason { get; init; }

        /// <summary>
        /// Lower bound of the 90 % interval, in ms; null when the estimation is impossible.
        /// </summary>
        public double? T5 { get; init; }

        /// <summary>
        /// Estimated presentation time reaching the target accuracy, in ms; null when the estimation is impossible.
        /// </summary>
        public double? T50 { get; init; }

        /// <summary>
        /// Upper bound of the 90 % interval, in ms; null when the estimation is impossible.
        /// </summary>
        public double? T95 { get; init; }

        /// <summary>
        /// Relative width of the 90 % interval; null when the estimation is impossible.
        /// </summary>
        public double? R { get; init; }

        public int Phase1TrialCount { get; init; }

        public int Phase2TrialCount { get; init; }

        /// <summary>
        /// Phase 2 starting point, in ms; null when phase 1 was not completed.
        /// </summary>
        public double? StartingPoint { get; init; }

        public required CalibrationParameters Parameters { get; init; }

        /// <summary>
        /// Accuracy read with the end page slider, when the slider was moved.
        /// </summary>
        public double? SliderAccuracy { get; set; }

        /// <summary>
        /// Presentation time read on the curve at the slider accuracy, in ms.
        /// </summary>
        public double? SliderPresentationTime { get; set; }
    }
}
