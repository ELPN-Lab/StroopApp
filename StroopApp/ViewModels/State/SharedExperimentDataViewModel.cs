using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Services.Charts;
using System.Collections.ObjectModel;

namespace StroopApp.ViewModels.State
{
    public partial class SharedExperimentDataViewModel : ViewModelBase
    {
        private readonly SharedExperimentData _model;
        private readonly ExperimentChartFactory _chartFactory;
        private readonly SKColor[] _palette = { SKColors.CornflowerBlue, SKColors.OrangeRed, SKColors.MediumSeaGreen, SKColors.Goldenrod };

        public ObservableCollection<Block> Blocks => _model.Blocks;
        public ObservableCollection<ReactionTimePoint> ReactionPoints => _model.ReactionPoints;
        public ObservableCollection<ISeries> BlockSeries => _model.BlockSeries;
        public ObservableCollection<RectangularSection> Sections => _model.Sections;

        public ObservableCollection<ISeries>? ColumnSerie
        {
            get => _model.ColumnSerie;
            set
            {
                if (!ReferenceEquals(_model.ColumnSerie, value))
                {
                    _model.ColumnSerie = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty] private Block? _currentBlock;
        [ObservableProperty] private ITrial? _currentTrial;
        [ObservableProperty] private bool _isBlockFinished;
        [ObservableProperty] private bool _isTaskStopped;
        [ObservableProperty] private bool _isParticipantSelectionEnabled;
        [ObservableProperty] private bool _hasUnsavedExports;

        public SharedExperimentDataViewModel(SharedExperimentData model)
        {
            _model = model;
            _chartFactory = new ExperimentChartFactory();

            _currentBlock = model.CurrentBlock;
            _currentTrial = model.CurrentTrial;
            _isBlockFinished = model.IsBlockFinished;
            _isTaskStopped = model.IsTaskStopped;
            _isParticipantSelectionEnabled = model.IsParticipantSelectionEnabled;
            _hasUnsavedExports = model.HasUnsavedExports;

            if (_model.ColumnSerie == null || _model.ColumnSerie.Count == 0)
                NewColumnSerie();
        }

        partial void OnCurrentBlockChanged(Block? value) => _model.CurrentBlock = value;
        partial void OnCurrentTrialChanged(ITrial? value) => _model.CurrentTrial = value;
        partial void OnIsBlockFinishedChanged(bool value) => _model.IsBlockFinished = value;
        partial void OnIsTaskStoppedChanged(bool value) => _model.IsTaskStopped = value;
        partial void OnIsParticipantSelectionEnabledChanged(bool value) => _model.IsParticipantSelectionEnabled = value;
        partial void OnHasUnsavedExportsChanged(bool value) => _model.HasUnsavedExports = value;

        public void NewColumnSerie()
        {
            ColumnSerie = _chartFactory.CreateLiveColumnSerie(ReactionPoints);
            OnPropertyChanged(nameof(ColumnSerie));
        }

        public void AddNewSerie(ExperimentSettingsViewModel settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            CurrentBlock = new Block(settings.CurrentProfile.CloneProfile(), settings.Block);

            Blocks.Add(CurrentBlock);

            var color = _palette[_model.ColorIndex % _palette.Length];
            var fillColor = color.WithAlpha(50);

            var start = _model.CurrentBlockStart;
            var count = settings.CurrentProfile.WordCount;
            var end = start + count - 1;
            _model.CurrentBlockEnd = end;

            var lineSeries = _chartFactory.CreateBlockLineSeries(CurrentBlock.TrialTimes, start);
            BlockSeries.Add(lineSeries);

            var section = _chartFactory.CreateBlockSection(start, end, settings.Block, fillColor);
            Sections.Add(section);

            _model.ColorIndex++;
            _model.CurrentBlockStart = end + 1;

            OnPropertyChanged(nameof(Blocks));
            OnPropertyChanged(nameof(BlockSeries));
            OnPropertyChanged(nameof(Sections));
        }

        public void Reset()
        {
            _model.Reset();
            NewColumnSerie();
            RefreshFromModel();

            OnPropertyChanged(nameof(Blocks));
            OnPropertyChanged(nameof(BlockSeries));
            OnPropertyChanged(nameof(Sections));
            OnPropertyChanged(nameof(ReactionPoints));
            OnPropertyChanged(nameof(ColumnSerie));
        }

        public void RefreshFromModel()
        {
            CurrentBlock = _model.CurrentBlock;
            CurrentTrial = _model.CurrentTrial;
            IsBlockFinished = _model.IsBlockFinished;
            IsTaskStopped = _model.IsTaskStopped;
            IsParticipantSelectionEnabled = _model.IsParticipantSelectionEnabled;
            HasUnsavedExports = _model.HasUnsavedExports;
        }
    }
}