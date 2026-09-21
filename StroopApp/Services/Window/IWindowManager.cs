using StroopApp.Models;
using StroopApp.ViewModels.State;

namespace StroopApp.Services.Window
{
    /// <summary>
    /// Defines contract for managing application windows.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Shows or activates the participant management window.
        /// </summary>
        void ShowParticipantWindow(ExperimentSettingsViewModel settings);

        // <summary>
        /// Shows or activates the participant management window for the simon task.
        /// </summary>
        void ShowSimonParticipantWindow(ExperimentSettingsViewModel settings);

        /// <summary>
        /// Closes the participant management window.
        /// </summary>
        void CloseParticipantWindow();

        /// <summary>
        /// Closes the participant simon task management window.
        /// </summary>
        void CloseSimonParticipantWindow();

    }
}
