using StroopApp.Models;
using StroopApp.Models.Calibration;
using StroopApp.Services.Calibration;
using Xunit;

namespace StroopApp.XUnitTests.Models.Calibration
{
    /// <summary>
    /// Unit tests for the CalibrationBlock state machine.
    /// </summary>
    public class CalibrationBlockTests
    {
        private static CalibrationBlock CreateBlock(CalibrationParameters? parameters = null)
        {
            return new CalibrationBlock(new StroopProfile(), 1, parameters ?? new CalibrationParameters());
        }

        private static StroopTrial Trial(bool? isCorrect, double? reactionTime = 500)
        {
            return new StroopTrial { IsValidResponse = isCorrect, ReactionTime = reactionTime };
        }

        private static void PlayPhase1(CalibrationBlock block, double reactionTime = 500)
        {
            for (int i = 0; i < block.Parameters.Phase1TrialCount; i++)
                block.RecordTrial(Trial(true, reactionTime));
        }

        private static int ExpectedSelection(double[] logWeights, CalibrationParameters parameters)
        {
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var candidates = CalibrationCalculator.CreateCandidates(grid);
            var probabilityTable = CalibrationCalculator.CreateProbabilityTable(grid, candidates, parameters);
            return CalibrationCalculator.SelectNextPresentationTime(logWeights, candidates, probabilityTable, CalibrationCalculator.CreateEntropyTable(probabilityTable));
        }

        /// <summary>
        /// Every phase 1 trial uses the starting time.
        /// </summary>
        [Fact]
        public void GetNextMaxReactionTime_Phase1_ReturnsStartingTime()
        {
            var block = CreateBlock(new CalibrationParameters { StartingTime = 3000 });

            for (int i = 0; i < 20; i++)
            {
                Assert.Equal(1, block.CurrentPhase);
                Assert.Equal(3000, block.GetNextMaxReactionTime());
                block.RecordTrial(Trial(true));
            }
        }

        /// <summary>
        /// After the last phase 1 trial, the block switches to phase 2 and uses the QUEST+ choice on the prior.
        /// </summary>
        [Fact]
        public void RecordTrial_LastPhase1Trial_SwitchesToPhase2WithQuestChoice()
        {
            var parameters = new CalibrationParameters();
            var block = CreateBlock(parameters);
            var reactionTimes = Enumerable.Range(0, 20).Select(i => 400.0 + 20 * i).ToList();

            foreach (double reactionTime in reactionTimes)
                block.RecordTrial(Trial(true, reactionTime));

            double expectedStart = CalibrationCalculator.ComputeStartingPoint(reactionTimes, parameters);
            var prior = CalibrationCalculator.CreatePrior(CalibrationCalculator.CreateGrid(parameters), expectedStart, parameters);
            Assert.Equal(2, block.CurrentPhase);
            Assert.Equal(20, block.Phase1TrialCount);
            Assert.Equal(expectedStart, block.StartingPoint);
            Assert.Equal(ExpectedSelection(prior, parameters), block.GetNextMaxReactionTime());
        }

        /// <summary>
        /// The starting point only uses correct phase 1 answers.
        /// </summary>
        [Fact]
        public void StartingPoint_ErrorsAndTimeouts_AreIgnored()
        {
            var parameters = new CalibrationParameters();
            var block = CreateBlock(parameters);
            var correctReactionTimes = new List<double> { 500, 550, 600, 650, 700, 750, 800, 850, 900, 950 };

            foreach (double reactionTime in correctReactionTimes)
                block.RecordTrial(Trial(true, reactionTime));
            for (int i = 0; i < 5; i++)
                block.RecordTrial(Trial(false, 300));
            for (int i = 0; i < 5; i++)
                block.RecordTrial(Trial(false, null));

            Assert.Equal(CalibrationCalculator.ComputeStartingPoint(correctReactionTimes, parameters), block.StartingPoint);
        }

