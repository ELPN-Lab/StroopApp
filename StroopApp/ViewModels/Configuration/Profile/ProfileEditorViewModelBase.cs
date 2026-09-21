using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.Services.Profile;
using System.Collections.ObjectModel;

namespace StroopApp.ViewModels.Configuration.Profile
{
    public abstract partial class ProfileEditorViewModelBase : ViewModelBase
    {
        public List<LanguageOption> Languages { get; } = new()
        {
            new LanguageOption { Code = "fr", DisplayName = "Français" },
            new LanguageOption { Code = "en", DisplayName = "English" }
        };

        private readonly IProfileService _profileService;
        public ExperimentProfile Profile { get; private set; }
        public ExperimentProfile ModifiedProfile => Profile;
        public ObservableCollection<ExperimentProfile> Profiles { get; }
        public bool? DialogResult { get; private set; }
        public Action? CloseAction { get; set; }

        [ObservableProperty]
        protected string _profileName = string.Empty;

        [ObservableProperty]
        protected int _hours;
        partial void OnHoursChanged(int value) => UpdateDerivedValues();

        [ObservableProperty]
        protected int _minutes;
        partial void OnMinutesChanged(int value) => UpdateDerivedValues();

        [ObservableProperty]
        protected int _seconds;
        partial void OnSecondsChanged(int value) => UpdateDerivedValues();

        [ObservableProperty]
        protected int _stimulusDuration;
        partial void OnStimulusDurationChanged(int value) => UpdateDerivedValues();

        [ObservableProperty]
        protected int _fixationDuration;
        partial void OnFixationDurationChanged(int value) => UpdateDerivedValues();

        protected bool _isUpdating;

        [ObservableProperty]
        private int _groupSize;

        [ObservableProperty]
        private int _taskDuration;

        [ObservableProperty]
        private int _stimulusCount;
        partial void OnStimulusCountChanged(int value) => UpdateDerivedValues();

        [ObservableProperty]
        private int _maxReactionTime;
        partial void OnMaxReactionTimeChanged(int value) => UpdateDerivedValues();

        [ObservableProperty]
        private CalculationMode _calculationMode;
        partial void OnCalculationModeChanged(CalculationMode value) => UpdateDerivedValues();

        [ObservableProperty]
        private int _congruencePercent;

        [ObservableProperty]
        private string _taskLanguage = "fr";

        [ObservableProperty]
        private LanguageOption _selectedTaskLanguage;

        partial void OnSelectedTaskLanguageChanged(LanguageOption value)
        {
            if (value != null)
                TaskLanguage = value.Code;
        }

        public ProfileEditorViewModelBase(ExperimentProfile profile, ObservableCollection<ExperimentProfile> profiles, IProfileService profileService) 
        {
            Profiles = profiles;
            Profile = Profiles.Contains(profile) ? profile.CloneProfile() : profile;
            _profileService = profileService;
            InitializeFromProfile();
        }
        
        protected virtual int ComputeStimulusDuration() => MaxReactionTime + FixationDuration;
        protected virtual Task<bool> ValidateAsync() => Task.FromResult(true);
        protected virtual void InitializeTaskSpecificFromProfile() { }
        protected virtual void SyncTaskSpecificToProfile() { }

        protected virtual void UpdateDerivedValues()
        {
            if (_isUpdating) return;
            _isUpdating = true; // To disable auto update during init

            StimulusDuration = ComputeStimulusDuration();

            if (CalculationMode == CalculationMode.TaskDuration)
            {
                TaskDuration = ((Hours * 3600) + (Minutes * 60) + Seconds) * 1000;
                if (StimulusDuration > 0)
                {
                    StimulusCount = TaskDuration / StimulusDuration;
                }
            }
            else if (CalculationMode == CalculationMode.WordCount)
            {
                TaskDuration = StimulusCount * StimulusDuration;
                Hours = TaskDuration / 3600000;
                Minutes = (TaskDuration % 3600000) / 60000;
                Seconds = (TaskDuration % 60000) / 1000;
            }

            SyncToProfile();
            _isUpdating = false;
        }


