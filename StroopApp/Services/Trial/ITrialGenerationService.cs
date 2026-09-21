using StroopApp.Models;
using StroopApp.ViewModels.State;

namespace StroopApp.Services.Trial
{
    /// <summary>
    /// Defines contract for generating trial sequences.
    /// </summary>
    public interface ITrialGenerationService
    {
        /// <summary>
        /// Generates a list of trials based on experiment parameters.
        /// </summary>
        /// <param name="settings">Experiment settings</param>
        /// <returns>List of generated trials</returns>
        List<ITrial> GenerateTrials(ExperimentSettingsViewModel settings);

        
    }
}
