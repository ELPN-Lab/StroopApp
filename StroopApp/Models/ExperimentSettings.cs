namespace StroopApp.Models
{
    /// <summary>
    /// Holds the current configuration state for an experiment, including the selected participant,
    /// preset profile, key mappings, shared context, and current block index.
    /// Refactored to delegate responsibilities to specialized context objects.
    /// </summary>
    public class ExperimentSettings
    {
        // Internal context objects (composition) - single source of truth
        private readonly ExperimentConfiguration _configuration;
        private readonly ParticipantContext _participantContext;
        private readonly ExperimentRunState _runState;

        public int Block
        {
            get => _runState.Block;
            set => _runState.Block = value;
        }

        public Participant Participant
        {
            get => _participantContext.Participant;
            set => _participantContext.Participant = value;
        }

        public ExperimentProfile? CurrentProfile
        {
            get => _configuration.Profile;
            set => _configuration.Profile = value;
        }

        public ExperimentKeyMappings KeyMappings
        {
            get => _configuration.KeyMappings;
            set => _configuration.KeyMappings = value;
        }

        public SharedExperimentData ExperimentContext
        {
            get => _runState.ExperimentContext;
            set => _runState.ExperimentContext = value;
        }

        public string ExportFolderPath
        {
            get => _configuration.ExportFolderPath;
            set => _configuration.ExportFolderPath = value;
        }

        /// <summary>
        /// Resets the experiment to its initial state, clearing trials and enabling participant selection.
        /// </summary>
        public void Reset()
        {
            // Preserve exact order of execution from characterization tests
            ExperimentContext.Reset();
            Block = 1;
            ExperimentContext.IsBlockFinished = true;
            ExperimentContext.IsParticipantSelectionEnabled = true;
            ExperimentContext.HasUnsavedExports = true;
        }

        public ExperimentSettings()
        {
            // Initialize internal contexts - single source of truth
            _configuration = new ExperimentConfiguration();
            _participantContext = new ParticipantContext();
            _runState = new ExperimentRunState();
        }
    }
}