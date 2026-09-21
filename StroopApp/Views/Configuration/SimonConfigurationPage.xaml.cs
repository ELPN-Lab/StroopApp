using StroopApp.Models;
using StroopApp.Services.Exportation;
using StroopApp.Services.KeyMapping;
using StroopApp.Services.Navigation;
using StroopApp.Services.Participant;
using StroopApp.Services.Profile;
using StroopApp.Services.Session;
using StroopApp.Services.Trial;
using StroopApp.Services.Window;
using StroopApp.ViewModels.Configuration;
using StroopApp.ViewModels.Configuration.Participant;
using StroopApp.ViewModels.Configuration.Profile;
using StroopApp.ViewModels.State;
using System.Windows.Controls;

namespace StroopApp.Views.Configuration
{
    /// <summary>
    /// Logique d'interaction pour ConfigurationPage.xaml
    /// </summary>
    public partial class SimonConfigurationPage : Page, INavigationAware
    {
        private readonly ExperimentSettingsViewModel _settings;
        private readonly IWindowManager _windowManager;
        private readonly IProfileService _profileService;
        private readonly IParticipantService _participantService;
        private readonly IKeyMappingService _keyMappingService;
        private readonly IExportationService _exportationService;
        private readonly ITrialGenerationService _trialGenerationService;
        private readonly IExperimentSessionService _sessionService;

        public new INavigationService NavigationService
        {
            set => Initialize(value);
        }
        public SimonConfigurationPage(
            ExperimentSettingsViewModel settings,
            IWindowManager windowManager,
            IProfileService profileService,
            IParticipantService participantService,
            IKeyMappingService keyMappingService,
            IExportationService exportationService,
            ISimonTrialGenerationService trialGenerationService,
            IExperimentSessionService sessionService)
        {
            InitializeComponent();
            _settings = settings;
            _windowManager = windowManager;
            _profileService = profileService;
            _participantService = participantService;
            _keyMappingService = keyMappingService;
            _exportationService = exportationService;
            _trialGenerationService = trialGenerationService;
            _sessionService = sessionService;
        }

        private void Initialize(INavigationService navigationService)
        {
            var profileViewModel = new ProfileManagementViewModel(_profileService, TaskType.Simon);
            var participantViewModel = new ParticipantManagementViewModel(
                _participantService,
                _settings.ExperimentContext.IsParticipantSelectionEnabled);
            var simonKeyMappingViewModel = new SimonResponseMappingViewModel(_keyMappingService);
            var exportFolderSelectorViewModel = new ExportFolderSelectorViewModel(
                _settings, _exportationService);

            DataContext = new SimonConfigurationPageViewModel(
                _settings,
                profileViewModel,
                participantViewModel,
                simonKeyMappingViewModel,
                exportFolderSelectorViewModel,
                navigationService,
                _windowManager,
                _trialGenerationService,
                _sessionService
                );
        }

    }
}