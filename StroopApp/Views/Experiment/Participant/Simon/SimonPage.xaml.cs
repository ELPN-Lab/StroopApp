using StroopApp.Services.Navigation;
using StroopApp.ViewModels.Experiment.Participant.Simon;
using StroopApp.ViewModels.State;
using System.Windows.Controls;
using System.Windows.Input;

namespace StroopApp.Views.Experiment.Participant.Simon
{
    /// <summary>
    /// Logique d'interaction pour SimonPage.xaml
    /// </summary>
    public partial class SimonPage : Page
    {
        private readonly SimonViewModel _viewModel;
        public SimonPage(ExperimentSettingsViewModel settings, INavigationService participantWindowNavigationService)
        {
            InitializeComponent();
            _viewModel = new SimonViewModel(settings, participantWindowNavigationService);
            DataContext = _viewModel;
            Loaded += (s, e) => Keyboard.Focus(this);
            Unloaded += (s, e) => _viewModel.Dispose();
        }

        private void SimonPage_KeyDown(object sender, KeyEventArgs e)
        {
            _viewModel.ProcessInput(e.Key);
        }
    }
}
