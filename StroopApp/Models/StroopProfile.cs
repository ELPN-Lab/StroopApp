namespace StroopApp.Models
{
    public class StroopProfile : ExperimentProfile
    {
        public StroopProfile() : base()
        {
            TaskType = TaskType.Stroop;
        }

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
                TaskLanguage = this.TaskLanguage
            };
        }
    }
}
