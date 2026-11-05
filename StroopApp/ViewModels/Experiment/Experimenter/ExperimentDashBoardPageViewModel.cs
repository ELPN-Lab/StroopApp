using CommunityToolkit.Mvvm.Input;
using StroopApp.Core;
using StroopApp.Resources;
using StroopApp.Services.Language;
using StroopApp.Services.Navigation;
using StroopApp.Services.Window;
using StroopApp.ViewModels.State;
using StroopApp.Views.Experiment.Experimenter;
using System.ComponentModel;

namespace StroopApp.ViewModels.Experiment.Experimenter
{
    /// <summary>
    /// ViewModel for the experiment dashboard, monitoring block completion and handling manual task termination.
    /// Automatically navigates to end page when block finishes.
    /// </summary>
    public partial class ExperimentDashBoardPageViewModel : ViewModelBase, IDisposable
    {
        private readonly ExperimentSettingsViewModel _settings;
        private readonly INavigationService _experimenterNavigationService;
        private bool _hasNavigatedToEndPage;

        public ExperimentProgressViewModel ExperimentProgressViewModel { get; }
        public GraphsViewModel GraphsViewModel { get; }

        public ExperimentDashBoardPageViewModel(
            ExperimentSettingsViewModel settings,
            INavigationService experimenterNavigationService,
            IWindowManager windowManager,
            ILanguageService languageService)
        {
            _settings = settings;
            _experimenterNavigationService = experimenterNavigationService;

            ExperimentProgressViewModel = new ExperimentProgressViewModel(settings);
            GraphsViewModel = new GraphsViewModel(settings);

            _settings.ExperimentContext.PropertyChanged += OnExperimentContextPropertyChanged;
        }

        private void OnExperimentContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_hasNavigatedToEndPage)
                return;

            if (e.PropertyName == nameof(_settings.ExperimentContext.IsBlockFinished)
                && _settings.ExperimentContext.IsBlockFinished)
            {
                _hasNavigatedToEndPage = true;
                _experimenterNavigationService.NavigateTo<EndExperimentPage>();
            }
        }

        [RelayCommand]
        public async Task StopTaskAsync()
        {
            try
            {
                bool confirmed = await ShowConfirmationDialogAsync(Strings.Title_ConfirmStopTask, Strings.Message_StopTask);
                if (!confirmed)
                    return;

                _settings.ExperimentContext.IsTaskStopped = true;
                _settings.ExperimentContext.StopCalibrationBlock();

                if (_settings.ExperimentContext.CurrentBlock != null)
                {
                    _settings.ExperimentContext.CurrentBlock.CalculateValues();
                    _settings.ExperimentContext.CurrentTrial = null;
                }

                _settings.ExperimentContext.IsBlockFinished = true;
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _settings.ExperimentContext.PropertyChanged -= OnExperimentContextPropertyChanged;
            ExperimentProgressViewModel.Dispose();
            GraphsViewModel.Dispose();
        }
    }
}
