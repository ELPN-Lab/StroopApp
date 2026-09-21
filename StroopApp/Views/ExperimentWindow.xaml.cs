using StroopApp.Services.Language;
using StroopApp.Services.Navigation.PageFactory;
using StroopApp.Services.Session;
using StroopApp.ViewModels;
using System.Windows;
using System.Windows.Forms;

namespace StroopApp.Views
{
    public partial class ExperimentWindow : Window
    {
        public ExperimentWindow(IPageFactory pageFactory, ILanguageService languageService, IExperimentSessionService sessionService)
        {
            InitializeComponent();
            var navigationService = new NavigationService(pageFactory);
            navigationService.SetFrame(MainFrame);

            DataContext = new ExperimentWindowViewModel(navigationService, languageService, sessionService);
        }
    }
}
