using CommunityToolkit.Mvvm.Input;
using StroopApp.Core;
using StroopApp.Resources;
using StroopApp.Services.Language;
using StroopApp.Services.Navigation;
using StroopApp.Services.Session;
using StroopApp.Services.Trial;
using StroopApp.Services.Window;
using StroopApp.ViewModels.Configuration.Participant;
using StroopApp.ViewModels.Configuration.Profile;
using StroopApp.ViewModels.State;
using StroopApp.Views;

namespace StroopApp.ViewModels.Configuration
{
    /// <summary>
    /// ViewModel for the main configuration page, coordinating profile, participant, and key mapping selection.
    /// Handles experiment launch with validation and initialization of trial sequences.
    /// </summary>
    public partial class ConfigurationPageViewModel : ViewModelBase
    {
        public ProfileManagementViewModel ProfileViewModel { get; }
        public ParticipantManagementViewModel ParticipantViewModel { get; }
        public KeyMappingViewModel KeyMappingViewModel { get; }
        public ExportFolderSelectorViewModel ExportFolderSelectorViewModel { get; }

        private readonly INavigationService _experimenterNavigationService;
        private readonly IWindowManager _windowManager;
        private readonly ILanguageService _languageService;
        private readonly ITrialGenerationService _trialGenerationService;
        private readonly IExperimentSessionService _sessionService;


        private readonly ExperimentSettingsViewModel _settings;

        public ConfigurationPageViewModel(ExperimentSettingsViewModel settings,
                                  ProfileManagementViewModel profileViewModel,
                                  ParticipantManagementViewModel participantViewModel,
                                  KeyMappingViewModel keyMappingViewModel,
                                  ExportFolderSelectorViewModel exportFolderSelectorViewModel,
                                  INavigationService experimenterNavigationService,
                                  IWindowManager windowManager,
                                  ITrialGenerationService trialGenerationService,
                                  ILanguageService languageService,
                                  IExperimentSessionService sessionService)
        {
            ProfileViewModel = profileViewModel;
            ParticipantViewModel = participantViewModel;
            KeyMappingViewModel = keyMappingViewModel;
            ExportFolderSelectorViewModel = exportFolderSelectorViewModel;
            _experimenterNavigationService = experimenterNavigationService;
            _windowManager = windowManager;
            _trialGenerationService = trialGenerationService;
            _languageService = languageService;
            _settings = settings;
            _sessionService = sessionService;
        }

        [RelayCommand]
        private async Task LaunchExperimentAsync()
        {
            try
            {
                _settings.CurrentProfile = ProfileViewModel.CurrentProfile;
                _settings.Participant = ParticipantViewModel.SelectedParticipant;
                _settings.KeyMappings.Stroop = KeyMappingViewModel.Mappings;

                if (_settings.CurrentProfile == null)
                {
                    await ShowErrorDialogAsync(Strings.Error_SelectProfile);
                    return;
                }

                if (_settings.Participant == null)
                {
                    await ShowErrorDialogAsync(Strings.Error_SelectParticipant);
                    return;
                }

                _sessionService.StartBlock(_trialGenerationService);
                _experimenterNavigationService.NavigateTo<ExperimentDashBoardPage>();
                _windowManager.ShowParticipantWindow(_settings);
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }
    }
}
