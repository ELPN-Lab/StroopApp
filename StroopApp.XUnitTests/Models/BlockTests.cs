using Xunit;


namespace StroopApp.Models.UnitTests
{
    /// <summary>
    /// Unit tests for the Block class.
    /// </summary>
    public partial class BlockTests
    {
        private Block CreateBlock()
        {
            return new Block(new StroopProfile { ProfileName = "TestProfile" }, blockNumber: 1);
        }

        /// <summary>
        /// Tests that CalculateValues correctly sets TrialsPerBlock, Accuracy to 0, and ResponseTimeMean to null when TrialRecords is empty.
        /// </summary>
        [Fact]
        public void CalculateValues_EmptyTrialRecords_SetsZeroValuesAndNullMean()
        {
            // Arrange
            var block = CreateBlock();

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(0, block.TrialsPerBlock);
            Assert.Equal(0, block.Accuracy);
            Assert.Null(block.ResponseTimeMean);
        }

        /// <summary>
        /// Tests that CalculateValues correctly calculates accuracy percentage based on valid responses.
        /// Input: Various combinations of valid and invalid trial responses.
        /// Expected: Correct accuracy percentage calculated.
        /// </summary>
        [Theory]
        [InlineData(1, 1, 100.0)]
        [InlineData(2, 2, 100.0)]
        [InlineData(2, 1, 50.0)]
        [InlineData(3, 2, 66.666666666666657)]
        [InlineData(4, 0, 0.0)]
        [InlineData(5, 3, 60.0)]
        [InlineData(10, 7, 70.0)]
        public void CalculateValues_WithVariousValidResponses_CalculatesCorrectAccuracy(int totalTrials, int validCount, double expectedAccuracy)
        {
            // Arrange
            var block = CreateBlock();

            for (int i = 0; i < totalTrials; i++)
            {
                var trial = new StroopTrial
                {
                    IsValidResponse = i < validCount,
                    Block = 1
                };
                block.TrialRecords.Add(trial);
            }

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(totalTrials, block.TrialsPerBlock);
            Assert.Equal(expectedAccuracy, block.Accuracy);
        }

        /// <summary>
        /// Tests that CalculateValues treats null IsValidResponse as invalid (not counted as valid).
        /// Input: Trials with null IsValidResponse values.
        /// Expected: Accuracy excludes null values from valid count.
        /// </summary>
        [Fact]
        public void CalculateValues_WithNullIsValidResponse_CountsAsInvalid()
        {
            // Arrange
            var block = CreateBlock();

            block.TrialRecords.Add(new StroopTrial { IsValidResponse = true, Block = 1 });
            block.TrialRecords.Add(new StroopTrial { IsValidResponse = null, Block = 1 });
            block.TrialRecords.Add(new StroopTrial { IsValidResponse = false, Block = 1 });
            block.TrialRecords.Add(new StroopTrial { IsValidResponse = null, Block = 1 });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(4, block.TrialsPerBlock);
            Assert.Equal(25.0, block.Accuracy);
        }

        /// <summary>
        /// Tests that CalculateValues correctly calculates mean response time from trials with valid ReactionTime values.
        /// Input: Trials with various ReactionTime values.
        /// Expected: Correct mean calculated from non-null ReactionTime values in matching block.
        /// </summary>
        [Theory]
        [InlineData(new double[] { 100.0 }, 100.0)]
        [InlineData(new double[] { 100.0, 200.0 }, 150.0)]
        [InlineData(new double[] { 50.0, 100.0, 150.0 }, 100.0)]
        [InlineData(new double[] { 0.0 }, 0.0)]
        [InlineData(new double[] { 1.5, 2.5, 3.5, 4.5 }, 3.0)]
        public void CalculateValues_WithReactionTimes_CalculatesCorrectMean(double[] reactionTimes, double expectedMean)
        {
            // Arrange
            var block = CreateBlock();

            foreach (var time in reactionTimes)
            {
                block.TrialRecords.Add(new StroopTrial
                {
                    ReactionTime = time,
                    Block = 1,
                    IsValidResponse = true
                });
            }

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(expectedMean, block.ResponseTimeMean);
        }

        /// <summary>
        /// Tests that CalculateValues excludes trials with null ReactionTime from mean calculation.
        /// Input: Mix of trials with null and non-null ReactionTime values.
        /// Expected: Mean calculated only from non-null values.
        /// </summary>
        [Fact]
        public void CalculateValues_WithNullReactionTimes_ExcludesFromMean()
        {
            // Arrange
            var block = CreateBlock();

            block.TrialRecords.Add(new StroopTrial { ReactionTime = 100.0, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = null, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = 200.0, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = null, Block = 1, IsValidResponse = false });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(4, block.TrialsPerBlock);
            Assert.Equal(150.0, block.ResponseTimeMean);
        }

