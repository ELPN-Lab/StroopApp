using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Models.Calibration;
using StroopApp.Resources;
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

            var profile = settings.CurrentProfile.CloneProfile();
            CurrentBlock = profile is StroopProfile { IsCalibration: true } stroopProfile
                ? new CalibrationBlock(stroopProfile, settings.Block, stroopProfile.CalibrationParameters)
                : new Block(profile, settings.Block);

            Blocks.Add(CurrentBlock);

            var color = _palette[_model.ColorIndex % _palette.Length];
            var fillColor = color.WithAlpha(50);

            var start = _model.CurrentBlockStart;
            var count = settings.CurrentProfile.WordCount;
            var end = start + count - 1;
            _model.CurrentBlockEnd = end;

            var blockLabel = $"{Strings.Header_BlockNumber} {settings.Block}";
            if (CurrentBlock is CalibrationBlock calibrationBlock)
                AddCalibrationPhases(calibrationBlock, start, end, blockLabel, color);
            else
            {
                BlockSeries.Add(_chartFactory.CreateBlockLineSeries(CurrentBlock.TrialTimes, start));
                Sections.Add(_chartFactory.CreateBlockSection(start, end, blockLabel, fillColor));
            }

            _model.ColorIndex++;
            _model.CurrentBlockStart = end + 1;

            OnPropertyChanged(nameof(Blocks));
            OnPropertyChanged(nameof(BlockSeries));
            OnPropertyChanged(nameof(Sections));
        }

        /// <summary>
        /// Shortens the current block's chart section to the trials actually played, after an early end.
        /// </summary>
        public void TrimCurrentBlock(int playedTrialCount)
        {
            bool isCalibration = CurrentBlock is CalibrationBlock;
            var blockSection = Sections[^(isCalibration ? 3 : 1)];
            int end = (int)blockSection.Xi! + playedTrialCount - 1;

            blockSection.Xj = end;
            if (isCalibration)
                TrimCalibrationPhases(end);

            _model.CurrentBlockEnd = end;
            _model.CurrentBlockStart = end + 1;
        }

        /// <summary>
        /// Ends the current calibration block on the experimenter's request, with the result so far and without its unplayed trials.
        /// </summary>
        public void StopCalibrationBlock()
        {
            if (CurrentBlock is not CalibrationBlock calibrationBlock)
                return;

            calibrationBlock.StopByExperimenter();
            TrimCalibrationBlock(calibrationBlock);
        }

        /// <summary>
        /// Removes the unplayed trials of a calibration block that ended early, from the block and from its chart section.
        /// </summary>
        public void TrimCalibrationBlock(CalibrationBlock calibrationBlock)
        {
            calibrationBlock.RemoveUnplayedTrials();
            TrimCurrentBlock(calibrationBlock.TrialRecords.Count);
        }

        private void AddCalibrationPhases(CalibrationBlock calibrationBlock, int start, int end, string blockLabel, SKColor color)
        {
            int phase1LastIndex = calibrationBlock.Parameters.Phase1TrialCount - 1;
            double phaseChange = start + phase1LastIndex + 0.5;

            BlockSeries.Add(_chartFactory.CreateBlockLineSeries(calibrationBlock.TrialTimes, start, lastIndex: phase1LastIndex));
            BlockSeries.Add(_chartFactory.CreateBlockLineSeries(calibrationBlock.TrialTimes, start, firstIndex: phase1LastIndex + 1));
            Sections.Add(_chartFactory.CreateBlockSection(start, end, $"{blockLabel} · {string.Format(Strings.Label_ChartCalibrationPhase, 1)}", color.WithAlpha(50)));
            Sections.Add(_chartFactory.CreateBlockSection(phaseChange, end, string.Format(Strings.Label_ChartCalibrationPhase, 2), color.WithAlpha(70)));
            Sections.Add(_chartFactory.CreatePhaseSeparator(phaseChange));
        }

        private void TrimCalibrationPhases(int end)
        {
            var phase2Section = Sections[^2];
            if (phase2Section.Xi < end)
            {
                phase2Section.Xj = end;
                return;
            }

            Sections.RemoveAt(Sections.Count - 1);
            Sections.RemoveAt(Sections.Count - 1);
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