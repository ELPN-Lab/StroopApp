using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.Services.Exportation;
using StroopApp.Services.Navigation;
using StroopApp.ViewModels.State;
using StroopApp.ViewModels.Configuration;
using StroopApp.Views;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DocumentFormat.OpenXml.Wordprocessing;
using StroopApp.Views.Configuration;

namespace StroopApp.ViewModels.Experiment.Experimenter.End
{
    /// <summary>
    /// ViewModel for the export dialog window, managing export workflow states (exporting, success, error),
    /// file opening, and post-export actions (new experiment, quit).
    /// </summary>
    public partial class ExportEndExperimentWindowViewModel : ViewModelBase
    {
        private readonly ExperimentSettingsViewModel _settings;
        private readonly IExportationService _exportationService;
        private readonly Action _closeWindow;
        private readonly Action<bool?> _setDialogResult;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DefaultView))]
        [NotifyPropertyChangedFor(nameof(CanReExport))]
        private bool _isExporting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DefaultView))]
        [NotifyPropertyChangedFor(nameof(CanReExport))]
        private bool _exportSuccess;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DefaultView))]
        [NotifyPropertyChangedFor(nameof(CanReExport))]
        private bool _exportError;

        public bool DefaultView => !IsExporting && !ExportSuccess && !ExportError;

        public Visibility CanReExport => ExportSuccess ? Visibility.Visible : Visibility.Collapsed;

        [ObservableProperty]
        private string _exportPath = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public ExportFolderSelectorViewModel ExportFolderSelectorViewModel { get; }

        public ExportEndExperimentWindowViewModel(ExperimentSettingsViewModel settings, IExportationService exportationService, Action closeWindow, Action<bool?> setDialogResult, INavigationService navigationService)
        {
            _settings = settings;
            _exportationService = exportationService;
            _closeWindow = closeWindow;
            _setDialogResult = setDialogResult;
            _navigationService = navigationService;

            ExportFolderSelectorViewModel = new ExportFolderSelectorViewModel(settings, exportationService);
            ExportPath = _exportationService.LoadExportFolderPath();
            ResetState();
            _settings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ExperimentSettingsViewModel.ExportFolderPath))
                    ExportPath = _settings.ExportFolderPath;
            };
        }

        partial void OnExportPathChanged(string value)
        {
            ResetState();
            ExportCommand.NotifyCanExecuteChanged();
            RetryCommand.NotifyCanExecuteChanged();
        }

        private void ResetState()
        {
            IsExporting = false;
            ExportSuccess = false;
            ExportError = false;
            ErrorMessage = string.Empty;
        }

        private bool CanExport() => !string.IsNullOrWhiteSpace(ExportPath) && !IsExporting;

        [RelayCommand(CanExecute = nameof(CanExport))]
        private async Task ExportAsync()
        {
            ResetState();
            IsExporting = true;

            try
            {
                await _exportationService.ExportDataAsync();

                IsExporting = false;
                ExportSuccess = true;
                _settings.ExperimentContext.HasUnsavedExports = false;
            }
            catch (Exception ex)
            {
                IsExporting = false;
                ExportError = true;
                ErrorMessage = $"{Strings.Error_ExportFailed}\n{ex.Message}";
            }
            finally
            {
                ExportCommand.NotifyCanExecuteChanged();
                RetryCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            IsExporting = false;
            _closeWindow();
        }

        [RelayCommand(CanExecute = nameof(CanExport))]
        private async Task RetryAsync()
        {
            await ExportAsync();
        }

        [RelayCommand]
        private void ReExport()
        {
            ResetState();
        }

        [RelayCommand]
        private async Task OpenAndSelectTodayExportFileAsync()
        {
            try
            {
                ExportPath = _exportationService.LoadExportFolderPath();

                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string participantId = _settings.Participant.Id.ToString();
                string folderPath = Path.Combine(ExportPath, "Results", participantId, today);

                var files = Directory.Exists(folderPath) ? Directory.GetFiles(folderPath, "*.xlsx") : Array.Empty<string>();
                string filePath = files.OrderByDescending(File.GetCreationTime).FirstOrDefault();

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
                else if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", $"\"{folderPath}\"");
                }
                else
                {
                    await ShowErrorDialogAsync(Strings.Error_FileOrFolderNotFound);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(string.Format(Strings.Error_OpeningFile, ex.Message));
            }
        }

        [RelayCommand]
        private async Task NewExperimentAsync()
        {
            try
            {
                _settings.Reset();
                _setDialogResult(true);
                _closeWindow();
                NavigateToConfigurationPage();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync(string.Format(Strings.Error_CreatingNewExperiment, ex.Message));
            }
        }

        [RelayCommand]
        private async Task QuitAsync()
        {
            try
            {
                if (_settings.ExperimentContext.HasUnsavedExports)
                {
                    if (await ShowConfirmationDialogAsync(
                        Strings.Title_ConfirmShutDown,
                        Strings.Message_ConfirmExitWithoutExport))
                    {
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        private void NavigateToConfigurationPage()
        {
            if (_settings.CurrentProfile.TaskType == TaskType.Stroop)
                _navigationService.NavigateTo<ConfigurationPage>();
            else if (_settings.CurrentProfile.TaskType == TaskType.Simon)
                _navigationService.NavigateTo<SimonConfigurationPage>();
        }
    }
}