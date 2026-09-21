namespace StroopApp.Models.Simon
{
    /// <summary>
    /// Represents a single trial in a Stroop task, containing stimulus data, participant responses,
    /// reaction time, and trial metadata such as block and trial number.
    /// </summary>
    public class SimonTrial : ITrial
    {
        public string ParticipantId { get; set; } = string.Empty;
        public bool IsSpatialCongruent { get; set; }
        public bool IsCongruent // unused but to be conform to the interface
        {
            get => IsSpatialCongruent;
            set => IsSpatialCongruent = value;
        }
        public bool IsAnswerCongruent { get; set; }
        public bool IsReversedMapping { get; set; }
        public int? ReversedMappingPercent { get; set; }
        public int CongruencePercent { get; set; } // spatial congruence (left stimulus placed left, independently from the reversal rule
        public int Block { get; set; }
        public SimonStimulus Stimulus { get; set; } = null!;
        public SimonAnswer ExpectedAnswer { get; set; }
        public SimonAnswer GivenAnswer { get; set; }
        public bool? IsValidResponse { get; set; }
        public double? ReactionTime { get; set; }
        public int TrialNumber { get; set; }
    }
}