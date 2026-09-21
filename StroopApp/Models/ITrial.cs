namespace StroopApp.Models
{
    public interface ITrial
    {
        int TrialNumber { get; set;  }
        public int Block { get; set; }
        public string ParticipantId { get; set; }
        public double? ReactionTime { get; set; }
        public bool? IsValidResponse { get; set; }
        public bool IsCongruent { get; set;  }
    }
}
