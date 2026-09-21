using StroopApp.Models;

namespace StroopApp.Services.KeyMapping
{
    /// <summary>
    /// Defines contract for key mapping persistence operations.
    /// </summary>
    public interface IKeyMappingService
    {
        /// <summary>
        /// Loads key mappings from storage.
        /// </summary>
        Task<ExperimentKeyMappings> LoadKeyMappings();

        /// <summary>
        /// Saves key mappings to storage.
        /// </summary>
        Task SaveKeyMappings(ExperimentKeyMappings mappings);
    }
}