        /// <summary>
        /// Tests that CalculateValues only includes trials matching the current BlockNumber in mean calculation.
        /// Input: Trials with different Block property values.
        /// Expected: Mean calculated only from trials where Block == BlockNumber.
        /// </summary>
        [Fact]
        public void CalculateValues_WithDifferentBlockNumbers_FiltersCorrectly()
        {
            // Arrange
            var block = CreateBlock();
            block.BlockNumber = 2;

            block.TrialRecords.Add(new StroopTrial { ReactionTime = 100.0, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = 200.0, Block = 2, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = 300.0, Block = 2, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = 400.0, Block = 3, IsValidResponse = false });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(4, block.TrialsPerBlock);
            Assert.Equal(250.0, block.ResponseTimeMean);
        }

        /// <summary>
        /// Tests that CalculateValues returns null ResponseTimeMean when no trials have valid ReactionTime for the current block.
        /// Input: Trials with all null ReactionTime or from different blocks.
        /// Expected: ResponseTimeMean is null.
        /// </summary>
        [Fact]
        public void CalculateValues_NoValidReactionTimesForBlock_SetsNullMean()
        {
            // Arrange
            var block = CreateBlock();

            block.TrialRecords.Add(new StroopTrial { ReactionTime = null, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = 100.0, Block = 2, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = null, Block = 1, IsValidResponse = false });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(3, block.TrialsPerBlock);
            Assert.Null(block.ResponseTimeMean);
        }

        /// <summary>
        /// Tests that CalculateValues handles special double values (NaN, Infinity) in ReactionTime.
        /// Input: Trials with double.NaN, double.PositiveInfinity, and double.NegativeInfinity.
        /// Expected: These values are included in mean calculation (as they are non-null).
        /// </summary>
        [Fact]
        public void CalculateValues_WithSpecialDoubleValues_IncludesInMean()
        {
            // Arrange
            var block = CreateBlock();

            block.TrialRecords.Add(new StroopTrial { ReactionTime = double.NaN, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = double.PositiveInfinity, Block = 1, IsValidResponse = true });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(2, block.TrialsPerBlock);
            Assert.True(double.IsNaN(block.ResponseTimeMean.Value));
        }

        /// <summary>
        /// Tests that CalculateValues handles negative ReactionTime values correctly.
        /// Input: Trials with negative ReactionTime values.
        /// Expected: Negative values are included in mean calculation.
        /// </summary>
        [Fact]
        public void CalculateValues_WithNegativeReactionTimes_IncludesInMean()
        {
            // Arrange
            var block = CreateBlock();

            block.TrialRecords.Add(new StroopTrial { ReactionTime = -100.0, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = 100.0, Block = 1, IsValidResponse = true });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(2, block.TrialsPerBlock);
            Assert.Equal(0.0, block.ResponseTimeMean);
        }

        /// <summary>
        /// Tests that CalculateValues handles extreme double values (MinValue, MaxValue) in ReactionTime.
        /// Input: Trials with double.MinValue and double.MaxValue.
        /// Expected: These values are included in mean calculation.
        /// </summary>
        [Fact]
        public void CalculateValues_WithExtremeDoubleValues_IncludesInMean()
        {
            // Arrange
            var block = CreateBlock();

            block.TrialRecords.Add(new StroopTrial { ReactionTime = double.MinValue, Block = 1, IsValidResponse = true });
            block.TrialRecords.Add(new StroopTrial { ReactionTime = double.MaxValue, Block = 1, IsValidResponse = true });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(2, block.TrialsPerBlock);
            Assert.NotNull(block.ResponseTimeMean);
        }

        /// <summary>
        /// Tests comprehensive scenario with mixed trial conditions.
        /// Input: Various trials with different IsValidResponse, ReactionTime, and Block values.
        /// Expected: All calculations correctly handle mixed conditions.
        /// </summary>
        [Fact]
        public void CalculateValues_MixedConditions_CalculatesAllValuesCorrectly()
        {
            // Arrange
            var block = CreateBlock();
            block.BlockNumber = 2;

            block.TrialRecords.Add(new StroopTrial { IsValidResponse = true, ReactionTime = 100.0, Block = 2 });
            block.TrialRecords.Add(new StroopTrial { IsValidResponse = false, ReactionTime = 200.0, Block = 2 });
            block.TrialRecords.Add(new StroopTrial { IsValidResponse = true, ReactionTime = null, Block = 2 });
            block.TrialRecords.Add(new StroopTrial { IsValidResponse = null, ReactionTime = 300.0, Block = 1 });
            block.TrialRecords.Add(new StroopTrial { IsValidResponse = true, ReactionTime = 400.0, Block = 2 });

            // Act
            block.CalculateValues();

            // Assert
            Assert.Equal(5, block.TrialsPerBlock);
            Assert.Equal(60.0, block.Accuracy);
            Assert.Equal(233.33333333333334, block.ResponseTimeMean);
        }
    }
}