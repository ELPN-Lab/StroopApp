using DocumentFormat.OpenXml.Bibliography;

namespace StroopApp.Models
{
    public enum CalculationMode
    {
        TaskDuration,
        WordCount
    }

    public abstract class ExperimentProfile
    {
        public ExperimentProfile()
        {
            Id = Guid.NewGuid();
            ProfileName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "fr" => "Nouveau profil",
                "en" => "New profile",
                _ => "New profile"
            };
            FixationDuration = 100;
            MaxReactionTime = 400;
            VisualCueDuration = 0;
            GroupSize = 5;
            HasVisualCue = false;
            WordCount = 10;
            CalculationMode = CalculationMode.WordCount;
            CongruencePercent = 50;
            DominantPercent = 50;
            SwitchPercent = null;
            UpdateDerivedValues();

            var currentCultureCode = Thread.CurrentThread.CurrentUICulture?.TwoLetterISOLanguageName;
            TaskLanguage = string.IsNullOrWhiteSpace(currentCultureCode) ? "en" : currentCultureCode;
        }

        public Guid Id { get; set; }
        public string ProfileName { get; set; }
        public TaskType TaskType { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int WordDuration { get; set; }
        public int FixationDuration { get; set; }
        public int VisualCueDuration { get; set; }
        public bool HasVisualCue { get; set; }
        public int GroupSize { get; set; }
        public int TaskDuration { get; set; }
        public int WordCount { get; set; }
        public int MaxReactionTime { get; set; }
        public CalculationMode CalculationMode { get; set; }
        public int DominantPercent { get; set; }
        public int CongruencePercent { get; set; }
        public int? SwitchPercent { get; set; }
        public string TaskLanguage { get; set; }

        public virtual void UpdateFrom(ExperimentProfile profile)
        {
            ProfileName = profile.ProfileName;
            TaskType = profile.TaskType;
            Hours = profile.Hours;
            Minutes = profile.Minutes;
            Seconds = profile.Seconds;
            WordDuration = profile.WordDuration;
            FixationDuration = profile.FixationDuration;
            VisualCueDuration = profile.VisualCueDuration;
            HasVisualCue = profile.HasVisualCue;
            GroupSize = profile.GroupSize;
            TaskDuration = profile.TaskDuration;
            WordCount = profile.WordCount;
            MaxReactionTime = profile.MaxReactionTime;
            CalculationMode = profile.CalculationMode;
            DominantPercent = profile.DominantPercent;
            CongruencePercent = profile.CongruencePercent;
            SwitchPercent = profile.SwitchPercent;
            TaskLanguage = profile.TaskLanguage;
        }

        public void UpdateDerivedValues()
        {
            WordDuration = MaxReactionTime + FixationDuration + VisualCueDuration;
            if (CalculationMode == CalculationMode.TaskDuration)
            {
                TaskDuration = ((Hours * 3600) + (Minutes * 60) + Seconds) * 1000;
                if (WordDuration > 0)
                {
                    WordCount = TaskDuration / WordDuration;
                }
            }
            else if (CalculationMode == CalculationMode.WordCount)
            {
                TaskDuration = WordCount * WordDuration;
                Hours = TaskDuration / 3600000;
                Minutes = (TaskDuration % 3600000) / 60000;
                Seconds = (TaskDuration % 60000) / 1000;
            }
        }

        public abstract ExperimentProfile CloneProfile();
    }
}