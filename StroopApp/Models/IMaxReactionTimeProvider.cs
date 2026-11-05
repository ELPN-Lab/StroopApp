namespace StroopApp.Models
{
    /// <summary>
    /// Gives the trial loop the presentation time of each trial and tells it when the block ends early.
    /// </summary>
    public interface IMaxReactionTimeProvider
    {
        /// <summary>
        /// True when the block must end before its last generated trial.
        /// </summary>
        bool IsFinished { get; }

        /// <summary>
        /// Presentation time of the next trial, in ms.
        /// </summary>
        int GetNextMaxReactionTime();

        /// <summary>
        /// Records a completed trial, answered or timed out.
        /// </summary>
        void RecordTrial(ITrial trial);
    }
}
