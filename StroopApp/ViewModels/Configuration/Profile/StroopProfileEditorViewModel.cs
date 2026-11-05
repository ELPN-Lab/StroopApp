using CommunityToolkit.Mvvm.ComponentModel;
using StroopApp.Models;
using StroopApp.Models.Calibration;
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

        /// <summary>
        /// Target accuracy shown as a percentage; the parameters store it as a fraction.
        /// </summary>
        public double TargetAccuracyPercent
        {
            get => CalibrationParameters.TargetAccuracy * 100;
            set => SetProperty(CalibrationParameters.TargetAccuracy * 100, value, CalibrationParameters, (p, v) => p.TargetAccuracy = v / 100);
        }

        public int Phase1TrialCount
        {
            get => CalibrationParameters.Phase1TrialCount;
            set
            {
                if (SetProperty(CalibrationParameters.Phase1TrialCount, value, CalibrationParameters, (p, v) => p.Phase1TrialCount = v))
                    UpdateDerivedValues();
            }
        }

        public int MaxPhase2TrialCount
        {
            get => CalibrationParameters.MaxPhase2TrialCount;
            set
            {
                if (SetProperty(CalibrationParameters.MaxPhase2TrialCount, value, CalibrationParameters, (p, v) => p.MaxPhase2TrialCount = v))
                    UpdateDerivedValues();
            }
        }


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

        [ObservableProperty]
        private bool _isCalibration;
        partial void OnIsCalibrationChanged(bool value) => UpdateDerivedValues();

        /// <summary>
        /// Parameters of the edited profile clone, bound directly by the view.
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TargetAccuracyPercent), nameof(Phase1TrialCount), nameof(MaxPhase2TrialCount))]
        private CalibrationParameters _calibrationParameters = new();

        public StroopProfileEditorViewModel(ExperimentProfile profile, ObservableCollection<ExperimentProfile> profiles, IProfileService profileService) :
            base(profile, profiles, profileService)
        {
            SwitchSettingsViewModel = new SwitchSettingsViewModel();
        }

        protected override bool HasFixedTrialDuration => !IsCalibration;

        protected override int ComputeStimulusDuration() => MaxReactionTime + FixationDuration + VisualCueDuration;

        protected override void UpdateDerivedValues()
        {
            if (!_isUpdating && IsCalibration)
            {
                _isUpdating = true;
                CalculationMode = CalculationMode.WordCount;
                StimulusCount = CalibrationParameters.Phase1TrialCount + CalibrationParameters.MaxPhase2TrialCount;
                _isUpdating = false;
            }
            base.UpdateDerivedValues();
        }

        protected override void InitializeTaskSpecificFromProfile()
        {
            HasVisualCue = Profile.HasVisualCue;
            VisualCueDuration = Profile.VisualCueDuration;
            DominantPercent = Profile.DominantPercent;
            SwitchPercent = Profile.SwitchPercent;
            if (Profile is StroopProfile stroopProfile)
            {
                IsCalibration = stroopProfile.IsCalibration;
                CalibrationParameters = stroopProfile.CalibrationParameters;
            }
        }

        protected override void SyncTaskSpecificToProfile()
        {
            Profile.HasVisualCue = HasVisualCue;
            Profile.VisualCueDuration = VisualCueDuration;
            Profile.DominantPercent = DominantPercent;
            Profile.SwitchPercent = SwitchPercent;
            if (Profile is StroopProfile stroopProfile)
            {
                stroopProfile.IsCalibration = IsCalibration;
                stroopProfile.CalibrationParameters = CalibrationParameters;
            }
        }

        protected override async Task<bool> ValidateAsync()
        {
            if (HasVisualCue && VisualCueDuration == 0)
            {
                await ShowErrorDialogAsync(Strings.Error_VisualCueDurationInvalid);
                return false;
            }
            if (IsCalibration)
            {
                var errors = CalibrationParameters.GetValidationErrors();
                if (errors.Count > 0)
                {
                    await ShowErrorDialogAsync(string.Join(Environment.NewLine, errors.Select(e => Strings.ResourceManager.GetString($"Error_Calibration_{e}", Strings.Culture))));
                    return false;
                }
            }
            return true;
        }
    }
}
