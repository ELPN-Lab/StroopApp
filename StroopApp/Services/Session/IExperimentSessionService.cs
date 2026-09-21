using StroopApp.Services.Trial;

namespace StroopApp.Services.Session
{
    /// <summary>
    /// Manages the lifecycle of an experiment session. 
    /// Handles Block initialisation, completion, manual abort, and reset. 
    /// Acts as the single source of truth for session state mutations.
    /// </summary>
    public interface IExperimentSessionService
    {
        /// <summary>
        /// Resets session flags, generate trials and insert it in the current block.
        /// </summary>
        /// <param name="trialGenerationService"></param>
        void StartBlock(ITrialGenerationService trialGenerationService);

        /// <summary>
        /// Marks the current block as finished after all trials completed.
        /// Calculates block statistics before signaling completion
        /// </summary>
        void CompleteBlock();

        /// <summary>
        /// Stops the session as the experimenter's command.
        /// Calculates partial block statistics and signal block completion.
        /// </summary>
        void AbortSession();

        /// <summary>
        /// Clears reaction points data and increments the block counter.
        /// </summary>
        void PrepareNextBlock();

        /// <summary>
        /// Resets all experiment data and configuration to their inintial state.
        /// </summary>
        void ResetForNewExperiment();

    }
}
