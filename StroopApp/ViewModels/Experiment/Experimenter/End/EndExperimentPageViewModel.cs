using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.Services.Charts;
using StroopApp.Services.Exportation;
using StroopApp.Services.Navigation;
using StroopApp.Services.Session;
using StroopApp.Services.Summary;
using StroopApp.Services.Window;
using StroopApp.ViewModels.State;
using StroopApp.Views;
using StroopApp.Views.Configuration;
using StroopApp.Views.Experiment.Experimenter.End;
using StroopApp.Views.Home;
using System.Collections.ObjectModel;
using System.Windows;

namespace StroopApp.ViewModels.Experiment.Experimenter.End
{
    /// <summary>
    /// ViewModel for the end-of-experiment page, handling continuation to next block, 
    /// starting new experiments, data export, and application shutdown with confirmations.
    /// </summary>
    public partial class EndExperimentPageViewModel : ViewModelBase, IDisposable
    {
        public ExperimentSettingsViewModel Settings { get; }
        public ObservableCollection<Block> Blocks { get; }
        public GlobalGraphViewModel GlobalGraphViewModel { get; }
        public LiveReactionTimeViewModel LiveReactionTimeViewModel { get; }

        private readonly IExportationService _exportationService;
        private readonly INavigationService _experimenterNavigationService;
        private readonly IWindowManager _windowManager;
        private readonly ExperimentChartFactory _chartFactory;
        private readonly IExperimentSessionService _sessionService;
        private readonly IEnumerable<BlockSummaryFormatter> _summaryFormatters;
        public IReadOnlyList<BlockSummaryColumn> SummaryColumns { get; }


            [ObservableProperty]
        private string _currentParticipant = string.Empty;

        [ObservableProperty]
        private string _currentProfile = string.Empty;

        public EndExperimentPageViewModel(ExperimentSettingsViewModel settings,
                                  IExportationService exportationService,
                                  INavigationService experimenterNavigationService,
                                  IWindowManager windowManager,
                                  IExperimentSessionService sessionService,
                                  IEnumerable<BlockSummaryFormatter> summaryFormatters)
        {
            Settings = settings;
            _exportationService = exportationService;
            _experimenterNavigationService = experimenterNavigationService;
            _windowManager = windowManager;
            _chartFactory = new ExperimentChartFactory();
            _sessionService = sessionService;
            _summaryFormatters = summaryFormatters;

            Blocks = Settings.ExperimentContext.Blocks;
            GlobalGraphViewModel = new GlobalGraphViewModel(settings);
            LiveReactionTimeViewModel = new LiveReactionTimeViewModel(settings);
            CurrentParticipant = string.Format(Strings.Label_CurrentParticipant, Settings.Participant.Id);
            CurrentProfile = string.Format(Strings.Label_CurrentProfile, Settings.CurrentProfile.ProfileName);

            var formatter = GetFormatter();
            SummaryColumns = formatter.GetColumns();

            UpdateBlock();
        }
        private BlockSummaryFormatter GetFormatter()
        {
            return _summaryFormatters.FirstOrDefault(f => f.TaskType == Settings.CurrentProfile.TaskType)
    ?? throw new InvalidOperationException($"Pas de formatter de résumé pour {Settings.CurrentProfile.TaskType}");
        }

        private void UpdateBlock()
        {
            var pointsSnapshot = new ObservableCollection<ReactionTimePoint>(Settings.ExperimentContext.ReactionPoints);
            Settings.ExperimentContext.ColumnSerie = _chartFactory.CreateSnapshotColumnSerie(pointsSnapshot);
        }

        [RelayCommand]
        private async Task Continue()
        {
            try
            {
                _sessionService.PrepareNextBlock();
                NavigateToConfigurationPage();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NewExperiment()
        {
            try
            {
                bool confirmed = await ShowConfirmationDialogAsync(Strings.Title_ConfirmNewExperiment, Strings.Message_ConfirmNewExperiment);
                if (confirmed)
                {
                    _sessionService.ResetForNewExperiment();

                    if(Settings.CurrentProfile.TaskType == TaskType.Stroop)
                        _windowManager.CloseParticipantWindow();
                    else if (Settings.CurrentProfile.TaskType == TaskType.Simon)
                        _windowManager.CloseSimonParticipantWindow();

                    NavigateToConfigurationPage();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Export()
        {
            try
            {
                var exportEndExperimentWindow = new ExportEndExperimentWindow(Settings, _exportationService, _experimenterNavigationService);
                exportEndExperimentWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task QuitWithoutExport()
        {
            try
            {
                if (await ShowConfirmationDialogAsync(Strings.Title_ConfirmShutDown, Strings.Message_ConfirmExitWithoutExport))
                {
                    _sessionService.ResetForNewExperiment();
                    if (Settings.CurrentProfile.TaskType == TaskType.Stroop)
                        _windowManager.CloseParticipantWindow();
                    else if (Settings.CurrentProfile.TaskType == TaskType.Simon)
                        _windowManager.CloseSimonParticipantWindow();
                    _experimenterNavigationService.NavigateTo<HomePage>();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task QuitWithExport()
        {
            try
            {
                if (await ShowConfirmationDialogAsync(Strings.Title_ConfirmShutDown, Strings.Message_ConfirmExitWithExport))
                {
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }
        public void Dispose()
        {
            LiveReactionTimeViewModel.Dispose();
        }

        private void NavigateToConfigurationPage()
        {
            if (Settings.CurrentProfile.TaskType == TaskType.Stroop)
                _experimenterNavigationService.NavigateTo<ConfigurationPage>();
            else if (Settings.CurrentProfile.TaskType == TaskType.Simon)
                _experimenterNavigationService.NavigateTo<SimonConfigurationPage>();
        }

    }
}
