using StroopApp.Services.Navigation;
using StroopApp.Services.Navigation.PageFactory;
using StroopApp.ViewModels.State;
using System.Windows;

namespace StroopApp.Views
{
    public partial class ParticipantWindow : Window
    {
        private readonly ExperimentSettingsViewModel _settings;
        private readonly INavigationService _participantNavigationService;
        private readonly Func<ExperimentSettingsViewModel, INavigationService, IDisposable> _viewModelFactory;

        public ParticipantWindow(
            ExperimentSettingsViewModel settings, 
            IPageFactory pageFactory, 
            Func<ExperimentSettingsViewModel, INavigationService, IDisposable> viewModelFactory)
        {
            InitializeComponent();
            _settings = settings;
            _viewModelFactory = viewModelFactory;
            var navigationService = new NavigationService(pageFactory);
            navigationService.SetFrame(ParticipantFrame);
            _participantNavigationService = navigationService;
            DataContext = viewModelFactory(settings, _participantNavigationService);
        }

        public void Reset()
        {
            if (DataContext is IDisposable oldViewModel)
            {
                oldViewModel.Dispose();
            }
            DataContext = _viewModelFactory(_settings, _participantNavigationService);
        }
    }

}
