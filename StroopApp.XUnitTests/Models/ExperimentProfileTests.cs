using System;

using StroopApp.Models;
using Xunit;

namespace StroopApp.XUnitTests.Models
{
    public class ExperimentProfileTests
    {
        [Fact]
        public void UpdateDerivedValues_IncludesVisualCueInWordDuration_WhenIsVisualCueTrue()
        {
            // Arrange
            var profile = new StroopProfile
            {
                CalculationMode = CalculationMode.WordCount,
                FixationDuration = 150,
                MaxReactionTime = 400,
                VisualCueDuration = 50,
                HasVisualCue = true,
                WordCount = 1
            };

            // Act
            profile.UpdateDerivedValues();

            // Assert
            Assert.Equal(600, profile.WordDuration);
        }

        [Fact]
        public void UpdateDerivedValues_ComputesWordCount_WhenTaskDurationMode()
        {
            // Arrange
            var profile = new StroopProfile
            {
                CalculationMode = CalculationMode.TaskDuration,
                Hours = 0,
                Minutes = 1,
                Seconds = 30,
                WordDuration = 500
            };

            // Act
            profile.UpdateDerivedValues();

            // Assert
            Assert.Equal(90_000, profile.TaskDuration);
            Assert.Equal(180, profile.WordCount);
        }

        [Fact]
        public void UpdateDerivedValues_ComputesTaskDurationAndTimeComponents_WhenWordCountMode()
        {
            // Arrange
            var profile = new StroopProfile
            {
                CalculationMode = CalculationMode.WordCount,
                WordCount = 20,
                FixationDuration = 100,
                MaxReactionTime = 150,
                VisualCueDuration = 0
            };

            // Act
            profile.UpdateDerivedValues();

            // Assert
            Assert.Equal(5_000, profile.TaskDuration);
            Assert.Equal(0, profile.Hours);
            Assert.Equal(0, profile.Minutes);
            Assert.Equal(5, profile.Seconds);
        }

        /// <summary>
        /// Tests that CloneProfile creates a new instance that is not the same reference as the original.
        /// </summary>
        [Fact]
        public void CloneProfile_CreatesNewInstance_NotSameReference()
        {
            // Arrange
            var original = new StroopProfile();

            // Act
            var cloned = original.CloneProfile();

            // Assert
            Assert.NotNull(cloned);
            Assert.NotSame(original, cloned);
        }

        /// <summary>
        /// Tests that CloneProfile correctly copies the Id property.
        /// </summary>
        /// <param name="guidValue">The Guid value to test.</param>
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
        [InlineData("12345678-1234-1234-1234-123456789012")]
        public void CloneProfile_CopiesIdCorrectly_WithVariousGuidValues(string guidValue)
        {
            // Arrange
            var original = new StroopProfile
            {
                Id = Guid.Parse(guidValue)
            };

            // Act
            var cloned = original.CloneProfile();

            // Assert
            Assert.Equal(original.Id, cloned.Id);
        }

        /// <summary>
        /// Tests that CloneProfile correctly copies the ProfileName property with various string values.
        /// </summary>
        /// <param name="profileName">The profile name to test.</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Simple Profile")]
        [InlineData("Profile with special chars: !@#$%^&*()")]
        [InlineData("Very long profile name that contains many characters to test how the clone method handles longer strings in the ProfileName property")]
        public void CloneProfile_CopiesProfileNameCorrectly_WithVariousStrings(string profileName)
        {
            // Arrange
            var original = new StroopProfile
            {
                ProfileName = profileName
            };

            // Act
            var cloned = original.CloneProfile();

            // Assert
            Assert.Equal(original.ProfileName, cloned.ProfileName);
        }

        /// <summary>
        /// Tests that CloneProfile correctly copies the CalculationMode property with both enum values.
        /// </summary>
        /// <param name="calculationMode">The calculation mode to test.</param>
        [Theory]
        [InlineData(CalculationMode.TaskDuration)]
        [InlineData(CalculationMode.WordCount)]
        public void CloneProfile_CopiesCalculationModeCorrectly_WithBothEnumValues(CalculationMode calculationMode)
        {
            // Arrange
            var original = new StroopProfile
            {
                CalculationMode = calculationMode
            };

            // Act
            var cloned = original.CloneProfile();

            // Assert
            Assert.Equal(original.CalculationMode, cloned.CalculationMode);
        }

        /// <summary>
        /// Tests that CloneProfile correctly copies Id, ProfileName, and CalculationMode together.
        /// Verifies that the first few properties initialized in lines 64-70 are all copied correctly.
        /// </summary>
        [Fact]
        public void CloneProfile_CopiesMultipleProperties_IdProfileNameAndCalculationMode()
        {
            // Arrange
            var testGuid = Guid.NewGuid();
            var original = new StroopProfile
            {
                Id = testGuid,
                ProfileName = "Test Profile Name",
                CalculationMode = CalculationMode.TaskDuration
            };

            // Act
            var cloned = original.CloneProfile();

            // Assert
            Assert.NotSame(original, cloned);
            Assert.Equal(testGuid, cloned.Id);
            Assert.Equal("Test Profile Name", cloned.ProfileName);
            Assert.Equal(CalculationMode.TaskDuration, cloned.CalculationMode);
        }
    }
}