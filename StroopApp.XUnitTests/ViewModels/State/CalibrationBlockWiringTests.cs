using StroopApp.Models;
using StroopApp.Models.Calibration;
using StroopApp.Services.Trial;
using StroopApp.ViewModels.State;
using Xunit;

namespace StroopApp.XUnitTests.ViewModels.State
{
    /// <summary>
    /// Unit tests for the creation and sizing of calibration blocks in the experiment state.
    /// </summary>
    public class CalibrationBlockWiringTests
    {
        private static ExperimentSettingsViewModel CreateSettings(ExperimentProfile profile)
        {
            return new ExperimentSettingsViewModel(new ExperimentSettings())
            {
                CurrentProfile = profile,
                Participant = new Participant { Id = "P1" },
                Block = 1
            };
        }

        /// <summary>
        /// A calibration profile creates a calibration block with a copy of its parameters.
        /// </summary>
        [Fact]
        public void AddNewSerie_CalibrationProfile_CreatesCalibrationBlock()
        {
            var profile = new StroopProfile { IsCalibration = true, CalibrationParameters = new CalibrationParameters { StartingTime = 5000 } };
            var settings = CreateSettings(profile);

            settings.ExperimentContext.AddNewSerie(settings);

            var block = Assert.IsType<CalibrationBlock>(settings.ExperimentContext.CurrentBlock);
            Assert.Equal(5000, block.GetNextMaxReactionTime());
            Assert.NotSame(profile.CalibrationParameters, block.Parameters);
        }

        /// <summary>
        /// A standard profile still creates a standard block.
        /// </summary>
        [Fact]
        public void AddNewSerie_StandardProfile_CreatesStandardBlock()
        {
            var settings = CreateSettings(new StroopProfile { MaxReactionTime = 700 });

            settings.ExperimentContext.AddNewSerie(settings);

            var block = Assert.IsType<Block>(settings.ExperimentContext.CurrentBlock);
            Assert.Equal(700, block.GetNextMaxReactionTime());
        }

        /// <summary>
        /// After an early end, the block's chart section ends at the last played trial and the next block starts right after.
        /// </summary>
        [Fact]
        public void TrimCurrentBlock_EarlyEnd_ShortensSectionAndNextStart()
        {
            var settings = CreateSettings(new StroopProfile { WordCount = 120 });
            settings.ExperimentContext.AddNewSerie(settings);

            settings.ExperimentContext.TrimCurrentBlock(45);

            var section = Assert.Single(settings.ExperimentContext.Sections);
            Assert.Equal(1, section.Xi);
            Assert.Equal(45, section.Xj);

            settings.Block = 2;
            settings.CurrentProfile = new StroopProfile { WordCount = 10 };
            settings.ExperimentContext.AddNewSerie(settings);

            Assert.Equal(46, settings.ExperimentContext.Sections[1].Xi);
        }

        /// <summary>
        /// For a calibration profile, trial generation creates phase 1 plus the maximum of phase 2 trials, whatever the word count.
        /// </summary>
        [Fact]
        public void TrialConfigurationAdapter_CalibrationProfile_UsesCalibrationTrialCount()
        {
            var profile = new StroopProfile
            {
                WordCount = 10,
                IsCalibration = true,
                CalibrationParameters = new CalibrationParameters { Phase1TrialCount = 20, MaxPhase2TrialCount = 80 }
            };

            var adapter = new ExperimentSettingsTrialConfigurationAdapter(CreateSettings(profile));

            Assert.Equal(100, adapter.WordCount);
        }

        /// <summary>
        /// For a standard profile, trial generation still uses the word count.
        /// </summary>
        [Fact]
        public void TrialConfigurationAdapter_StandardProfile_UsesWordCount()
        {
            var adapter = new ExperimentSettingsTrialConfigurationAdapter(CreateSettings(new StroopProfile { WordCount = 37 }));

            Assert.Equal(37, adapter.WordCount);
        }
    }
}
