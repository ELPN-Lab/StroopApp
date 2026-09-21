using StroopApp.Services.Navigation;
using StroopApp.ViewModels.Configuration.Home;
using System.Windows.Controls;

namespace StroopApp.Views.Home
{
    public partial class HomePage : Page, INavigationAware
    {

        public new INavigationService NavigationService
        {
            set => Initialize(value);
        }


        public HomePage()
        {
            InitializeComponent();
        }

        private void Initialize(INavigationService navigationService)
        {
            DataContext = new HomePageViewModel(navigationService);
        }
    }
}
