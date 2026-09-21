using CommunityToolkit.Mvvm.ComponentModel;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.Services.Profile;
using System.Collections.ObjectModel;

namespace StroopApp.ViewModels.Configuration.Profile
{
    /// <summary>
    /// ViewModel for creating or editing an experiment profile in a modal dialog.
    /// Handles validation, derived value recalculation, and language selection.
    /// </summary>
    public partial class StroopProfileEditorViewModel : ProfileEditorViewModelBase
    {

        public SwitchSettingsViewModel SwitchSettingsViewModel { get; }


        // Now using ObervsableProperty for automatic INotifyPropertyChanged implementation, instead of manually
        // implementing properties and calling OnPropertyChanged. This reduces boilerplate and potential for errors.
        // N.B : Migration to .NET 10 will allow even more concise code.

        [ObservableProperty]
        private int _visualCueDuration;
        partial void OnVisualCueDurationChanged(int value) => UpdateDerivedValues();

        [ObservableProperty]
        private bool _hasVisualCue;
        partial void OnHasVisualCueChanged(bool value)
        {
            if (!value)
            {
                VisualCueDuration = 0;
                DominantPercent = 50;
                SwitchPercent = null;
            }
            else
            {
                SwitchPercent = 50;
            }
            UpdateDerivedValues();
        }

        [ObservableProperty]
        private int _dominantPercent;

        [ObservableProperty]
        private int? _switchPercent;

        public StroopProfileEditorViewModel(ExperimentProfile profile, ObservableCollection<ExperimentProfile> profiles, IProfileService profileService) :
            base(profile, profiles, profileService)
        {
            SwitchSettingsViewModel = new SwitchSettingsViewModel();
        }

        protected override int ComputeStimulusDuration() => MaxReactionTime + FixationDuration + VisualCueDuration;
        protected override void InitializeTaskSpecificFromProfile()
        {
            HasVisualCue = Profile.HasVisualCue;
            VisualCueDuration = Profile.VisualCueDuration;
            DominantPercent = Profile.DominantPercent;
            SwitchPercent = Profile.SwitchPercent;
        }
        protected override void SyncTaskSpecificToProfile()
        {
            Profile.HasVisualCue = HasVisualCue;
            Profile.VisualCueDuration = VisualCueDuration;
            Profile.DominantPercent = DominantPercent;
            Profile.SwitchPercent = SwitchPercent;
        }

        protected override async Task<bool> ValidateAsync()
        {
            if (HasVisualCue && VisualCueDuration == 0)
            {
                await ShowErrorDialogAsync(Strings.Error_VisualCueDurationInvalid);
                return false;
            }
            return true;
        }
    }
}