namespace StroopApp.XUnitTests.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using DocumentFormat.OpenXml.Wordprocessing;
    using StroopApp.Models;
    using Xunit;

    public class SharedExperimentDataTests
    {
        ExperimentSettings Settings = new ExperimentSettings
        {
            Participant = new Participant
            {
                Id = "42"
            },
            ExperimentContext = new SharedExperimentData(),
            CurrentProfile = new StroopProfile
            {
                HasVisualCue = true
            }
        };
        [Fact]
        public void Constructor_InitializesCollectionsAndDefaults()
        {
            // Arrange & Act
            var data = new SharedExperimentData();
            // Assert
            Assert.NotNull(data.Blocks);
            Assert.Empty(data.Blocks);
            Assert.NotNull(data.BlockSeries);
            Assert.Empty(data.BlockSeries);
            Assert.NotNull(data.Sections);
            Assert.Empty(data.Sections);
            Assert.NotNull(data.ReactionPoints);
            Assert.Empty(data.ReactionPoints);
            Assert.NotNull(data.ColumnSerie);
            Assert.True(data.CurrentBlockStart == 1);
        }

        [Fact]
        public void IsBlockFinished_SetTrueAndFalse_UpdatesValue()
        {
            // Arrange
            var data = new SharedExperimentData();
            // Act
            data.IsBlockFinished = true;
            // Assert
            Assert.True(data.IsBlockFinished);
            // Act
            data.IsBlockFinished = false;
            // Assert
            Assert.False(data.IsBlockFinished);
        }

        [Fact]
        public void IsParticipantSelectionEnabled_DefaultTrue_AndCanSet()
        {
            // Arrange
            var data = new SharedExperimentData();
            // Assert
            Assert.True(data.IsParticipantSelectionEnabled);
            // Act
            data.IsParticipantSelectionEnabled = false;
            // Assert
            Assert.False(data.IsParticipantSelectionEnabled);
        }

        [Fact]
        public void NextAction_SetValue_UpdatesValue()
        {
            // Arrange
            var data = new SharedExperimentData();
            // Act
            data.NextAction = ExperimentAction.Quit;
            // Assert
            Assert.Equal(ExperimentAction.Quit, data.NextAction);
        }

    }
}