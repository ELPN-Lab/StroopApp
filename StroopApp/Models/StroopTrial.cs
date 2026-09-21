namespace StroopApp.Models
{
    /// <summary>
    /// Represents a single trial in a Stroop task, containing stimulus data, participant responses,
    /// reaction time, and trial metadata such as block and trial number.
    /// </summary>
    public class StroopTrial : ITrial
    {
        public string ParticipantId { get; set; } = string.Empty;
        public bool IsCongruent { get; set; }
        public bool HasVIsualCue { get; set; }
        public int? SwitchPercent { get; set; }
        public int CongruencePercent { get; set; }
        public int DominancePercent { get; set; }
        public int Block { get; set; }
        public Word Stimulus { get; set; } = null!;
        public string ExpectedAnswer { get; set; } = string.Empty;
        public string GivenAnswer { get; set; } = string.Empty;
        public bool? IsValidResponse { get; set; }
        public double? ReactionTime { get; set; }
        public int TrialNumber { get; set; }
        public VisualCueType VisualCue { get; set; }

        /// <summary>
        /// Calculates the expected answer based on trial type (VisualCue, Congruent, or Incongruent).
        /// </summary>
        public void DetermineExpectedAnswer()
        {
            if (HasVIsualCue)
            {
                ExpectedAnswer = (VisualCue == VisualCueType.Square) ? Stimulus.InternalText : Stimulus.Color;
            }
            else if (IsCongruent)
            {
                ExpectedAnswer = Stimulus.InternalText;
            }
            else
            {
                ExpectedAnswer = Stimulus.Color;
            }
        }
    }
}