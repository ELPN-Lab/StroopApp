using CommunityToolkit.Mvvm.Input;

using StroopApp.Core;
using StroopApp.Resources;
using StroopApp.Services.Language;
using StroopApp.Services.Navigation;
using StroopApp.Services.Session;
using StroopApp.Views;
using StroopApp.Views.Experiment.Experimenter;
using StroopApp.Views.Home;
using System.Windows;

namespace StroopApp.ViewModels
{
	/// <summary>
	/// Root ViewModel for the main experiment window.
	/// Manages language selection and initial navigation to configuration page.
	/// </summary>
  public partial class ExperimentWindowViewModel : ViewModelBase
	{
		public bool IsEnglishSelected => Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "en";
		public bool IsFrenchSelected => Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName == "fr";
		public bool IsNotOnHomePage => _navigationService.CurrentPageType != typeof(HomePage);
		private readonly ILanguageService _languageService;
		private readonly INavigationService _navigationService;
		private readonly IExperimentSessionService _sessionService;
		public ExperimentWindowViewModel(INavigationService experimentNavigationService, ILanguageService languageService, IExperimentSessionService experimentSessionService)
		{
			_languageService = languageService;
			experimentNavigationService.NavigateTo<HomePage>();
			//experimentNavigationService.NavigateTo<ConfigurationPage>();
			_navigationService = experimentNavigationService;
			_navigationService.Navigated += _ => ReturnHomePageCommand.NotifyCanExecuteChanged();
			_navigationService.Navigated += _ => OnPropertyChanged(nameof(IsNotOnHomePage));
			_sessionService = experimentSessionService;
		}

		[RelayCommand]
		private void ChangeLanguage(string lang)
		{
			if (Application.Current.Resources["Loc"] is LocalizedStrings loc)
				loc.ChangeCulture(lang);

			_languageService.SetLanguage(lang);

			OnPropertyChanged(nameof(IsEnglishSelected));
			OnPropertyChanged(nameof(IsFrenchSelected));
		}

		[RelayCommand (CanExecute = nameof(CanReturnHomePage))]
		private async Task ReturnHomePageAsync()
		{
            try
            {
				bool confirmed = true;
                if (_navigationService.IsCurrentPage<ExperimentDashBoardPage>())
					confirmed = await ShowConfirmationDialogAsync(Strings.Title_ConfirmStopTask, Strings.Message_StopTask);
				if(_navigationService.IsCurrentPage<EndExperimentPage>())
					confirmed = await ShowConfirmationDialogAsync(Strings.Title_ConfirmShutDown, Strings.Message_ConfirmExitWithoutExport);
                if (!confirmed)
                    return;
                _sessionService.ResetForNewExperiment();
                _navigationService.NavigateTo<HomePage>();


            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }



		private bool CanReturnHomePage() => _navigationService.CurrentPageType != typeof(HomePage);

	}
}
