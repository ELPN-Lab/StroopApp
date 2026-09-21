namespace StroopApp.Models.Simon
{
    public class SimonProfile : ExperimentProfile
    {
        public SimonStimulusMode StimulusMode { get; set; }
        public SimonStimulusPositionMode StimulusPositionMode { get; set; }
        public SimonAnswerMode AnswerMode { get; set; }

        // basic shapes and colors
        public string BaseColor { get; set; } = "#FFFFFF";
        public SimonStimulusShape BaseShape { get; set; } = SimonStimulusShape.Circle;  

        // For color stimulus mode
        public string LeftColor { get; set; } = "#0000FF";
        public string RightColor { get; set; } = "#FF0000";
        public SimonStimulusShape StandardShape { get; set; } = SimonStimulusShape.Circle;
        public SimonStimulusShape ReversedShape { get; set; } = SimonStimulusShape.Square;

        // For shape and arrow stimulus mode
        public SimonStimulusShape LeftShape { get; set; } = SimonStimulusShape.Circle;
        public SimonStimulusShape RightShape { get; set; } = SimonStimulusShape.Square;
        public string StandardColor { get; set; } = "#0072B2";
        public string ReversedColor { get; set; } = "#E69F00";

        public ReversalCuePresentation ReversalCuePresentation { get; set; }
        public int ReversedMappingPercent { get; set; }

        public SimonProfile() : base()
        {
            TaskType = TaskType.Simon;
            StimulusMode = SimonStimulusMode.Color;
            StimulusPositionMode = SimonStimulusPositionMode.LeftRight;
            AnswerMode = SimonAnswerMode.LeftRight;
            ReversedMappingPercent = 0;
            ReversalCuePresentation = ReversalCuePresentation.Integrated;
        }

        public override void UpdateFrom(ExperimentProfile profile)
        {
            base.UpdateFrom(profile);
            if (profile is SimonProfile simonProfile)
            {
                StimulusMode = simonProfile.StimulusMode;
                StimulusPositionMode = simonProfile.StimulusPositionMode;
                AnswerMode = simonProfile.AnswerMode;
                BaseShape = simonProfile.BaseShape;
                BaseColor = simonProfile.BaseColor;
                StandardShape = simonProfile.StandardShape;
                ReversedShape = simonProfile.ReversedShape;
                LeftShape = simonProfile.LeftShape;
                RightShape = simonProfile.RightShape;
                StandardColor = simonProfile.StandardColor;
                ReversedColor = simonProfile.ReversedColor;
                LeftColor = simonProfile.LeftColor;
                RightColor = simonProfile.RightColor;
                ReversalCuePresentation = simonProfile.ReversalCuePresentation;
                ReversedMappingPercent = simonProfile.ReversedMappingPercent;
            }
        }
        public override SimonProfile CloneProfile()
        {
            return new SimonProfile()
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
                StimulusMode = this.StimulusMode,
                StimulusPositionMode = this.StimulusPositionMode,
                AnswerMode = this.AnswerMode,
                BaseShape = this.BaseShape,
                BaseColor = this.BaseColor,
                StandardShape = this.StandardShape,
                ReversedShape = this.ReversedShape,
                LeftShape = this.LeftShape,
                RightShape = this.RightShape,
                StandardColor = this.StandardColor,
                ReversedColor = this.ReversedColor,
                LeftColor = this.LeftColor,
                RightColor = this.RightColor,
                ReversalCuePresentation = this.ReversalCuePresentation,
                ReversedMappingPercent = this.ReversedMappingPercent,
            };
        }
    }
}
