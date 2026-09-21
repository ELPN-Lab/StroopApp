using StroopApp.ViewModels.Experiment.Participant.Instructions;
using System.Windows.Controls;
using System.Windows.Input;

namespace StroopApp.Views.Experiment.Participant
{
    public partial class InstructionsPage : Page
    {
        private readonly InstructionsViewModelBase _viewModel;
        public InstructionsPage(InstructionsViewModelBase viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.InstructionChanged += (s, e) => InstructionContentControl.Content = _viewModel.CurrentInstruction;
            Loaded += (s, e) =>
            {
                InstructionContentControl.Content = _viewModel.CurrentInstruction;
                Keyboard.Focus(this);
            };
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                _viewModel.NextCommand.Execute(null);
        }
    }
}
