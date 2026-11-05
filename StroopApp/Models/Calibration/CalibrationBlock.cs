using StroopApp.Services.Calibration;

namespace StroopApp.Models.Calibration
{
    /// <summary>
    /// Block that runs the two calibration phases and estimates the presentation time reaching the target accuracy.
    /// </summary>
    public class CalibrationBlock : Block
    {
        private readonly double[] _grid;
        private readonly int[] _candidates;
        private readonly double[,] _probabilityTable;
        private readonly double[,] _entropyTable;
        private readonly List<double> _correctPhase1ReactionTimes = new();
        private double[]? _logWeights;
        private int _nextMaxReactionTime;

        public CalibrationBlock(ExperimentProfile profile, int blockNumber, CalibrationParameters parameters) : base(profile, blockNumber)
        {
            Parameters = parameters.Clone();
            _grid = CalibrationCalculator.CreateGrid(Parameters);
            _candidates = CalibrationCalculator.CreateCandidates(_grid);
            _probabilityTable = CalibrationCalculator.CreateProbabilityTable(_grid, _candidates, Parameters);
            _entropyTable = CalibrationCalculator.CreateEntropyTable(_probabilityTable);
            _nextMaxReactionTime = Parameters.StartingTime;
        }

        /// <summary>
        /// Copy of the parameters taken when the block was created.
        /// </summary>
        public CalibrationParameters Parameters { get; }

        public int CurrentPhase { get; private set; } = 1;

        public int Phase1TrialCount { get; private set; }

        public int Phase2TrialCount { get; private set; }

        public double? StartingPoint { get; private set; }

        /// <summary>
        /// Calibration outcome; null while the block runs.
        /// </summary>
        public CalibrationResult? Result { get; private set; }

        public override bool IsFinished => Result is not null;

        public override int GetNextMaxReactionTime() => _nextMaxReactionTime;

        public override void RecordTrial(ITrial trial)
        {
            if (IsFinished)
                return;

            if (CurrentPhase == 1)
                RecordPhase1Trial(trial);
            else
                RecordPhase2Trial(trial.IsValidResponse == true);
        }

        /// <summary>
        /// Ends the block on the experimenter's request and produces the result available so far.
        /// </summary>
        public void StopByExperimenter()
        {
            if (IsFinished)
                return;

            Result = CurrentPhase == 1
                ? new CalibrationResult
                {
                    Status = CalibrationStatus.EstimationImpossible,
                    StopReason = CalibrationStopReason.StoppedByExperimenter,
                    Phase1TrialCount = Phase1TrialCount,
                    Parameters = Parameters
                }
                : CreateEstimateResult(CalibrationStopReason.StoppedByExperimenter);
        }

        /// <summary>
        /// Removes the generated trials that were never played.
        /// </summary>
        public void RemoveUnplayedTrials()
        {
            int playedTrialCount = Phase1TrialCount + Phase2TrialCount;
            while (TrialRecords.Count > playedTrialCount)
                TrialRecords.RemoveAt(TrialRecords.Count - 1);
        }

        private void RecordPhase1Trial(ITrial trial)
        {
            Phase1TrialCount++;
            if (trial.IsValidResponse == true && trial.ReactionTime.HasValue)
                _correctPhase1ReactionTimes.Add(trial.ReactionTime.Value);

            if (Phase1TrialCount < Parameters.Phase1TrialCount)
                return;

            StartingPoint = CalibrationCalculator.ComputeStartingPoint(_correctPhase1ReactionTimes, Parameters);
            _logWeights = CalibrationCalculator.CreatePrior(_grid, StartingPoint.Value, Parameters);
            CurrentPhase = 2;
            _nextMaxReactionTime = SelectNextMaxReactionTime();
        }

        private void RecordPhase2Trial(bool isCorrect)
        {
            _logWeights = CalibrationCalculator.Update(_logWeights!, _grid, _nextMaxReactionTime, isCorrect, Parameters);
            Phase2TrialCount++;

            double r = ComputeR();
            if (!CalibrationCalculator.ShouldStop(Phase2TrialCount, r, Parameters))
            {
                _nextMaxReactionTime = SelectNextMaxReactionTime();
                return;
            }

            bool isConverged = Phase2TrialCount >= Parameters.MinPhase2TrialCount && r <= Parameters.ConvergedThreshold;
            Result = CreateEstimateResult(isConverged ? CalibrationStopReason.Converged : CalibrationStopReason.MaxTrialsReached);
        }

        private int SelectNextMaxReactionTime()
        {
            return CalibrationCalculator.SelectNextPresentationTime(_logWeights!, _candidates, _probabilityTable, _entropyTable);
        }

        private double ComputeR()
        {
            return CalibrationCalculator.ComputeR(
                CalibrationCalculator.Quantile(_logWeights!, _grid, 0.05),
                CalibrationCalculator.Quantile(_logWeights!, _grid, 0.5),
                CalibrationCalculator.Quantile(_logWeights!, _grid, 0.95));
        }

        private CalibrationResult CreateEstimateResult(CalibrationStopReason stopReason)
        {
            double t5 = CalibrationCalculator.Quantile(_logWeights!, _grid, 0.05);
            double t50 = CalibrationCalculator.Quantile(_logWeights!, _grid, 0.5);
            double t95 = CalibrationCalculator.Quantile(_logWeights!, _grid, 0.95);
            double r = CalibrationCalculator.ComputeR(t5, t50, t95);

            return new CalibrationResult
            {
                Status = CalibrationCalculator.GetStatus(r, Parameters),
                StopReason = stopReason,
                T5 = t5,
                T50 = t50,
                T95 = t95,
                R = r,
                Phase1TrialCount = Phase1TrialCount,
                Phase2TrialCount = Phase2TrialCount,
                StartingPoint = StartingPoint,
                Parameters = Parameters
            };
        }
    }
}
