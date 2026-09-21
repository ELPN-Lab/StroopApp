namespace StroopApp.XUnitTests.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using StroopApp.Models;
    using Xunit;

    public class ExperimentSettingsTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var settings = new ExperimentSettings();
            // Assert
            Assert.NotNull(settings.KeyMappings);
            Assert.NotNull(settings.ExperimentContext);
            Assert.Equal("", settings.ExportFolderPath);
            Assert.Equal(1, settings.Block);
        }

        [Fact]
        public void Block_SetValue_UpdatesProperty()
        {
            // Arrange
            var settings = new ExperimentSettings();
            // Act
            settings.Block = 3;
            // Assert
            Assert.Equal(3, settings.Block);
        }

        [Fact]
        public void Participant_SetValue_UpdatesProperty()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var participant = new Participant();
            // Act
            settings.Participant = participant;
            // Assert
            Assert.Equal(participant, settings.Participant);
        }

        [Fact]
        public void CurrentProfile_SetValue_UpdatesProperty()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var profile = new StroopProfile();
            // Act
            settings.CurrentProfile = profile;
            // Assert
            Assert.Equal(profile, settings.CurrentProfile);
        }

        [Fact]
        public void KeyMappings_SetValue_UpdatesProperty()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var mappings = new ExperimentKeyMappings();
            // Act
            settings.KeyMappings = mappings;
            // Assert
            Assert.Equal(mappings, settings.KeyMappings);
        }

        [Fact]
        public void ExperimentContext_SetValue_UpdatesProperty()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var context = new SharedExperimentData();
            // Act
            settings.ExperimentContext = context;
            // Assert
            Assert.Equal(context, settings.ExperimentContext);
        }

        [Fact]
        public void ExportFolderPath_SetValue_UpdatesProperty()
        {
            // Arrange
            var settings = new ExperimentSettings();
            // Act
            settings.ExportFolderPath = "C:\\Exports";
            // Assert
            Assert.Equal("C:\\Exports", settings.ExportFolderPath);
        }

        [Fact]
        public void Reset_CallsContextReset_ResetsBlock_AndNotifies()
        {
            // Arrange
            var context = new SharedExperimentDataMock();
            var settings = new ExperimentSettings
            {
                ExperimentContext = context,
                Block = 5
            };
            // Act
            settings.Reset();
            // Assert
            Assert.True(context.ResetCalled);
            Assert.Equal(1, settings.Block);
        }

        // ========== CHARACTERIZATION TESTS FOR Reset() =========
        // These tests freeze the current behavior of Reset() to enable safe refactoring
        [Fact]
        public void Reset_GivenBlockIs5_ResetsBlockTo1()
        {
            // Arrange
            var settings = new ExperimentSettings
            {
                Block = 5
            };
            // Act
            settings.Reset();
            // Assert
            Assert.Equal(1, settings.Block);
        }

        [Fact]
        public void Reset_GivenBlockIs1_BlockRemainsAt1()
        {
            // Arrange
            var settings = new ExperimentSettings
            {
                Block = 1
            };
            // Act
            settings.Reset();
            // Assert
            Assert.Equal(1, settings.Block);
        }

        [Fact]
        public void Reset_AlwaysSetsIsBlockFinishedToTrue()
        {
            // Arrange
            var settings = new ExperimentSettings();
            settings.ExperimentContext.IsBlockFinished = false;
            // Act
            settings.Reset();
            // Assert
            Assert.True(settings.ExperimentContext.IsBlockFinished);
        }

        [Fact]
        public void Reset_AlwaysSetsIsParticipantSelectionEnabledToTrue()
        {
            // Arrange
            var settings = new ExperimentSettings();
            settings.ExperimentContext.IsParticipantSelectionEnabled = false;
            // Act
            settings.Reset();
            // Assert
            Assert.True(settings.ExperimentContext.IsParticipantSelectionEnabled);
        }

        [Fact]
        public void Reset_AlwaysSetsHasUnsavedExportsToTrue()
        {
            // Arrange
            var settings = new ExperimentSettings();
            settings.ExperimentContext.HasUnsavedExports = false;
            // Act
            settings.Reset();
            // Assert
            Assert.True(settings.ExperimentContext.HasUnsavedExports);
        }

        [Fact]
        public void Reset_CallsExperimentContextReset()
        {
            // Arrange
            var spyContext = new SpySharedExperimentData();
            var settings = new ExperimentSettings
            {
                ExperimentContext = spyContext
            };
            // Act
            settings.Reset();
            // Assert
            Assert.Equal(1, spyContext.ResetCallCount);
        }

        [Fact]
        public void Reset_DoesNotModifyParticipant()
        {
            // Arrange
            var participant = new Participant
            {
                Id = "TestParticipant123"
            };
            var settings = new ExperimentSettings
            {
                Participant = participant
            };
            // Act
            settings.Reset();
            // Assert
            Assert.Same(participant, settings.Participant);
            Assert.Equal("TestParticipant123", settings.Participant.Id);
        }

        [Fact]
        public void Reset_DoesNotModifyCurrentProfile()
        {
            // Arrange
            var profile = new StroopProfile
            {
                ProfileName = "TestProfile"
            };
            var settings = new ExperimentSettings
            {
                CurrentProfile = profile
            };
            // Act
            settings.Reset();
            // Assert
            Assert.Same(profile, settings.CurrentProfile);
            Assert.Equal("TestProfile", settings.CurrentProfile.ProfileName);
        }

        [Fact]
        public void Reset_DoesNotModifyKeyMappings()
        {
            // Arrange
            var mappings = new ExperimentKeyMappings();
            var settings = new ExperimentSettings
            {
                KeyMappings = mappings
            };
            // Act
            settings.Reset();
            // Assert
            Assert.Same(mappings, settings.KeyMappings);
        }

        [Fact]
        public void Reset_DoesNotModifyExportFolderPath()
        {
            // Arrange
            var settings = new ExperimentSettings
            {
                ExportFolderPath = "C:\\MyExports"
            };
            // Act
            settings.Reset();
            // Assert
            Assert.Equal("C:\\MyExports", settings.ExportFolderPath);
        }

        // ========== TEST HELPERS ==========
        private class SharedExperimentDataMock : SharedExperimentData
        {
            public bool ResetCalled { get; private set; }

            public override void Reset()
            {
                ResetCalled = true;
            }
        }

        private class SpySharedExperimentData : SharedExperimentData
        {
            public int ResetCallCount { get; private set; }

            public override void Reset()
            {
                ResetCallCount++;
                base.Reset();
            }
        }

        private class OrderTrackingSharedExperimentData : SharedExperimentData
        {
            public List<string> ExecutionLog { get; } = new List<string>();

            public override void Reset()
            {
                ExecutionLog.Add("Reset");
                base.Reset();
            }
        }
    }
}