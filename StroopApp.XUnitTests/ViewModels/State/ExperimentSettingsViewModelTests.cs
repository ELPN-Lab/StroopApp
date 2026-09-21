using StroopApp.Models;
using StroopApp.ViewModels.State;
using Xunit;

namespace StroopApp.ViewModels.State.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref="ExperimentSettingsViewModel"/>.
    /// </summary>
    public partial class ExperimentSettingsViewModelTests
    {
        /// <summary>
        /// Tests that Reset method refreshes the ExperimentContext from the model.
        /// </summary>
        [Fact]
        public void Reset_RefreshesExperimentContextFromModel()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);

            // Modify ExperimentContext properties
            viewModel.ExperimentContext.IsBlockFinished = false;
            viewModel.ExperimentContext.IsParticipantSelectionEnabled = false;
            viewModel.ExperimentContext.HasUnsavedExports = false;

            // Act
            viewModel.Reset();

            // Assert
            Assert.True(viewModel.ExperimentContext.IsBlockFinished);
            Assert.True(viewModel.ExperimentContext.IsParticipantSelectionEnabled);
            Assert.True(viewModel.ExperimentContext.HasUnsavedExports);
        }

        /// <summary>
        /// Tests that Reset method with Block at minimum value resets to default.
        /// </summary>
        [Fact]
        public void Reset_WithBlockAtMinValue_ResetsToDefault()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);
            viewModel.Block = int.MinValue;

            // Act
            viewModel.Reset();

            // Assert
            Assert.Equal(1, viewModel.Block);
        }

        /// <summary>
        /// Tests that Reset method with Block at maximum value resets to default.
        /// </summary>
        [Fact]
        public void Reset_WithBlockAtMaxValue_ResetsToDefault()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);
            viewModel.Block = int.MaxValue;

            // Act
            viewModel.Reset();

            // Assert
            Assert.Equal(1, viewModel.Block);
        }

        /// <summary>
        /// Tests that Reset method with Block at zero resets to default.
        /// </summary>
        [Fact]
        public void Reset_WithBlockAtZero_ResetsToDefault()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);
            viewModel.Block = 0;

            // Act
            viewModel.Reset();

            // Assert
            Assert.Equal(1, viewModel.Block);
        }

        /// <summary>
        /// Tests that Reset method with empty ExportFolderPath resets properly.
        /// </summary>
        [Fact]
        public void Reset_WithEmptyExportFolderPath_ResetsToModelValue()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);
            viewModel.ExportFolderPath = string.Empty;

            // Act
            viewModel.Reset();

            // Assert
            Assert.Equal(model.ExportFolderPath, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that Reset method with very long ExportFolderPath resets properly.
        /// </summary>
        [Fact]
        public void Reset_WithVeryLongExportFolderPath_ResetsToModelValue()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);
            viewModel.ExportFolderPath = new string('x', 10000);

            // Act
            viewModel.Reset();

            // Assert
            Assert.Equal(model.ExportFolderPath, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that Reset method with whitespace ExportFolderPath resets properly.
        /// </summary>
        [Fact]
        public void Reset_WithWhitespaceExportFolderPath_ResetsToModelValue()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);
            viewModel.ExportFolderPath = "   ";

            // Act
            viewModel.Reset();

            // Assert
            Assert.Equal(model.ExportFolderPath, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that Reset method synchronizes all properties with the underlying model.
        /// </summary>
        [Fact]
        public void Reset_SynchronizesAllPropertiesWithModel()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);

            // Modify ViewModel properties
            viewModel.Block = 42;
            viewModel.Participant = new Participant { Id = "P123" };
            viewModel.CurrentProfile = new StroopProfile { ProfileName = "Profile1" };
            viewModel.KeyMappings = new ExperimentKeyMappings();
            viewModel.ExportFolderPath = "C:\\Modified";

            // Act
            viewModel.Reset();

            // Assert - all properties should match model after reset
            Assert.Equal(model.Block, viewModel.Block);
            Assert.Equal(model.Participant, viewModel.Participant);
            Assert.Equal(model.CurrentProfile, viewModel.CurrentProfile);
            Assert.Equal(model.KeyMappings, viewModel.KeyMappings);
            Assert.Equal(model.ExportFolderPath, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that Reset method resets ExperimentContext state flags correctly.
        /// </summary>
        [Fact]
        public void Reset_ResetsExperimentContextStateFlags()
        {
            // Arrange
            var model = new ExperimentSettings();
            var viewModel = new ExperimentSettingsViewModel(model);

            // Modify all ExperimentContext state flags
            viewModel.ExperimentContext.IsBlockFinished = false;
            viewModel.ExperimentContext.IsTaskStopped = true;
            viewModel.ExperimentContext.IsParticipantSelectionEnabled = false;
            viewModel.ExperimentContext.HasUnsavedExports = false;

            // Act
            viewModel.Reset();

            // Assert - verify state flags are refreshed from model
            Assert.Equal(model.ExperimentContext.IsBlockFinished, viewModel.ExperimentContext.IsBlockFinished);
            Assert.Equal(model.ExperimentContext.IsTaskStopped, viewModel.ExperimentContext.IsTaskStopped);
            Assert.Equal(model.ExperimentContext.IsParticipantSelectionEnabled, viewModel.ExperimentContext.IsParticipantSelectionEnabled);
            Assert.Equal(model.ExperimentContext.HasUnsavedExports, viewModel.ExperimentContext.HasUnsavedExports);
        }

        /// <summary>
        /// Tests that the constructor initializes all properties correctly from the model.
        /// Input: Valid ExperimentSettings model with all properties set.
        /// Expected: All ViewModel properties match the model values.
        /// </summary>
        [Fact]
        public void Constructor_InitializesAllPropertiesFromModel_PropertiesMatchModelValues()
        {
            // Arrange
            var participant = new Participant { Id = "P001" };
            var profile = new StroopProfile { ProfileName = "TestProfile" };
            var keyMappings = new ExperimentKeyMappings();
            var exportPath = "C:\\TestPath\\Export";
            var blockValue = 5;

            var model = new ExperimentSettings
            {
                Block = blockValue,
                Participant = participant,
                CurrentProfile = profile,
                KeyMappings = keyMappings,
                ExportFolderPath = exportPath
            };

            // Act
            var viewModel = new ExperimentSettingsViewModel(model);

            // Assert
            Assert.Equal(blockValue, viewModel.Block);
            Assert.Same(participant, viewModel.Participant);
            Assert.Same(profile, viewModel.CurrentProfile);
            Assert.Same(keyMappings, viewModel.KeyMappings);
            Assert.Equal(exportPath, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that the constructor creates a SharedExperimentDataViewModel wrapping the model's ExperimentContext.
        /// Input: Valid ExperimentSettings model.
        /// Expected: ExperimentContext property is not null.
        /// </summary>
        [Fact]
        public void Constructor_CreatesExperimentContextViewModel_NotNull()
        {
            // Arrange
            var model = new ExperimentSettings();

            // Act
            var viewModel = new ExperimentSettingsViewModel(model);

            // Assert
            Assert.NotNull(viewModel.ExperimentContext);
        }

        /// <summary>
        /// Tests that the constructor handles various Block values including boundary cases.
        /// Input: ExperimentSettings with different Block values (int.MinValue, int.MaxValue, 0, negative).
        /// Expected: ViewModel Block property matches the model Block value.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(1)]
        [InlineData(100)]
        public void Constructor_WithVariousBlockValues_InitializesCorrectly(int blockValue)
        {
            // Arrange
            var model = new ExperimentSettings
            {
                Block = blockValue
            };

            // Act
            var viewModel = new ExperimentSettingsViewModel(model);

            // Assert
            Assert.Equal(blockValue, viewModel.Block);
        }

        /// <summary>
        /// Tests that the constructor handles empty ExportFolderPath.
        /// Input: ExperimentSettings with empty string for ExportFolderPath.
        /// Expected: ViewModel ExportFolderPath is empty string.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyExportFolderPath_InitializesCorrectly()
        {
            // Arrange
            var model = new ExperimentSettings
            {
                ExportFolderPath = string.Empty
            };

            // Act
            var viewModel = new ExperimentSettingsViewModel(model);

            // Assert
            Assert.Equal(string.Empty, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that the constructor handles whitespace-only ExportFolderPath.
        /// Input: ExperimentSettings with whitespace string for ExportFolderPath.
        /// Expected: ViewModel ExportFolderPath is the whitespace string.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void Constructor_WithWhitespaceExportFolderPath_InitializesCorrectly(string whitespace)
        {
            // Arrange
            var model = new ExperimentSettings
            {
                ExportFolderPath = whitespace
            };

            // Act
            var viewModel = new ExperimentSettingsViewModel(model);

            // Assert
            Assert.Equal(whitespace, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that the constructor handles very long ExportFolderPath strings.
        /// Input: ExperimentSettings with very long string for ExportFolderPath.
        /// Expected: ViewModel ExportFolderPath matches the long string.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongExportFolderPath_InitializesCorrectly()
        {
            // Arrange
            var longPath = new string('A', 10000);
            var model = new ExperimentSettings
            {
                ExportFolderPath = longPath
            };

            // Act
            var viewModel = new ExperimentSettingsViewModel(model);

            // Assert
            Assert.Equal(longPath, viewModel.ExportFolderPath);
        }

        /// <summary>
        /// Tests that the constructor handles ExportFolderPath with special characters.
        /// Input: ExperimentSettings with special characters in ExportFolderPath.
        /// Expected: ViewModel ExportFolderPath matches the input with special characters.
        /// </summary>
        [Theory]
        [InlineData("C:\\Path\\With\\Backslashes")]
        [InlineData("C:/Path/With/Slashes")]
        [InlineData("\\\\NetworkPath\\Share")]
        [InlineData("Path with spaces")]
        [InlineData("Path_with-special.chars!@#$%")]
        public void Constructor_WithSpecialCharactersInExportFolderPath_InitializesCorrectly(string pathWithSpecialChars)
        {
            // Arrange
            var model = new ExperimentSettings
            {
                ExportFolderPath = pathWithSpecialChars
            };

            // Act
            var viewModel = new ExperimentSettingsViewModel(model);

            // Assert
            Assert.Equal(pathWithSpecialChars, viewModel.ExportFolderPath);
        }
    }
}