using StroopApp.Core;
using StroopApp.Models;
using StroopApp.ViewModels.State;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace StroopApp.ViewModels.Experiment.Experimenter
{
    /// <summary>
    /// ViewModel for live reaction time display with automatic grouping and average calculation.
    /// Updates incrementally as new trials complete and recalculates when group size changes.
    /// </summary>
    public class LiveReactionTimeViewModel : ViewModelBase, IDisposable
    {
        private readonly ExperimentSettingsViewModel _settings;

        public ObservableCollection<ReactionTimePoint> ReactionPoints { get; }
        public ObservableCollection<ReactionGroup> GroupAverages { get; } = new();

        private int _lastCompletedGroupIndex = 0;

        public int GroupSize => _settings.CurrentProfile.GroupSize;

        public LiveReactionTimeViewModel(ExperimentSettingsViewModel settings)
        {
            _settings = settings;
            ReactionPoints = _settings.ExperimentContext.ReactionPoints;

            _settings.ExperimentContext.ReactionPoints.CollectionChanged += ReactionPoints_CollectionChanged;
            if (_settings.CurrentProfile is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += OnProfilePropertyChanged;
            }

            RecomputeAllGroupsAfterGroupSizeChange();
        }

        private void ReactionPoints_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                TryAddCompletedGroup();
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                GroupAverages.Clear();
                _lastCompletedGroupIndex = 0;
            }
        }

        private void TryAddCompletedGroup()
        {
            if (_settings?.CurrentProfile == null)
                return;

            int groupSize = _settings.CurrentProfile.GroupSize;
            if (groupSize <= 0)
                groupSize = 1;

            int total = ReactionPoints.Count;
            if (total == 0)
                return;
            if (total % groupSize != 0)
                return;

            int currentGroupIndex = total / groupSize;
            if (currentGroupIndex <= _lastCompletedGroupIndex)
                return;

            AddGroup(currentGroupIndex, groupSize);
        }

        private void AddGroup(int groupIndex, int groupSize)
        {
            int startTrial = (groupIndex - 1) * groupSize + 1;
            int endTrial = groupIndex * groupSize;

            var slice = ReactionPoints
                .Skip(startTrial - 1)
                .Take(groupSize)
                .ToList();

            double? average = ResponseTimeStatistics.MeanOfCorrectAnswers(
                slice.Select(p => (p.IsValidResponse, p.ReactionTime)));
            int correct = slice.Count(p => p.IsValidResponse == true);

            GroupAverages.Add(new ReactionGroup(startTrial, endTrial, average, correct, groupSize));
            _lastCompletedGroupIndex = groupIndex;
        }
        private void RecomputeAllGroupsAfterGroupSizeChange()
        {
            GroupAverages.Clear();
            _lastCompletedGroupIndex = 0;

            int groupSize = GroupSize <= 0 ? 1 : GroupSize;
            int total = ReactionPoints.Count;
            if (total == 0)
                return;

            int fullGroupCount = total / groupSize;
            for (int gi = 1; gi <= fullGroupCount; gi++)
                AddGroup(gi, groupSize);
        }
        private void OnProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExperimentProfile.GroupSize))
            {
                OnPropertyChanged(nameof(GroupSize));
                RecomputeAllGroupsAfterGroupSizeChange();
            }
        }
        public void Dispose()
        {
            _settings.ExperimentContext.ReactionPoints.CollectionChanged -= ReactionPoints_CollectionChanged;

            if (_settings.CurrentProfile is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged -= OnProfilePropertyChanged;
            }
        }
    }
}
