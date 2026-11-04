using StroopApp.Models;
using StroopApp.Models.Calibration;
using StroopApp.Services.Profile;
using System.Collections.ObjectModel;
using System.IO;
using Xunit;

namespace StroopApp.XUnitTests.Models
{
    /// <summary>
    /// Unit tests for the calibration settings of StroopProfile: defaults, cloning, update and JSON storage.
    /// </summary>
    public class StroopProfileCalibrationTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly ProfileService _profileService;

        public StroopProfileCalibrationTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
            _profileService = new ProfileService(new AppConfiguration { ConfigDirectory = _tempDirectory });
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }

        private static StroopProfile CreateCalibrationProfile()
        {
            return new StroopProfile
            {
                IsCalibration = true,
                CalibrationParameters = new CalibrationParameters { StartingTime = 5000, Beta = 1.85, MaxPhase2TrialCount = 80 }
            };
        }

        /// <summary>
        /// A new Stroop profile is not a calibration and holds the default parameters.
        /// </summary>
        [Fact]
        public void Constructor_Defaults_NotCalibrationWithDefaultParameters()
        {
            var profile = new StroopProfile();

            Assert.False(profile.IsCalibration);
            Assert.Equivalent(new CalibrationParameters(), profile.CalibrationParameters);
        }

        /// <summary>
        /// Cloning copies the calibration flag and parameter values into an independent object.
        /// </summary>
        [Fact]
        public void CloneProfile_CalibrationProfile_CopiesSettingsIndependently()
        {
            var original = CreateCalibrationProfile();

            var clone = original.CloneProfile();
            clone.CalibrationParameters.Beta = 3.0;

            Assert.True(clone.IsCalibration);
            Assert.Equal(5000, clone.CalibrationParameters.StartingTime);
            Assert.Equal(80, clone.CalibrationParameters.MaxPhase2TrialCount);
            Assert.NotSame(original.CalibrationParameters, clone.CalibrationParameters);
            Assert.Equal(1.85, original.CalibrationParameters.Beta);
        }

        /// <summary>
        /// UpdateFrom copies the calibration flag and parameter values into an independent object.
        /// </summary>
        [Fact]
        public void UpdateFrom_CalibrationProfile_CopiesSettingsIndependently()
        {
            var source = CreateCalibrationProfile();
            var target = new StroopProfile();

            target.UpdateFrom(source);
            target.CalibrationParameters.Beta = 3.0;

            Assert.True(target.IsCalibration);
            Assert.Equal(5000, target.CalibrationParameters.StartingTime);
            Assert.NotSame(source.CalibrationParameters, target.CalibrationParameters);
            Assert.Equal(1.85, source.CalibrationParameters.Beta);
        }

        /// <summary>
        /// A profile file written before the calibration feature loads as a non-calibration profile with default parameters.
        /// </summary>
        [Fact]
        public void LoadProfiles_FileWithoutCalibrationFields_UsesDefaults()
        {
            File.WriteAllText(Path.Combine(_tempDirectory, "profiles.json"), "[{\"TaskType\":0,\"ProfileName\":\"Old\",\"MaxReactionTime\":400,\"WordCount\":10}]");

            var profile = Assert.IsType<StroopProfile>(Assert.Single(_profileService.LoadProfiles()));

            Assert.False(profile.IsCalibration);
            Assert.Equivalent(new CalibrationParameters(), profile.CalibrationParameters);
        }

        /// <summary>
        /// A calibration profile keeps its settings after saving and loading.
        /// </summary>
        [Fact]
        public void SaveAndLoadProfiles_CalibrationProfile_KeepsSettings()
        {
            var original = CreateCalibrationProfile();

            _profileService.SaveProfiles(new ObservableCollection<ExperimentProfile> { original });
            var loaded = Assert.IsType<StroopProfile>(Assert.Single(_profileService.LoadProfiles()));

            Assert.True(loaded.IsCalibration);
            Assert.Equivalent(original.CalibrationParameters, loaded.CalibrationParameters);
        }

        /// <summary>
        /// A calibration trial can last up to the upper bound, whatever the max reaction time.
        /// </summary>
        [Fact]
        public void GetLongestReactionTime_Calibration_ReturnsUpperBound()
        {
            var profile = CreateCalibrationProfile();
            profile.MaxReactionTime = 1000;

            Assert.Equal(profile.CalibrationParameters.UpperBound, profile.GetLongestReactionTime());
        }

        /// <summary>
        /// Outside calibration, the longest trial is the max reaction time.
        /// </summary>
        [Fact]
        public void GetLongestReactionTime_Standard_ReturnsMaxReactionTime()
        {
            var profile = new StroopProfile { MaxReactionTime = 1000 };

            Assert.Equal(1000, profile.GetLongestReactionTime());
        }
    }
}
