using StroopApp.ViewModels.Configuration.Profile;
using System.ComponentModel;
using System.Windows;

namespace StroopApp.Views.Configuration.Profile
{
    public partial class StroopProfileEditorWindow : Window
    {
        public StroopProfileEditorWindow(StroopProfileEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            var switchVM = viewModel.SwitchSettingsViewModel;

            switchVM.DominantPercent = viewModel.DominantPercent;
            switchVM.SwitchPercent = viewModel.SwitchPercent;

            PropertyChangedEventHandler switchChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(switchVM.DominantPercent))
                    viewModel.DominantPercent = switchVM.DominantPercent;

                if (e.PropertyName == nameof(switchVM.SwitchPercent))
                    viewModel.SwitchPercent = switchVM.SwitchPercent;
            };

            PropertyChangedEventHandler editorChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.DominantPercent))
                    switchVM.DominantPercent = viewModel.DominantPercent;

                if (e.PropertyName == nameof(viewModel.SwitchPercent))
                    switchVM.SwitchPercent = viewModel.SwitchPercent;
            };

            switchVM.PropertyChanged += switchChangedHandler;
            viewModel.PropertyChanged += editorChangedHandler;

            Closed += (_, _) =>
            {
                switchVM.PropertyChanged -= switchChangedHandler;
                viewModel.PropertyChanged -= editorChangedHandler;
            };

            viewModel.CloseAction = () =>
            {
                DialogResult = viewModel.DialogResult;
                Close();
            };
        }
    }
}