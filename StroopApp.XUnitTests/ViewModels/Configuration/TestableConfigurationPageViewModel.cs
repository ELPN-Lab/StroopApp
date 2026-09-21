using StroopApp.Services.Navigation;
using StroopApp.Services.Trial;
using StroopApp.Services.Window;
using StroopApp.Services.Language;
using StroopApp.ViewModels.Configuration;
using StroopApp.ViewModels.Configuration.Participant;
using StroopApp.ViewModels.Configuration.Profile;
using StroopApp.ViewModels.State;
using StroopApp.Services.Session;

namespace StroopApp.XUnitTests.ViewModels.Configuration
{
    public class TestableConfigurationPageViewModel : ConfigurationPageViewModel
    {
        public bool ErrorDialogShown = false;
        public string? LastErrorMessage = null;

        public TestableConfigurationPageViewModel(
            ExperimentSettingsViewModel settings,
            ProfileManagementViewModel profileViewModel,
            ParticipantManagementViewModel participantViewModel,
            KeyMappingViewModel keyMappingViewModel,
            ExportFolderSelectorViewModel exportFolderSelectorViewModel,
            INavigationService experimenterNavigationService,
            IWindowManager windowManager,
            ITrialGenerationService trialGenerationService,
            ILanguageService languageService,
            IExperimentSessionService sessionService
        ) : base(settings, profileViewModel, participantViewModel, keyMappingViewModel, exportFolderSelectorViewModel, experimenterNavigationService, windowManager, trialGenerationService, languageService, sessionService)
        { }

        protected override Task ShowErrorDialogAsync(string message)
        {
            ErrorDialogShown = true;
            LastErrorMessage = message;
            return Task.CompletedTask;
        }
    }
}
