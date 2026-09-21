using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StroopApp.Core;
using StroopApp.Services.Navigation;
using StroopApp.ViewModels.State;
using System.Windows;
using System.Windows.Controls;

namespace StroopApp.ViewModels.Experiment.Participant.Instructions
{
    public abstract partial class InstructionsViewModelBase : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly Func<Page> _nextPageFactory;
        protected readonly ExperimentSettingsViewModel _settings;

        protected abstract int TotalPages { get; }
        protected abstract UIElement GenerateInstructionPage(int page);

        [ObservableProperty]
        private int _currentPageIndex;

        public UIElement CurrentInstruction { get; private set; } = null;
        public event EventHandler? InstructionChanged;

        protected InstructionsViewModelBase(
            ExperimentSettingsViewModel settings,
            INavigationService navigationService,
            Func<Page> nextPageFactory)
        {
            _settings = settings;
            _navigationService = navigationService;
            _nextPageFactory = nextPageFactory;
            CurrentInstruction = GenerateInstructionPage(0);
        }

        [RelayCommand]
        private void Next()
        {
            CurrentPageIndex++;
            if (CurrentPageIndex < TotalPages)
            {
                CurrentInstruction = GenerateInstructionPage(CurrentPageIndex);
                InstructionChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _navigationService.NavigateTo(_nextPageFactory);
            }

        }

    }
}
