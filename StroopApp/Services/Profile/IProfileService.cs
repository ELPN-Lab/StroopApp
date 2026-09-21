using StroopApp.Models;
using System.Collections.ObjectModel;

namespace StroopApp.Services.Profile
{
    /// <summary>
    /// Defines contract for experiment profile persistence and last selection tracking.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Loads all experiment profiles from storage.
        /// </summary>
        ObservableCollection<ExperimentProfile> LoadProfiles();

        /// <summary>
        /// Saves all experiment profiles to storage.
        /// </summary>
        void SaveProfiles(ObservableCollection<ExperimentProfile> profiles);

        /// <summary>
        /// Inserts or updates a profile and returns the refreshed collection.
        /// </summary>
        ObservableCollection<ExperimentProfile> UpsertProfile(ExperimentProfile profile);

        /// <summary>
        /// Deletes a profile from the collection.
        /// </summary>
        void DeleteProfile(ExperimentProfile profile);

        /// <summary>
        /// Loads the last selected profile ID.
        /// </summary>
        Guid? LoadLastSelectedProfile();

        /// <summary>
        /// Saves the currently selected profile ID.
        /// </summary>
        void SaveLastSelectedProfile(ExperimentProfile profile);
    }
}
