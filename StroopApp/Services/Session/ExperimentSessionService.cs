using DocumentFormat.OpenXml.Wordprocessing;
using StroopApp.Services.Trial;
using StroopApp.ViewModels.State;

namespace StroopApp.Services.Session
{
    internal class ExperimentSessionService : IExperimentSessionService
    {
        private readonly ExperimentSettingsViewModel _settings;

        public ExperimentSessionService(ExperimentSettingsViewModel settings)
        {
            _settings = settings;
        }
        public void StartBlock(ITrialGenerationService trialGenerationService)
        {
            _settings.ExperimentContext.IsTaskStopped = false;
            _settings.ExperimentContext.IsBlockFinished = false;
            _settings.ExperimentContext.NewColumnSerie();
            _settings.ExperimentContext.AddNewSerie(_settings);

            if (_settings.ExperimentContext.CurrentBlock is null)
                throw new InvalidOperationException("CurrentBlock was not Initialized after AddNewSerie");

            var trials = trialGenerationService.GenerateTrials(_settings);
            foreach (var trial in trials)
                _settings.ExperimentContext.CurrentBlock.TrialRecords.Add(trial);
        }
        public void CompleteBlock()
        {
            if (_settings.ExperimentContext.CurrentBlock is null) return;

            _settings.ExperimentContext.CurrentBlock.CalculateValues();
            _settings.ExperimentContext.CurrentTrial = null;
            _settings.ExperimentContext.IsBlockFinished = true;
        }
        public void PrepareNextBlock()
        {
            _settings.ExperimentContext.ReactionPoints.Clear();
            _settings.ExperimentContext.NewColumnSerie();
            _settings.Block++;
            _settings.ExperimentContext.IsBlockFinished = false;
            _settings.ExperimentContext.IsParticipantSelectionEnabled = false;
            _settings.ExperimentContext.HasUnsavedExports = true;
        }
        public void ResetForNewExperiment()
        {
            _settings.Reset();
        }
        public void AbortSession()
        {
            _settings.ExperimentContext.IsTaskStopped = true;

            if (_settings.ExperimentContext.CurrentBlock is not null)
            {
                _settings.ExperimentContext.CurrentBlock.CalculateValues();
                _settings.ExperimentContext.CurrentTrial = null;
            }

            _settings.ExperimentContext.IsBlockFinished = true;
        }
    }
}
