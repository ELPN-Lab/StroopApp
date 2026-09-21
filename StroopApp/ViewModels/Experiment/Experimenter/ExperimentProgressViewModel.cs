using StroopApp.Core;
using StroopApp.Models;
using StroopApp.ViewModels.State;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace StroopApp.ViewModels.Experiment.Experimenter
{
    /// <summary>
    /// ViewModel for tracking experiment progress, displaying completion percentage and current trial records.
    /// Updates automatically when experiment context changes.
    /// </summary>
    public class ExperimentProgressViewModel : ViewModelBase, IDisposable
    {
        public ExperimentSettingsViewModel Settings { get; }

        public int Progress => (Settings.CurrentProfile?.WordCount > 0)
            ? (int)(((double)Settings.ExperimentContext.ReactionPoints.Count / Settings.CurrentProfile.WordCount) * 100)
            : 0;

        public ObservableCollection<ITrial?> TrialRecords =>
            Settings.ExperimentContext.Blocks.Count > Settings.Block
                ? Settings.ExperimentContext.Blocks[Settings.Block].TrialRecords
                : new ObservableCollection<ITrial?>();

        private ObservableCollection<ITrial?>? _hookedTrialRecords;

        public ExperimentProgressViewModel(ExperimentSettingsViewModel settings)
        {
            Settings = settings;

            Settings.PropertyChanged += Settings_PropertyChanged;
            Settings.ExperimentContext.ReactionPoints.CollectionChanged += ReactionPoints_CollectionChanged;
            Settings.ExperimentContext.Blocks.CollectionChanged += Blocks_CollectionChanged;
            HookCurrentBlockTrialRecords();
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.Block) || e.PropertyName == nameof(Settings.CurrentProfile))
            {
                HookCurrentBlockTrialRecords();
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(TrialRecords));
            }
        }

        private void ReactionPoints_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(Progress));

        private void Blocks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            HookCurrentBlockTrialRecords();
            OnPropertyChanged(nameof(TrialRecords));
        }

        private void CurrentBlockTrialRecords_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(TrialRecords));

        private void HookCurrentBlockTrialRecords()
        {
            if (_hookedTrialRecords != null)
                _hookedTrialRecords.CollectionChanged -= CurrentBlockTrialRecords_CollectionChanged;

            if (Settings.ExperimentContext.Blocks.Count > Settings.Block)
            {
                _hookedTrialRecords = Settings.ExperimentContext.Blocks[Settings.Block].TrialRecords;
                _hookedTrialRecords.CollectionChanged += CurrentBlockTrialRecords_CollectionChanged;
            }
            else
            {
                _hookedTrialRecords = null;
            }
        }

        public void Dispose()
        {
            Settings.PropertyChanged -= Settings_PropertyChanged;
            Settings.ExperimentContext.ReactionPoints.CollectionChanged -= ReactionPoints_CollectionChanged;
            Settings.ExperimentContext.Blocks.CollectionChanged -= Blocks_CollectionChanged;

            if (_hookedTrialRecords != null)
                _hookedTrialRecords.CollectionChanged -= CurrentBlockTrialRecords_CollectionChanged;
        }
    }
}