        /// <summary>
        /// With fewer than 5 correct phase 1 answers, phase 2 starts from the starting time.
        /// </summary>
        [Fact]
        public void StartingPoint_TooFewCorrectAnswers_IsStartingTime()
        {
            var block = CreateBlock(new CalibrationParameters { StartingTime = 2500 });

            for (int i = 0; i < 4; i++)
                block.RecordTrial(Trial(true, 400));
            for (int i = 0; i < 16; i++)
                block.RecordTrial(Trial(false, 400));

            Assert.Equal(2500, block.StartingPoint);
        }

        /// <summary>
        /// A phase 2 trial updates the weights exactly like the calculator.
        /// </summary>
        [Fact]
        public void RecordTrial_Phase2Trial_FollowsCalculator()
        {
            var parameters = new CalibrationParameters();
            var block = CreateBlock(parameters);
            PlayPhase1(block, 500);
            var grid = CalibrationCalculator.CreateGrid(parameters);
            var prior = CalibrationCalculator.CreatePrior(grid, 500, parameters);
            int firstTime = ExpectedSelection(prior, parameters);
            var posterior = CalibrationCalculator.Update(prior, grid, firstTime, true, parameters);

            block.RecordTrial(Trial(true));

            Assert.Equal(1, block.Phase2TrialCount);
            Assert.Equal(ExpectedSelection(posterior, parameters), block.GetNextMaxReactionTime());
        }

        /// <summary>
        /// A timed-out phase 2 trial counts as an error.
        /// </summary>
        [Fact]
        public void RecordTrial_Phase2Timeout_CountsAsError()
        {
            var withError = CreateBlock();
            var withTimeout = CreateBlock();
            PlayPhase1(withError);
            PlayPhase1(withTimeout);

            withError.RecordTrial(Trial(false, 300));
            withTimeout.RecordTrial(Trial(null, null));

            Assert.Equal(withError.GetNextMaxReactionTime(), withTimeout.GetNextMaxReactionTime());
        }

        /// <summary>
        /// Once R reaches the converged threshold after the minimum number of trials, the block stops as converged.
        /// </summary>
        [Fact]
        public void RecordTrial_ConvergedAfterMinimum_StopsAsConverged()
        {
            var parameters = new CalibrationParameters { ConvergedThreshold = 0.5, NearlyConvergedThreshold = 0.6, MinPhase2TrialCount = 5 };
            var block = CreateBlock(parameters);
            PlayPhase1(block, 800);

            while (!block.IsFinished && block.Phase2TrialCount < 100)
                block.RecordTrial(Trial(block.GetNextMaxReactionTime() >= 800));

            Assert.True(block.IsFinished);
            Assert.Equal(CalibrationStopReason.Converged, block.Result!.StopReason);
            Assert.Equal(CalibrationStatus.Converged, block.Result.Status);
            Assert.True(block.Phase2TrialCount >= 5);
            Assert.True(block.Result.R <= 0.5);
        }

        /// <summary>
        /// At the maximum number of phase 2 trials, the block stops whatever R.
        /// </summary>
        [Fact]
        public void RecordTrial_MaximumReached_StopsAsMaxTrialsReached()
        {
            var parameters = new CalibrationParameters { ConvergedThreshold = 0.0001, MinPhase2TrialCount = 10, MaxPhase2TrialCount = 10 };
            var block = CreateBlock(parameters);
            PlayPhase1(block);

            for (int i = 0; i < 9; i++)
                block.RecordTrial(Trial(i % 2 == 0));
            Assert.False(block.IsFinished);

            block.RecordTrial(Trial(true));

            Assert.True(block.IsFinished);
            Assert.Equal(CalibrationStopReason.MaxTrialsReached, block.Result!.StopReason);
            Assert.Equal(10, block.Result.Phase2TrialCount);
            Assert.Equal(20, block.Result.Phase1TrialCount);
        }