        protected virtual void InitializeFromProfile()
        {
            _isUpdating = true; // Stop auto-update during the initialisation

            ProfileName = Profile.ProfileName;
            Hours = Profile.Hours;
            Minutes = Profile.Minutes;
            Seconds = Profile.Seconds;
            StimulusDuration = Profile.WordDuration;
            FixationDuration = Profile.FixationDuration;
            GroupSize = Profile.GroupSize;
            TaskDuration = Profile.TaskDuration;
            StimulusCount = Profile.WordCount;
            MaxReactionTime = Profile.MaxReactionTime;
            CalculationMode = Profile.CalculationMode;
            CongruencePercent = Profile.CongruencePercent;
            TaskLanguage = Profile.TaskLanguage;
            SelectedTaskLanguage = Languages.FirstOrDefault(l => l.Code == TaskLanguage) ?? Languages[0];
            InitializeTaskSpecificFromProfile();
            _isUpdating = false;
            UpdateDerivedValues();
        }

        protected virtual void SyncToProfile()
        {
            if (Profile == null) return;

            Profile.ProfileName = ProfileName;
            Profile.Hours = Hours;
            Profile.Minutes = Minutes;
            Profile.Seconds = Seconds;
            Profile.WordDuration = StimulusDuration;
            Profile.WordCount = StimulusCount;
            Profile.FixationDuration = FixationDuration;
            Profile.GroupSize = GroupSize;
            Profile.TaskDuration = TaskDuration;
            Profile.MaxReactionTime = MaxReactionTime;
            Profile.CalculationMode = CalculationMode;
            Profile.CongruencePercent = CongruencePercent;
            Profile.TaskLanguage = TaskLanguage;
            SyncTaskSpecificToProfile();
        }

        [RelayCommand]
        public async Task SaveAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProfileName))
                {
                    await ShowErrorDialogAsync(Strings.Error_ProfileNameEmpty);
                    return;
                }

                if (Profiles.Any(p => p.Id != Profile.Id && p.ProfileName == ProfileName))
                {
                    await ShowErrorDialogAsync(Strings.Error_ProfileNameExists);
                    return;
                }

                if (StimulusDuration > 0 && TaskDuration % StimulusDuration != 0)
                {
                    await ShowErrorDialogAsync(Strings.Error_TrialDurationNotDividingTaskDuration);
                    return;
                }

                int stimulusNumber = StimulusDuration > 0 ? TaskDuration / StimulusDuration : 0;
                if (GroupSize <= 0 || (stimulusNumber > 0 && stimulusNumber % GroupSize != 0))
                {
                    await ShowErrorDialogAsync(Strings.Error_GroupSizeInvalid);
                    return;
                }

                if (!await ValidateAsync()) return;

                if (MaxReactionTime <= 0)
                {
                    await ShowErrorDialogAsync(Strings.Error_MaxResponseTimeInvalid);
                    return;
                }

                SyncToProfile();

                LoadUpdatedProfiles();
                FindMatchedProfile();

                DialogResult = true;
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"{Strings.Error_Title}: {ex.Message}");
            }
        }

        private void LoadUpdatedProfiles() 
        {
            var updatedProfiles = _profileService.UpsertProfile(Profile);
            var taskSpecificProfiles = updatedProfiles.Where(p => p.TaskType == Profile.TaskType);
            Profiles.Clear();
            foreach (var profile in taskSpecificProfiles)
            {
                Profiles.Add(profile);
            }
        }

        private void FindMatchedProfile()
        {
            var matchedProfile = Profiles.FirstOrDefault(p => p.Id == Profile.Id);
            if (matchedProfile != null)
            {
                Profile = matchedProfile;
                InitializeFromProfile();
            }
        }


        [RelayCommand]
        public void Cancel()
        {
            DialogResult = false;
            CloseAction?.Invoke();
        }


    }
}
