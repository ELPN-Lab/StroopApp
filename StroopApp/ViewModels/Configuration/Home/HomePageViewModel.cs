using CommunityToolkit.Mvvm.Input;
using StroopApp.Core;
using StroopApp.Services.Navigation;
using StroopApp.Views;
using StroopApp.Views.Configuration;
    
namespace StroopApp.ViewModels.Configuration.Home
{
    public partial class HomePageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        public HomePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateToStroop()
        {
            _navigationService.NavigateTo<ConfigurationPage>();
        }

        [RelayCommand]
        private void NavigateToSimon()
        {
            _navigationService.NavigateTo<SimonConfigurationPage>();
        }
    }
}
