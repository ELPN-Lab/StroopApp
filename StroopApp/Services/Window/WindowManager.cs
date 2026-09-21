using StroopApp.Services.Navigation.PageFactory;
using StroopApp.ViewModels.Experiment.Participant;
using StroopApp.ViewModels.Experiment.Participant.Instructions;
using StroopApp.ViewModels.State;
using StroopApp.Views;
using StroopApp.Views.Experiment.Participant;
using StroopApp.Views.Experiment.Participant.Simon;

namespace StroopApp.Services.Window
{
    /// <summary>
    /// Service for managing application windows (creation, activation, cleanup).
    /// Ensures single instance of participant management window.
    /// </summary>
    public class WindowManager : IWindowManager
    {
        private ParticipantWindow? _participantWindow;
        private ParticipantWindow? _simonParticipantWindow;
        private readonly IPageFactory _pageFactory;

        public WindowManager(IPageFactory pageFactory)
        {
            _pageFactory = pageFactory;
        }

        /// <summary>
        /// Shows or activates the participant management window.
        /// Creates a new instance if none exists, otherwise resets and activates the existing one.
        /// </summary>
        public void ShowParticipantWindow(ExperimentSettingsViewModel settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (_participantWindow == null)
            {
                _participantWindow = new ParticipantWindow(settings, _pageFactory,
                    (s, nav) => new ParticipantWindowViewModel(s, nav,
                    () => new InstructionsPage(new StroopInstructionsViewModel(s, nav, () => new StroopPage(s, nav)))));
                _participantWindow.Closed += (_, _) => _participantWindow = null;
                _participantWindow.Show();
            }
            else
            {
                _participantWindow.Reset();
                _participantWindow.Activate();

            }
        }

        public void ShowSimonParticipantWindow(ExperimentSettingsViewModel settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (_simonParticipantWindow is null)
            {
                _simonParticipantWindow = new ParticipantWindow(settings, _pageFactory,
                    (s, nav) => new ParticipantWindowViewModel(s, nav,
                    () => new InstructionsPage(new SimonInstructionsViewModel(s, nav, () => new SimonPage(s, nav)))));
                _simonParticipantWindow.Closed += (_, _) => _simonParticipantWindow = null;
                _simonParticipantWindow.Show();
            }
            else
            {
                _simonParticipantWindow.Reset();
                _simonParticipantWindow.Activate();
            }
            ;
        }

        /// <summary>
        /// Closes the participant management window if open.
        /// </summary>
        public void CloseParticipantWindow()
        {
            _participantWindow?.Close();
            _participantWindow = null;
        }

        public void CloseSimonParticipantWindow()
        {
            _simonParticipantWindow?.Close();
            _simonParticipantWindow = null;
        }
    }
}