        /// <summary>
        /// Stopping during phase 1 gives no estimate.
        /// </summary>
        [Fact]
        public void StopByExperimenter_DuringPhase1_EstimationImpossible()
        {
            var block = CreateBlock();
            for (int i = 0; i < 5; i++)
                block.RecordTrial(Trial(true));

            block.StopByExperimenter();

            Assert.True(block.IsFinished);
            Assert.Equal(CalibrationStatus.EstimationImpossible, block.Result!.Status);
            Assert.Equal(CalibrationStopReason.StoppedByExperimenter, block.Result.StopReason);
            Assert.Null(block.Result.T50);
            Assert.Null(block.Result.R);
            Assert.Null(block.Result.StartingPoint);
            Assert.Equal(5, block.Result.Phase1TrialCount);
        }

        /// <summary>
        /// Stopping during phase 2 gives the estimate available so far.
        /// </summary>
        [Fact]
        public void StopByExperimenter_DuringPhase2_GivesEstimate()
        {
            var block = CreateBlock();
            PlayPhase1(block);
            for (int i = 0; i < 3; i++)
                block.RecordTrial(Trial(true));

            block.StopByExperimenter();

            var result = block.Result!;
            Assert.True(block.IsFinished);
            Assert.NotEqual(CalibrationStatus.EstimationImpossible, result.Status);
            Assert.Equal(CalibrationStopReason.StoppedByExperimenter, result.StopReason);
            Assert.Equal(3, result.Phase2TrialCount);
            Assert.NotNull(result.T50);
            Assert.True(result.T5 <= result.T50 && result.T50 <= result.T95);
            Assert.Equal(CalibrationCalculator.GetStatus(result.R!.Value, block.Parameters), result.Status);
        }

        /// <summary>
        /// Trials recorded after the end are ignored.
        /// </summary>
        [Fact]
        public void RecordTrial_AfterFinished_IsIgnored()
        {
            var block = CreateBlock();
            block.RecordTrial(Trial(true));
            block.StopByExperimenter();

            block.RecordTrial(Trial(true));

            Assert.Equal(1, block.Phase1TrialCount);
        }

        /// <summary>
        /// Stopping a block that already ended keeps its result.
        /// </summary>
        [Fact]
        public void StopByExperimenter_AfterFinished_KeepsResult()
        {
            var block = CreateBlock(new CalibrationParameters { MinPhase2TrialCount = 1, MaxPhase2TrialCount = 1 });
            PlayPhase1(block);
            block.RecordTrial(Trial(true));

            block.StopByExperimenter();

            Assert.Equal(CalibrationStopReason.MaxTrialsReached, block.Result!.StopReason);
        }

        /// <summary>
        /// Generated trials that were never played are removed, played ones are kept in order.
        /// </summary>
        [Fact]
        public void RemoveUnplayedTrials_AfterEarlyStop_KeepsPlayedTrials()
        {
            var block = CreateBlock();
            for (int i = 1; i <= 120; i++)
                block.TrialRecords.Add(new StroopTrial { TrialNumber = i });
            PlayPhase1(block);
            for (int i = 0; i < 5; i++)
                block.RecordTrial(Trial(true));
            block.StopByExperimenter();

            block.RemoveUnplayedTrials();

            Assert.Equal(25, block.TrialRecords.Count);
            Assert.Equal(Enumerable.Range(1, 25), block.TrialRecords.Select(trial => trial!.TrialNumber));
        }

        /// <summary>
        /// The block keeps its own copy of the parameters.
        /// </summary>
        [Fact]
        public void Parameters_ModifiedAfterCreation_DoNotAffectBlock()
        {
            var parameters = new CalibrationParameters { StartingTime = 1500 };
            var block = CreateBlock(parameters);

            parameters.StartingTime = 4000;

            Assert.Equal(1500, block.GetNextMaxReactionTime());
            Assert.NotSame(parameters, block.Parameters);
        }
    }
}
