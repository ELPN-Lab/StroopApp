using StroopApp.Models.Calibration;

namespace StroopApp.Models
{
    public class StroopProfile : ExperimentProfile
    {
        public StroopProfile() : base()
        {
            TaskType = TaskType.Stroop;
        }

        public bool IsCalibration { get; set; }

        public CalibrationParameters CalibrationParameters { get; set; } = new();

        public override void UpdateFrom(ExperimentProfile profile)
        {
            base.UpdateFrom(profile);
            if (profile is StroopProfile stroopProfile)
            {
                IsCalibration = stroopProfile.IsCalibration;
                CalibrationParameters = stroopProfile.CalibrationParameters.Clone();
            }
        }

        /// <summary>
        /// A calibration trial can last up to the calibration upper bound.
        /// </summary>
        public override int GetLongestReactionTime() => IsCalibration ? CalibrationParameters.UpperBound : MaxReactionTime;

        public override StroopProfile CloneProfile()
        {
            return new StroopProfile()
            {
                Id = this.Id,
                ProfileName = this.ProfileName,
                TaskType = this.TaskType,
                CalculationMode = this.CalculationMode,
                Hours = this.Hours,
                Minutes = this.Minutes,
                Seconds = this.Seconds,
                TaskDuration = this.TaskDuration,
                WordDuration = this.WordDuration,
                MaxReactionTime = this.MaxReactionTime,
                GroupSize = this.GroupSize,
                VisualCueDuration = this.VisualCueDuration,
                FixationDuration = this.FixationDuration,
                WordCount = this.WordCount,
                HasVisualCue = this.HasVisualCue,
                DominantPercent = this.DominantPercent,
                CongruencePercent = this.CongruencePercent,
                SwitchPercent = this.SwitchPercent,
                TaskLanguage = this.TaskLanguage,
                IsCalibration = this.IsCalibration,
                CalibrationParameters = this.CalibrationParameters.Clone()
            };
        }
    }
}
