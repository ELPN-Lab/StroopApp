using StroopApp.ViewModels.Configuration.Profile;
using System.Windows;

namespace StroopApp.Views.Configuration.Profile
{
    /// <summary>
    /// Logique d'interaction pour SimonProfileEditorWindow.xaml
    /// </summary>
    public partial class SimonProfileEditorWindow : Window
    {
        public SimonProfileEditorWindow(SimonProfileEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseAction = () =>
            {
                DialogResult = viewModel.DialogResult;
                Close();
            };
        }
    }
}
