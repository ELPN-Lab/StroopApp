using LiveChartsCore.SkiaSharpView;
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
        /// Stopping a calibration block gives its result right away and keeps only the played trials, so the end page can show them.
        /// </summary>
        [Fact]
        public void StopCalibrationBlock_DuringPhase1_SetsResultAndTrims()
        {
            var settings = CreateSettings(new StroopProfile { IsCalibration = true, WordCount = 120 });
            settings.ExperimentContext.AddNewSerie(settings);
            var block = Assert.IsType<CalibrationBlock>(settings.ExperimentContext.CurrentBlock);
            for (int i = 1; i <= 120; i++)
                block.TrialRecords.Add(new StroopTrial { TrialNumber = i });
            for (int i = 0; i < 3; i++)
                block.RecordTrial(block.TrialRecords[i]!);

            settings.ExperimentContext.StopCalibrationBlock();

            Assert.Equal(CalibrationStopReason.StoppedByExperimenter, block.Result?.StopReason);
            Assert.Equal(3, block.TrialRecords.Count);
            Assert.Equal(3, settings.ExperimentContext.Sections[0].Xj);
        }

        /// <summary>
        /// Stopping a standard block leaves it untouched.
        /// </summary>
        [Fact]
        public void StopCalibrationBlock_StandardBlock_DoesNothing()
        {
            var settings = CreateSettings(new StroopProfile { WordCount = 120 });
            settings.ExperimentContext.AddNewSerie(settings);

            settings.ExperimentContext.StopCalibrationBlock();

            Assert.Equal(120, settings.ExperimentContext.Sections[0].Xj);
        }

        /// <summary>
        /// A calibration block keeps one section over the whole block, with a phase 2 section and a separator from the default 20th trial.
        /// </summary>
        [Fact]
        public void AddNewSerie_CalibrationProfile_AddsBlockPhase2AndSeparatorSections()
        {
            var settings = CreateSettings(new StroopProfile { IsCalibration = true, WordCount = 120 });

            settings.ExperimentContext.AddNewSerie(settings);

            Assert.Collection(settings.ExperimentContext.Sections,
                block => { Assert.Equal(1, block.Xi); Assert.Equal(120, block.Xj); },
                phase2 => { Assert.Equal(20.5, phase2.Xi); Assert.Equal(120, phase2.Xj); },
                separator => { Assert.Equal(20.5, separator.Xi); Assert.Equal(20.5, separator.Xj); });
        }

        /// <summary>
        /// Each phase of a calibration block has its own line, which leaves out the other phase so no segment links the two.
        /// </summary>
        [Fact]
        public void AddNewSerie_CalibrationProfile_EachLineLeavesOutTheOtherPhase()
        {
            var settings = CreateSettings(new StroopProfile { IsCalibration = true, WordCount = 120 });

            settings.ExperimentContext.AddNewSerie(settings);

            var phase1Line = Assert.IsType<LineSeries<double?>>(settings.ExperimentContext.BlockSeries[0]);
            var phase2Line = Assert.IsType<LineSeries<double?>>(settings.ExperimentContext.BlockSeries[1]);
            Assert.False(phase1Line.Mapping!(500, 19).IsEmpty);
            Assert.True(phase1Line.Mapping!(500, 20).IsEmpty);
            Assert.True(phase2Line.Mapping!(500, 19).IsEmpty);
            Assert.False(phase2Line.Mapping!(500, 20).IsEmpty);
        }

        /// <summary>
        /// A calibration block that ends during phase 1 loses its phase 2 section and its separator.
        /// </summary>
        [Fact]
        public void TrimCurrentBlock_CalibrationEndedInPhase1_RemovesPhase2Sections()
        {
            var settings = CreateSettings(new StroopProfile { IsCalibration = true, WordCount = 120 });
            settings.ExperimentContext.AddNewSerie(settings);

            settings.ExperimentContext.TrimCurrentBlock(3);

            var section = Assert.Single(settings.ExperimentContext.Sections);
            Assert.Equal(3, section.Xj);
        }

        /// <summary>
        /// A calibration block that ends during phase 2 shortens the block and phase 2 sections and keeps the separator.
        /// </summary>
        [Fact]
        public void TrimCurrentBlock_CalibrationEndedInPhase2_ShortensBlockAndPhase2Sections()
        {
            var settings = CreateSettings(new StroopProfile { IsCalibration = true, WordCount = 120 });
            settings.ExperimentContext.AddNewSerie(settings);

            settings.ExperimentContext.TrimCurrentBlock(45);

            Assert.Collection(settings.ExperimentContext.Sections,
                block => Assert.Equal(45, block.Xj),
                phase2 => Assert.Equal(45, phase2.Xj),
                separator => Assert.Equal(20.5, separator.Xi));
        }

        /// <summary>
        /// A calibration block that ends right after phase 1 has no phase 2 trial, so it loses its phase 2 section and its separator.
        /// </summary>
        [Fact]
        public void TrimCurrentBlock_CalibrationEndedAtPhaseChange_RemovesPhase2Sections()
        {
            var settings = CreateSettings(new StroopProfile { IsCalibration = true, WordCount = 120 });
            settings.ExperimentContext.AddNewSerie(settings);

            settings.ExperimentContext.TrimCurrentBlock(20);

            var section = Assert.Single(settings.ExperimentContext.Sections);
            Assert.Equal(20, section.Xj);
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
