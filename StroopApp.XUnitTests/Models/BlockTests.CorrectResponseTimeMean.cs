using Xunit;


namespace StroopApp.Models.UnitTests
{
    /// <summary>
    /// Tests of the correct-answers mean response time computed by Block.CalculateValues.
    /// </summary>
    public partial class BlockTests
    {
        /// <summary>
        /// In a block with wrong answers, the overall mean still averages every timed trial
        /// (same value as before the change) while the correct mean uses correct trials only.
        /// </summary>
        [Fact]
        public void CalculateValues_BlockWithWrongAnswers_KeepsOverallMeanAndAveragesCorrectOnly()
        {
            var block = CreateBlock();
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = true, ReactionTime = 400.0 });
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = false, ReactionTime = 800.0 });
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = true, ReactionTime = 600.0 });
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = false, ReactionTime = 600.0 });

            block.CalculateValues();

            Assert.Equal(600.0, block.ResponseTimeMean);
            Assert.Equal(500.0, block.CorrectResponseTimeMean);
        }

        /// <summary>
        /// A block with only correct answers gives the same value for both means.
        /// </summary>
        [Fact]
        public void CalculateValues_BlockWithOnlyCorrectAnswers_BothMeansAreEqual()
        {
            var block = CreateBlock();
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = true, ReactionTime = 300.0 });
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = true, ReactionTime = 500.0 });

            block.CalculateValues();

            Assert.Equal(400.0, block.ResponseTimeMean);
            Assert.Equal(400.0, block.CorrectResponseTimeMean);
        }

        /// <summary>
        /// A block without any correct answer has no correct mean, while the overall mean keeps its value.
        /// </summary>
        [Fact]
        public void CalculateValues_BlockWithoutCorrectAnswer_CorrectMeanIsNullAndOverallMeanIsKept()
        {
            var block = CreateBlock();
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = false, ReactionTime = 500.0 });
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = false, ReactionTime = 700.0 });

            block.CalculateValues();

            Assert.Null(block.CorrectResponseTimeMean);
            Assert.Equal(600.0, block.ResponseTimeMean);
        }

        /// <summary>
        /// An empty block has no correct mean.
        /// </summary>
        [Fact]
        public void CalculateValues_EmptyBlock_CorrectMeanIsNull()
        {
            var block = CreateBlock();

            block.CalculateValues();

            Assert.Null(block.CorrectResponseTimeMean);
        }

        /// <summary>
        /// Correct trials without a time, or recorded for another block, do not enter the correct mean.
        /// </summary>
        [Fact]
        public void CalculateValues_CorrectTrialsWithoutTimeOrFromOtherBlock_AreExcludedFromCorrectMean()
        {
            var block = CreateBlock();
            block.BlockNumber = 2;
            block.TrialRecords.Add(new StroopTrial { Block = 2, IsValidResponse = true, ReactionTime = 200.0 });
            block.TrialRecords.Add(new StroopTrial { Block = 2, IsValidResponse = true, ReactionTime = null });
            block.TrialRecords.Add(new StroopTrial { Block = 1, IsValidResponse = true, ReactionTime = 900.0 });
            block.TrialRecords.Add(new StroopTrial { Block = 2, IsValidResponse = false, ReactionTime = 1000.0 });

            block.CalculateValues();

            Assert.Equal(200.0, block.CorrectResponseTimeMean);
        }
    }
}
