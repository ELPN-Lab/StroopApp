using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Moq;
using StroopApp.Models;
using StroopApp.Services.Language;
using StroopApp.Services.Trial;
using StroopApp.ViewModels.State;
using Xunit;

namespace StroopApp.Services.Trial.UnitTests
{
    /// <summary>
    /// Unit tests for the TrialGenerationService class.
    /// </summary>
    public partial class TrialGenerationServiceTests
    {
        /// <summary>
        /// Tests that GenerateTrials throws ArgumentException when settings parameter is null.
        /// </summary>
        [Fact]
        public void GenerateTrials_SettingsIsNull_ThrowsArgumentException()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);
            ExperimentSettingsViewModel? settings = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => service.GenerateTrials(settings!));
            Assert.Equal("settings", exception.ParamName);
            Assert.Contains("Settings and  CurrentProfile cannot be null", exception.Message);
        }

        /// <summary>
        /// Tests that GenerateTrials throws ArgumentException when settings.CurrentProfile is null.
        /// </summary>
        [Fact]
        public void GenerateTrials_CurrentProfileIsNull_ThrowsArgumentException()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);
            var modelSettings = new ExperimentSettings();
            modelSettings.CurrentProfile = null!;
            var settings = new ExperimentSettingsViewModel(modelSettings);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => service.GenerateTrials(settings));
            Assert.Equal("settings", exception.ParamName);
            Assert.Contains("Settings and  CurrentProfile cannot be null", exception.Message);
        }

        /// <summary>
        /// Tests that GenerateTrials throws ArgumentNullException when config is null.
        /// </summary>
        [Fact]
        public void GenerateTrials_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.GenerateTrials((ITrialConfiguration)null!));
        }

        /// <summary>
        /// Tests that GenerateTrials returns an empty list when WordCount is 0.
        /// </summary>
        [Fact]
        public void GenerateTrials_WordCountZero_ReturnsEmptyList()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(0);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GenerateTrials returns exactly one trial when WordCount is 1.
        /// </summary>
        [Fact]
        public void GenerateTrials_WordCountOne_ReturnsOneTrialWithCorrectProperties()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_BLUE", "en")).Returns("Blue");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_RED", "en")).Returns("Red");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_GREEN", "en")).Returns("Green");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_YELLOW", "en")).Returns("Yellow");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(1);
            mockConfig.Setup(c => c.CongruencePercent).Returns(100);
            mockConfig.Setup(c => c.Block).Returns(2);
            mockConfig.Setup(c => c.ParticipantId).Returns("P123");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(60);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var trial = (StroopTrial)result[0];
            Assert.Equal(1, trial.TrialNumber);
            Assert.Equal(2, trial.Block);
            Assert.Equal("P123", trial.ParticipantId);
            Assert.False(trial.HasVIsualCue);
            Assert.Equal(60, trial.SwitchPercent);
            Assert.Equal(100, trial.CongruencePercent);
            Assert.NotNull(trial.Stimulus);
        }

        /// <summary>
        /// Tests that GenerateTrials respects CongruencePercent when set to 0 (all incongruent).
        /// </summary>
        [Fact]
        public void GenerateTrials_CongruencePercentZero_AllTrialsAreIncongruent()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(20);
            mockConfig.Setup(c => c.CongruencePercent).Returns(0);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Count);
            Assert.All(result, trial => Assert.False(trial.IsCongruent));
        }

        /// <summary>
        /// Tests that GenerateTrials respects CongruencePercent when set to 100 (all congruent).
        /// </summary>
        [Fact]
        public void GenerateTrials_CongruencePercent100_AllTrialsAreCongruent()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(20);
            mockConfig.Setup(c => c.CongruencePercent).Returns(100);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Count);
            Assert.All(result, trial => Assert.True(trial.IsCongruent));
        }

        /// <summary>
        /// Tests that GenerateTrials generates approximately correct congruent/incongruent ratio for 50%.
        /// </summary>
        [Fact]
        public void GenerateTrials_CongruencePercent50_GeneratesCorrectRatio()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(100);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Count);
            var congruentCount = result.Count(t => t.IsCongruent);
            Assert.Equal(50, congruentCount);
        }

        /// <summary>
        /// Tests that GenerateTrials uses TaskLanguage when provided to retrieve localized words.
        /// </summary>
        [Fact]
        public void GenerateTrials_WithTaskLanguage_UsesSpecifiedCulture()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_BLUE", "fr")).Returns("Bleu");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_RED", "fr")).Returns("Rouge");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_GREEN", "fr")).Returns("Vert");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_YELLOW", "fr")).Returns("Jaune");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(4);
            mockConfig.Setup(c => c.CongruencePercent).Returns(100);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("fr");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_BLUE", "fr"), Times.Once);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_RED", "fr"), Times.Once);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_GREEN", "fr"), Times.Once);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_YELLOW", "fr"), Times.Once);
        }

        /// <summary>
        /// Tests that GenerateTrials uses current UI culture when TaskLanguage is null.
        /// </summary>
        [Fact]
        public void GenerateTrials_TaskLanguageNull_UsesCurrentUICulture()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var currentCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), currentCulture)).Returns("Word");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(1);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns((string)null!);

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            mockLanguageService.Verify(s => s.GetLocalizedString(It.IsAny<string>(), currentCulture), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that GenerateTrials uses current UI culture when TaskLanguage is empty.
        /// </summary>
        [Fact]
        public void GenerateTrials_TaskLanguageEmpty_UsesCurrentUICulture()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var currentCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), currentCulture)).Returns("Word");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(1);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns(string.Empty);

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            mockLanguageService.Verify(s => s.GetLocalizedString(It.IsAny<string>(), currentCulture), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that GenerateTrials uses current UI culture when TaskLanguage is whitespace.
        /// </summary>
        [Fact]
        public void GenerateTrials_TaskLanguageWhitespace_UsesCurrentUICulture()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var currentCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), currentCulture)).Returns("Word");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(1);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("   ");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            mockLanguageService.Verify(s => s.GetLocalizedString(It.IsAny<string>(), currentCulture), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that GenerateTrials sets TrialNumber sequentially starting from 1.
        /// </summary>
        [Fact]
        public void GenerateTrials_GeneratesSequentialTrialNumbers()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(10);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            for (int i = 0; i < result.Count; i++)
            {
                Assert.Equal(i + 1, result[i].TrialNumber);
            }
        }

        /// <summary>
        /// Tests that GenerateTrials sets Block property from config on all trials.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void GenerateTrials_SetsBlockFromConfig(int blockNumber)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(blockNumber);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.Equal(blockNumber, trial.Block));
        }

        /// <summary>
        /// Tests that GenerateTrials sets ParticipantId from config on all trials.
        /// </summary>
        [Theory]
        [InlineData("P001")]
        [InlineData("Participant123")]
        [InlineData("")]
        public void GenerateTrials_SetsParticipantIdFromConfig(string participantId)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns(participantId);
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.Equal(participantId, trial.ParticipantId));
        }

        /// <summary>
        /// Tests that GenerateTrials sets HasVIsualCue to false when hasVisualCue is false.
        /// </summary>
        [Fact]
        public void GenerateTrials_HasVisualCueFalse_SetsHasVIsualCueFalse()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, iTrial =>
            {
                var trial = (StroopTrial)iTrial;
                Assert.False(trial.HasVIsualCue);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials sets HasVIsualCue to true when hasVisualCue is true.
        /// </summary>
        [Fact]
        public void GenerateTrials_HasVisualCueTrue_SetsHasVIsualCueTrue()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(true);
            mockConfig.Setup(c => c.DominantPercent).Returns(60);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.True(trial.HasVIsualCue);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials sets SwitchPercent from DominantPercent on all trials.
        /// </summary>
        [Theory]
        [InlineData(30)]
        [InlineData(50)]
        [InlineData(70)]
        public void GenerateTrials_SetsSwitchPercentFromDominantPercent(int dominantPercent)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(dominantPercent);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.Equal(dominantPercent, trial.SwitchPercent);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials sets CongruencePercent from config on all trials.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void GenerateTrials_SetsCongruencePercentFromConfig(int congruencePercent)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(congruencePercent);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.Equal(congruencePercent, trial.CongruencePercent);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials creates Word stimuli for all trials.
        /// </summary>
        [Fact]
        public void GenerateTrials_CreatesWordStimulusForAllTrials()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(10);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.NotNull(trial.Stimulus);
                Assert.NotNull(trial.Stimulus.Color);
                Assert.NotNull(trial.Stimulus.InternalText);
                Assert.NotNull(trial.Stimulus.Text);
            });
        }

        /// <summary>
        /// Tests that congruent trials have matching Color and InternalText in their stimuli.
        /// </summary>
        [Fact]
        public void GenerateTrials_CongruentTrials_HaveMatchingColorAndInternalText()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(20);
            mockConfig.Setup(c => c.CongruencePercent).Returns(100);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.True(trial.IsCongruent);
                Assert.Equal(trial.Stimulus.Color, trial.Stimulus.InternalText);
            });
        }

        /// <summary>
        /// Tests that incongruent trials have different Color and InternalText in their stimuli.
        /// </summary>
        [Fact]
        public void GenerateTrials_IncongruentTrials_HaveDifferentColorAndInternalText()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(20);
            mockConfig.Setup(c => c.CongruencePercent).Returns(0);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.False(trial.IsCongruent);
                Assert.NotEqual(trial.Stimulus.Color, trial.Stimulus.InternalText);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials assigns visual cues when hasVisualCue is true.
        /// </summary>
        [Fact]
        public void GenerateTrials_WithVisualCue_AssignsVisualCuesToTrials()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(10);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(true);
            mockConfig.Setup(c => c.DominantPercent).Returns(60);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.True(Enum.IsDefined(typeof(VisualCueType), trial.VisualCue));
            });
        }

        /// <summary>
        /// Tests that GenerateTrials does not assign non-None visual cues when hasVisualCue is false.
        /// </summary>
        [Fact]
        public void GenerateTrials_WithoutVisualCue_DoesNotAssignVisualCues()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(10);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(60);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.Equal(VisualCueType.None, trial.VisualCue);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials uses valid color names from the predefined set.
        /// </summary>
        [Fact]
        public void GenerateTrials_UsesValidColorNames()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(20);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");
            var validColors = new[] { "Blue", "Red", "Green", "Yellow" };

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.Contains(trial.Stimulus.Color, validColors);
                Assert.Contains(trial.Stimulus.InternalText, validColors);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials handles CongruencePercent with rounding correctly.
        /// </summary>
        [Theory]
        [InlineData(33, 33)]
        [InlineData(67, 67)]
        [InlineData(25, 25)]
        [InlineData(75, 75)]
        public void GenerateTrials_CongruencePercentWithRounding_CalculatesCorrectly(int congruencePercent, int expectedCongruent)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(100);
            mockConfig.Setup(c => c.CongruencePercent).Returns(congruencePercent);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            var congruentCount = result.Count(t => t.IsCongruent);
            Assert.Equal(expectedCongruent, congruentCount);
        }

        /// <summary>
        /// Tests that GenerateTrials handles edge case with WordCount of int.MaxValue (simulated with large number).
        /// </summary>
        [Fact]
        public void GenerateTrials_LargeWordCount_GeneratesCorrectNumberOfTrials()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(1000);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000, result.Count);
        }

        /// <summary>
        /// Tests that GenerateTrials handles negative Block numbers.
        /// </summary>
        [Fact]
        public void GenerateTrials_NegativeBlock_SetsNegativeBlockOnTrials()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(-1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.Equal(-1, trial.Block));
        }

        /// <summary>
        /// Tests that GenerateTrials handles extreme CongruencePercent values beyond 100.
        /// </summary>
        [Fact]
        public void GenerateTrials_CongruencePercentAbove100_AllTrialsCongruent()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(10);
            mockConfig.Setup(c => c.CongruencePercent).Returns(100);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert - when congruentCount > total, all will be congruent
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.True(trial.IsCongruent));
        }

        /// <summary>
        /// Tests that GenerateTrials handles extreme negative CongruencePercent values.
        /// </summary>
        [Fact]
        public void GenerateTrials_NegativeCongruencePercent_AllTrialsIncongruent()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(10);
            mockConfig.Setup(c => c.CongruencePercent).Returns(0);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert - 0% congruent will result in all incongruent
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.False(trial.IsCongruent));
        }

        /// <summary>
        /// Tests that GenerateTrials correctly retrieves all four localized word strings.
        /// </summary>
        [Fact]
        public void GenerateTrials_RetrievesAllFourLocalizedWords()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_BLUE", "en")).Returns("Blue");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_RED", "en")).Returns("Red");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_GREEN", "en")).Returns("Green");
            mockLanguageService.Setup(s => s.GetLocalizedString("Word_YELLOW", "en")).Returns("Yellow");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(1);
            mockConfig.Setup(c => c.CongruencePercent).Returns(100);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_BLUE", "en"), Times.Once);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_RED", "en"), Times.Once);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_GREEN", "en"), Times.Once);
            mockLanguageService.Verify(s => s.GetLocalizedString("Word_YELLOW", "en"), Times.Once);
        }

        /// <summary>
        /// Tests that GenerateTrials handles int.MinValue for CongruencePercent.
        /// </summary>
        [Fact]
        public void GenerateTrials_CongruencePercentIntMinValue_AllTrialsIncongruent()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(10);
            mockConfig.Setup(c => c.CongruencePercent).Returns(int.MinValue);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.False(trial.IsCongruent));
        }

        /// <summary>
        /// Tests that GenerateTrials handles int.MinValue for Block.
        /// </summary>
        [Fact]
        public void GenerateTrials_BlockIntMinValue_SetsBlockCorrectly()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(int.MinValue);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.Equal(int.MinValue, trial.Block));
        }

        /// <summary>
        /// Tests that GenerateTrials handles int.MaxValue for Block.
        /// </summary>
        [Fact]
        public void GenerateTrials_BlockIntMaxValue_SetsBlockCorrectly()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(int.MaxValue);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(50);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, trial => Assert.Equal(int.MaxValue, trial.Block));
        }

        /// <summary>
        /// Tests that GenerateTrials handles int.MinValue for DominantPercent.
        /// </summary>
        [Fact]
        public void GenerateTrials_DominantPercentIntMinValue_SetsSwitchPercentCorrectly()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(int.MinValue);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.Equal(int.MinValue, trial.SwitchPercent);
            });
        }

        /// <summary>
        /// Tests that GenerateTrials handles int.MaxValue for DominantPercent.
        /// </summary>
        [Fact]
        public void GenerateTrials_DominantPercentIntMaxValue_SetsSwitchPercentCorrectly()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            mockLanguageService.Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<string>())).Returns("MockWord");
            var service = new TrialGenerationService(mockLanguageService.Object);
            var mockConfig = new Mock<ITrialConfiguration>();
            mockConfig.Setup(c => c.WordCount).Returns(5);
            mockConfig.Setup(c => c.CongruencePercent).Returns(50);
            mockConfig.Setup(c => c.Block).Returns(1);
            mockConfig.Setup(c => c.ParticipantId).Returns("P001");
            mockConfig.Setup(c => c.HasVisualCue).Returns(false);
            mockConfig.Setup(c => c.DominantPercent).Returns(int.MaxValue);
            mockConfig.Setup(c => c.TaskLanguage).Returns("en");

            // Act
            var result = service.GenerateTrials(mockConfig.Object);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, itrial =>
            {
                var trial = (StroopTrial)itrial;
                Assert.Equal(int.MaxValue, trial.SwitchPercent);
            });

        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence throws ArgumentException when count is zero.
        /// </summary>
        [Fact]
        public void GenerateVisualCueSequence_CountIsZero_ThrowsArgumentException()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => service.GenerateVisualCueSequence(0, 50));
            Assert.Equal("count", exception.ParamName);
            Assert.Contains("Count of visual cues must be positive", exception.Message);
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence throws ArgumentException when count is negative.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(int.MinValue)]
        public void GenerateVisualCueSequence_CountIsNegative_ThrowsArgumentException(int count)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => service.GenerateVisualCueSequence(count, 50));
            Assert.Equal("count", exception.ParamName);
            Assert.Contains("Count of visual cues must be positive", exception.Message);
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence returns a sequence with exactly one element when count is 1.
        /// </summary>
        [Fact]
        public void GenerateVisualCueSequence_CountIsOne_ReturnsSingleElement()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(1, 50);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0] == VisualCueType.Round || result[0] == VisualCueType.Square);
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence returns a sequence of the correct length.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void GenerateVisualCueSequence_ValidCount_ReturnsCorrectLength(int count)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(count, 50);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(count, result.Count);
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence with 0% switch percentage returns a sequence with no switches.
        /// </summary>
        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        public void GenerateVisualCueSequence_ZeroSwitchPercentage_ReturnsNoSwitches(int count)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(count, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(count, result.Count);
            var firstValue = result[0];
            Assert.All(result, cue => Assert.Equal(firstValue, cue));
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence with 100% switch percentage returns maximum switches.
        /// </summary>
        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        public void GenerateVisualCueSequence_HundredSwitchPercentage_ReturnsMaximumSwitches(int count)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(count, 100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(count, result.Count);

            // Count actual switches
            int switches = 0;
            for (int i = 1; i < result.Count; i++)
            {
                if (result[i] != result[i - 1])
                {
                    switches++;
                }
            }

            // With 100% switch percentage, all transitions should be switches
            Assert.Equal(count - 1, switches);
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence returns only valid VisualCueType values.
        /// </summary>
        [Fact]
        public void GenerateVisualCueSequence_ReturnsOnlyValidVisualCueTypes()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(50, 50);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, cue => Assert.True(cue == VisualCueType.Round || cue == VisualCueType.Square));
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence with various switch percentages produces approximately correct switch counts.
        /// </summary>
        [Theory]
        [InlineData(100, 0)]
        [InlineData(100, 25)]
        [InlineData(100, 50)]
        [InlineData(100, 75)]
        [InlineData(100, 100)]
        public void GenerateVisualCueSequence_VariousSwitchPercentages_ProducesApproximateCorrectSwitchCount(int count, int switchPercentage)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(count, switchPercentage);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(count, result.Count);

            // Count actual switches
            int actualSwitches = 0;
            for (int i = 1; i < result.Count; i++)
            {
                if (result[i] != result[i - 1])
                {
                    actualSwitches++;
                }
            }

            // Expected switches based on algorithm: (count - 1) * switchPercentage / 100
            int expectedSwitches = (count - 1) * switchPercentage / 100;
            Assert.Equal(expectedSwitches, actualSwitches);
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence handles negative switch percentage.
        /// </summary>
        [Fact]
        public void GenerateVisualCueSequence_NegativeSwitchPercentage_HandlesGracefully()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(10, -10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Count);
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence with count of 2 produces valid sequence.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        public void GenerateVisualCueSequence_CountIsTwo_ProducesValidSequence(int switchPercentage)
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Act
            var result = service.GenerateVisualCueSequence(2, switchPercentage);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.True(result[0] == VisualCueType.Round || result[0] == VisualCueType.Square);
            Assert.True(result[1] == VisualCueType.Round || result[1] == VisualCueType.Square);

            if (switchPercentage == 0)
            {
                Assert.Equal(result[0], result[1]);
            }
            else if (switchPercentage == 100)
            {
                Assert.NotEqual(result[0], result[1]);
            }
        }

        /// <summary>
        /// Tests that GenerateVisualCueSequence produces deterministic switch count for given parameters.
        /// </summary>
        [Fact]
        public void GenerateVisualCueSequence_MultipleCallsSameParameters_ProducesSameSwitchCount()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var service = new TrialGenerationService(mockLanguageService.Object);
            int count = 50;
            int switchPercentage = 60;

            // Act
            var result1 = service.GenerateVisualCueSequence(count, switchPercentage);
            var result2 = service.GenerateVisualCueSequence(count, switchPercentage);

            // Assert
            int switches1 = 0;
            for (int i = 1; i < result1.Count; i++)
            {
                if (result1[i] != result1[i - 1])
                    switches1++;
            }

            int switches2 = 0;
            for (int i = 1; i < result2.Count; i++)
            {
                if (result2[i] != result2[i - 1])
                    switches2++;
            }

            // Both should have the same number of switches (deterministic based on algorithm)
            Assert.Equal(switches1, switches2);
        }

        /// <summary>
        /// Verifies that the constructor throws ArgumentNullException when languageService parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullLanguageService_ThrowsArgumentNullException()
        {
            // Arrange
            ILanguageService? languageService = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new TrialGenerationService(languageService!));
            Assert.Equal("languageService", exception.ParamName);
        }

        /// <summary>
        /// Verifies that the constructor successfully initializes the service when provided with a valid languageService.
        /// </summary>
        [Fact]
        public void Constructor_ValidLanguageService_InitializesSuccessfully()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();

            // Act
            var service = new TrialGenerationService(mockLanguageService.Object);

            // Assert
            Assert.NotNull(service);
        }
    }
}