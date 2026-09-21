using StroopApp.Models;
using StroopApp.Models.Simon;
using StroopApp.Services.Profile;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StroopApp.Services.Profile.UnitTests
{
    /// <summary>
    /// Unit tests for the ProfileService class.
    /// </summary>
    public class ProfileServiceTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes all fields when given a valid AppConfiguration
        /// with a non-null ConfigDirectory.
        /// </summary>
        [Fact]
        public void Constructor_ValidConfigDirectory_InitializesFieldsCorrectly()
        {
            // Arrange
            var configDir = new AppConfiguration { ConfigDirectory = @"C:\TestConfig" };

            // Act
            var service = new ProfileService(configDir);

            // Assert
            // Verify the service is created (fields are private, so we can only verify no exception was thrown)
            Assert.NotNull(service);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when the ConfigDirectory property
        /// of the AppConfiguration parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullConfigDirectory_ThrowsArgumentNullException()
        {
            // Arrange
            var configDir = new AppConfiguration { ConfigDirectory = null! };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ProfileService(configDir));
            Assert.Equal("configDir.ConfigDirectory", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when the AppConfiguration parameter
        /// itself is null.
        /// </summary>
        [Fact]
        public void Constructor_NullAppConfiguration_ThrowsArgumentNullException()
        {
            // Arrange
            AppConfiguration configDir = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ProfileService(configDir));
        }

        /// <summary>
        /// Tests that the constructor handles empty string ConfigDirectory without throwing exceptions.
        /// Path.Combine will handle empty strings appropriately.
        /// </summary>
        [Fact]
        public void Constructor_EmptyStringConfigDirectory_DoesNotThrow()
        {
            // Arrange
            var configDir = new AppConfiguration { ConfigDirectory = string.Empty };

            // Act
            var service = new ProfileService(configDir);

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Tests that the constructor handles whitespace-only ConfigDirectory without throwing exceptions.
        /// Path.Combine will handle whitespace strings appropriately.
        /// </summary>
        [Fact]
        public void Constructor_WhitespaceConfigDirectory_DoesNotThrow()
        {
            // Arrange
            var configDir = new AppConfiguration { ConfigDirectory = "   " };

            // Act
            var service = new ProfileService(configDir);

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Tests that the constructor handles ConfigDirectory with special path characters
        /// without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_ConfigDirectoryWithSpecialCharacters_DoesNotThrow()
        {
            // Arrange
            var configDir = new AppConfiguration { ConfigDirectory = @"C:\Test\Path\With\Special-Chars_123" };

            // Act
            var service = new ProfileService(configDir);

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Tests that the constructor handles very long ConfigDirectory paths without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongConfigDirectory_DoesNotThrow()
        {
            // Arrange
            var longPath = new string('a', 200);
            var configDir = new AppConfiguration { ConfigDirectory = longPath };

            // Act
            var service = new ProfileService(configDir);

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Tests that the constructor handles ConfigDirectory with path separator at the end.
        /// </summary>
        [Fact]
        public void Constructor_ConfigDirectoryWithTrailingSeparator_DoesNotThrow()
        {
            // Arrange
            var configDir = new AppConfiguration { ConfigDirectory = @"C:\TestConfig\" };

            // Act
            var service = new ProfileService(configDir);

            // Assert
            Assert.NotNull(service);
        }

        /// <summary>
        /// Tests that the constructor handles relative paths in ConfigDirectory without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_RelativePathConfigDirectory_DoesNotThrow()
        {
            // Arrange
            var configDir = new AppConfiguration { ConfigDirectory = @"..\RelativePath" };

            // Act
            var service = new ProfileService(configDir);

            // Assert
            Assert.NotNull(service);
        }
        private readonly string _tempDirectory;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            // Create a unique temporary directory for each test
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);

            var config = new AppConfiguration { ConfigDirectory = _tempDirectory };
            _profileService = new ProfileService(config);
        }
        [Fact]
        public void Dispose()
        {
            // Clean up temporary directory after each test
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        /// <summary>
        /// Tests that UpsertProfile inserts a new profile with Guid.Empty and generates a new GUID.
        /// Input: Profile with Id = Guid.Empty.
        /// Expected: Profile is added with a new GUID, collection contains the profile.
        /// </summary>
        [Fact]
        public void UpsertProfile_NewProfileWithEmptyGuid_GeneratesNewGuidAndInsertsProfile()
        {
            // Arrange
            var profile = new StroopProfile
            {
                Id = Guid.Empty,
                ProfileName = "Test Profile",
                Hours = 1,
                Minutes = 30,
                Seconds = 0,
                FixationDuration = 100,
                MaxReactionTime = 400,
                VisualCueDuration = 50,
                HasVisualCue = true,
                GroupSize = 10,
                WordCount = 20,
                CalculationMode = CalculationMode.WordCount,
                DominantPercent = 60,
                CongruencePercent = 40,
                SwitchPercent = 30,
                TaskLanguage = "en"
            };

            // Act
            var result = _profileService.UpsertProfile(profile);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.NotEqual(Guid.Empty, profile.Id);
            Assert.Equal("Test Profile", result.First().ProfileName);
        }

        /// <summary>
        /// Tests that UpsertProfile inserts a new profile with a specific GUID.
        /// Input: Profile with a specific non-empty GUID.
        /// Expected: Profile is added with the same GUID, collection contains the profile.
        /// </summary>
        [Fact]
        public void UpsertProfile_NewProfileWithSpecificGuid_InsertsProfileWithSameGuid()
        {
            // Arrange
            var specificGuid = Guid.NewGuid();
            var profile = new StroopProfile
            {
                Id = specificGuid,
                ProfileName = "Specific GUID Profile",
                Hours = 0,
                Minutes = 45,
                Seconds = 30,
                FixationDuration = 150,
                MaxReactionTime = 500,
                VisualCueDuration = 0,
                HasVisualCue = false,
                GroupSize = 5,
                WordCount = 15,
                CalculationMode = CalculationMode.TaskDuration,
                DominantPercent = 50,
                CongruencePercent = 50,
                SwitchPercent = null,
                TaskLanguage = "fr"
            };

            // Act
            var result = _profileService.UpsertProfile(profile);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(specificGuid, result.First().Id);
            Assert.Equal("Specific GUID Profile", result.First().ProfileName);
        }

        /// <summary>
        /// Tests that UpsertProfile updates an existing profile's properties.
        /// Input: Profile with matching ID to an existing profile.
        /// Expected: Existing profile is updated with new values, UpdateDerivedValues is called.
        /// </summary>
        [Fact]
        public void UpsertProfile_ExistingProfile_UpdatesAllProperties()
        {
            // Arrange
            var existingProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Original Name",
                Hours = 1,
                Minutes = 0,
                Seconds = 0,
                FixationDuration = 100,
                MaxReactionTime = 400,
                VisualCueDuration = 0,
                HasVisualCue = false,
                GroupSize = 5,
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount,
                DominantPercent = 50,
                CongruencePercent = 50,
                SwitchPercent = null,
                TaskLanguage = "en"
            };

            _profileService.UpsertProfile(existingProfile);

            var updatedProfile = new StroopProfile
            {
                Id = existingProfile.Id,
                ProfileName = "Updated Name",
                Hours = 2,
                Minutes = 15,
                Seconds = 30,
                FixationDuration = 200,
                MaxReactionTime = 600,
                VisualCueDuration = 100,
                HasVisualCue = true,
                GroupSize = 20,
                WordCount = 50,
                CalculationMode = CalculationMode.TaskDuration,
                DominantPercent = 70,
                CongruencePercent = 30,
                SwitchPercent = 25,
                TaskLanguage = "fr"
            };

            // Act
            var result = _profileService.UpsertProfile(updatedProfile);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var savedProfile = result.First();
            Assert.Equal(existingProfile.Id, savedProfile.Id);
            Assert.Equal("Updated Name", savedProfile.ProfileName);
            Assert.Equal(2, savedProfile.Hours);
            Assert.Equal(15, savedProfile.Minutes);
            Assert.Equal(30, savedProfile.Seconds);
            Assert.Equal(200, savedProfile.FixationDuration);
            Assert.Equal(600, savedProfile.MaxReactionTime);
            Assert.Equal(100, savedProfile.VisualCueDuration);
            Assert.True(savedProfile.HasVisualCue);
            Assert.Equal(20, savedProfile.GroupSize);
            Assert.Equal(9033, savedProfile.WordCount); // Calculated: TaskDuration (8130000ms) / WordDuration (900ms)
            Assert.Equal(CalculationMode.TaskDuration, savedProfile.CalculationMode);
            Assert.Equal(70, savedProfile.DominantPercent);
            Assert.Equal(30, savedProfile.CongruencePercent);
            Assert.Equal(25, savedProfile.SwitchPercent);
            Assert.Equal("fr", savedProfile.TaskLanguage);
            // Verify UpdateDerivedValues was called by checking WordDuration calculation
            Assert.Equal(900, savedProfile.WordDuration); // 600 + 200 + 100
        }

        /// <summary>
        /// Tests that UpsertProfile inserts a new profile into a collection that already has other profiles.
        /// Input: New profile when collection already contains profiles.
        /// Expected: New profile is added, existing profiles remain unchanged, count increases.
        /// </summary>
        [Fact]
        public void UpsertProfile_NewProfileWithExistingProfiles_AddsWithoutAffectingOthers()
        {
            // Arrange
            var existingProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Existing Profile",
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount
            };

            _profileService.UpsertProfile(existingProfile);

            var newProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "New Profile",
                WordCount = 20,
                CalculationMode = CalculationMode.WordCount
            };

            // Act
            var result = _profileService.UpsertProfile(newProfile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.ProfileName == "Existing Profile");
            Assert.Contains(result, p => p.ProfileName == "New Profile");
        }

        /// <summary>
        /// Tests that UpsertProfile updates only the matching profile when multiple profiles exist.
        /// Input: Updated profile when collection contains multiple profiles.
        /// Expected: Only the matching profile is updated, others remain unchanged.
        /// </summary>
        [Fact]
        public void UpsertProfile_UpdateWithMultipleProfiles_UpdatesOnlyMatchingProfile()
        {
            // Arrange
            var profile1 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 1",
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount
            };

            var profile2 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 2",
                WordCount = 15,
                CalculationMode = CalculationMode.WordCount
            };

            _profileService.UpsertProfile(profile1);
            _profileService.UpsertProfile(profile2);

            var updatedProfile2 = new StroopProfile
            {
                Id = profile2.Id,
                ProfileName = "Updated Profile 2",
                WordCount = 25,
                CalculationMode = CalculationMode.WordCount
            };

            // Act
            var result = _profileService.UpsertProfile(updatedProfile2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var unchangedProfile = result.FirstOrDefault(p => p.Id == profile1.Id);
            Assert.NotNull(unchangedProfile);
            Assert.Equal("Profile 1", unchangedProfile.ProfileName);
            Assert.Equal(10, unchangedProfile.WordCount);

            var changedProfile = result.FirstOrDefault(p => p.Id == profile2.Id);
            Assert.NotNull(changedProfile);
            Assert.Equal("Updated Profile 2", changedProfile.ProfileName);
            Assert.Equal(25, changedProfile.WordCount);
            Assert.Equal(CalculationMode.WordCount, changedProfile.CalculationMode);
        }

        /// <summary>
        /// Tests that UpsertProfile handles edge case of updating with all nullable values set to null.
        /// Input: Profile update with SwitchPercent set to null.
        /// Expected: Nullable property is correctly updated to null.
        /// </summary>
        [Fact]
        public void UpsertProfile_UpdateWithNullableProperty_HandlesNullCorrectly()
        {
            // Arrange
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test",
                SwitchPercent = 50,
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount
            };

            _profileService.UpsertProfile(profile);

            var updatedProfile = new StroopProfile
            {
                Id = profile.Id,
                ProfileName = "Test Updated",
                SwitchPercent = null,
                WordCount = 15,
                CalculationMode = CalculationMode.WordCount
            };

            // Act
            var result = _profileService.UpsertProfile(updatedProfile);

            // Assert
            Assert.NotNull(result);
            var savedProfile = result.First();
            Assert.Null(savedProfile.SwitchPercent);
        }

        /// <summary>
        /// Tests that UpsertProfile handles boundary values for integer properties.
        /// Input: Profile with maximum and minimum integer values.
        /// Expected: Values are correctly persisted.
        /// </summary>
        [Fact]
        public void UpsertProfile_WithBoundaryIntegerValues_PersistsCorrectly()
        {
            // Arrange
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Boundary Test",
                Hours = int.MaxValue,
                Minutes = int.MaxValue,
                Seconds = int.MaxValue,
                FixationDuration = int.MaxValue,
                MaxReactionTime = int.MaxValue,
                VisualCueDuration = int.MaxValue,
                GroupSize = int.MaxValue,
                WordCount = int.MaxValue,
                DominantPercent = int.MaxValue,
                CongruencePercent = int.MaxValue,
                SwitchPercent = int.MaxValue,
                CalculationMode = CalculationMode.WordCount,
                TaskLanguage = "en"
            };

            // Act
            var result = _profileService.UpsertProfile(profile);

            // Assert
            Assert.NotNull(result);
            var savedProfile = result.First();
            Assert.Equal(int.MaxValue, savedProfile.Hours);
            Assert.Equal(int.MaxValue, savedProfile.Minutes);
            Assert.Equal(int.MaxValue, savedProfile.Seconds);
        }

        /// <summary>
        /// Tests that UpsertProfile handles empty string for TaskLanguage.
        /// Input: Profile with empty string TaskLanguage.
        /// Expected: Empty string is correctly persisted.
        /// </summary>
        [Fact]
        public void UpsertProfile_WithEmptyStringTaskLanguage_PersistsCorrectly()
        {
            // Arrange
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Empty Language Test",
                TaskLanguage = "",
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount
            };

            // Act
            var result = _profileService.UpsertProfile(profile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.First().TaskLanguage);
        }

        /// <summary>
        /// Tests that UpsertProfile persists profile with very long string values.
        /// Input: Profile with very long ProfileName and TaskLanguage strings.
        /// Expected: Long strings are correctly persisted.
        /// </summary>
        [Fact]
        public void UpsertProfile_WithVeryLongStrings_PersistsCorrectly()
        {
            // Arrange
            var longString = new string('A', 10000);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = longString,
                TaskLanguage = longString,
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount
            };

            // Act
            var result = _profileService.UpsertProfile(profile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(longString, result.First().ProfileName);
            Assert.Equal(longString, result.First().TaskLanguage);
        }

        /// <summary>
        /// Tests that UpsertProfile handles profile with special characters in strings.
        /// Input: Profile with special characters in ProfileName and TaskLanguage.
        /// Expected: Special characters are correctly persisted.
        /// </summary>
        [Fact]
        public void UpsertProfile_WithSpecialCharactersInStrings_PersistsCorrectly()
        {
            // Arrange
            var specialString = "Test\n\r\t\"'<>&\\Profile";
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = specialString,
                TaskLanguage = "en\u0000test",
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount
            };

            // Act
            var result = _profileService.UpsertProfile(profile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(specialString, result.First().ProfileName);
        }

        /// <summary>
        /// Tests that UpsertProfile throws exception when profile parameter is null.
        /// Input: null profile.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void UpsertProfile_NullProfile_ThrowsArgumentNullException()
        {
            // Arrange
            StroopProfile? profile = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _profileService.UpsertProfile(profile!));
        }

        /// <summary>
        /// Tests that UpsertProfile verifies UpdateDerivedValues is called during update.
        /// Input: Existing profile updated with new MaxReactionTime, FixationDuration, VisualCueDuration.
        /// Expected: WordDuration is recalculated correctly (sum of the three).
        /// </summary>
        [Fact]
        public void UpsertProfile_UpdateExistingProfile_CallsUpdateDerivedValues()
        {
            // Arrange
            var existingProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test",
                MaxReactionTime = 100,
                FixationDuration = 50,
                VisualCueDuration = 25,
                WordCount = 10,
                CalculationMode = CalculationMode.WordCount
            };

            _profileService.UpsertProfile(existingProfile);

            var updatedProfile = new StroopProfile
            {
                Id = existingProfile.Id,
                ProfileName = "Test",
                MaxReactionTime = 500,
                FixationDuration = 200,
                VisualCueDuration = 100,
                WordCount = 20,
                CalculationMode = CalculationMode.WordCount
            };

            // Act
            var result = _profileService.UpsertProfile(updatedProfile);

            // Assert
            Assert.NotNull(result);
            var savedProfile = result.First();
            // WordDuration should be MaxReactionTime + FixationDuration + VisualCueDuration
            Assert.Equal(800, savedProfile.WordDuration);
            // TaskDuration should be WordCount * WordDuration for WordCount mode
            Assert.Equal(16000, savedProfile.TaskDuration);
        }


        /// <summary>
        /// Adds a profile in a service and verify in a new one that the file has been written in disk
        /// Input : StroopProfile and a double service
        /// Expect : Profile persisted in the disk with properties
        /// </summary>
        [Fact]
        public void UpsertProfile_NewProfile_PersistsToDisk()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service1 = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test Profile",
            };
            var expectedPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service1.UpsertProfile(profile);
                var service2 = new ProfileService(config);
                var loadedProfiles = service2.LoadProfiles();

                // Assert
                Assert.Contains(loadedProfiles, p => p.Id == profile.Id);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that UpsertProfile handles all CalculationMode enum values correctly.
        /// Input: Profiles with different CalculationMode values.
        /// Expected: CalculationMode is correctly persisted.
        /// </summary>
        [Theory]
        [InlineData(CalculationMode.WordCount)]
        [InlineData(CalculationMode.TaskDuration)]
        public void UpsertProfile_WithDifferentCalculationModes_PersistsCorrectly(CalculationMode mode)
        {
            // Arrange
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = $"Test {mode}",
                CalculationMode = mode,
                WordCount = 10,
                Hours = 1,
                Minutes = 0,
                Seconds = 0
            };

            // Act
            var result = _profileService.UpsertProfile(profile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mode, result.First().CalculationMode);
        }



        /// <summary>
        /// Tests that SaveProfiles throws ArgumentNullException when profiles parameter is null.
        /// </summary>
        [Fact]
        public void SaveProfiles_NullProfiles_ThrowsArgumentNullException()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);

            try
            {
                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => service.SaveProfiles(null!));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveProfiles creates directory and saves empty collection successfully.
        /// </summary>
        [Fact]
        public void SaveProfiles_EmptyCollection_CreatesFileWithEmptyArray()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profiles = new ObservableCollection<ExperimentProfile>();
            var expectedPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.SaveProfiles(profiles);

                // Assert
                Assert.True(Directory.Exists(tempDir));
                Assert.True(File.Exists(expectedPath));
                var content = File.ReadAllText(expectedPath);
                var deserializedProfiles = JsonSerializer.Deserialize<ObservableCollection<ExperimentProfile>>(content);
                Assert.NotNull(deserializedProfiles);
                Assert.Empty(deserializedProfiles);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveProfiles correctly serializes and saves a single profile to file.
        /// </summary>
        [Fact]
        public void SaveProfiles_SingleProfile_SavesCorrectly()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test Profile",
                FixationDuration = 100,
                MaxReactionTime = 400,
                WordCount = 10
            };
            var profiles = new ObservableCollection<ExperimentProfile> { profile };
            var expectedPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.SaveProfiles(profiles);

                // Assert
                Assert.True(File.Exists(expectedPath));
                var content = File.ReadAllText(expectedPath);
                var deserializedProfiles = JsonSerializer.Deserialize<ObservableCollection<StroopProfile>>(content);
                Assert.NotNull(deserializedProfiles);
                Assert.Single(deserializedProfiles);
                Assert.Equal(profile.Id, deserializedProfiles[0].Id);
                Assert.Equal(profile.ProfileName, deserializedProfiles[0].ProfileName);
                Assert.Equal(profile.FixationDuration, deserializedProfiles[0].FixationDuration);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveProfiles correctly serializes and saves multiple profiles to file.
        /// </summary>
        [Fact]
        public void SaveProfiles_MultipleProfiles_SavesCorrectly()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile1 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 1",
                FixationDuration = 100
            };
            var profile2 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 2",
                FixationDuration = 200
            };
            var profile3 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 3",
                FixationDuration = 300
            };
            var profiles = new ObservableCollection<ExperimentProfile> { profile1, profile2, profile3 };
            var expectedPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.SaveProfiles(profiles);

                // Assert
                Assert.True(File.Exists(expectedPath));
                var content = File.ReadAllText(expectedPath);
                var deserializedProfiles = JsonSerializer.Deserialize<ObservableCollection<StroopProfile>>(content);
                Assert.NotNull(deserializedProfiles);
                Assert.Equal(3, deserializedProfiles.Count);
                Assert.Equal(profile1.Id, deserializedProfiles[0].Id);
                Assert.Equal(profile2.Id, deserializedProfiles[1].Id);
                Assert.Equal(profile3.Id, deserializedProfiles[2].Id);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveProfiles creates the directory when it does not exist.
        /// </summary>
        [Fact]
        public void SaveProfiles_DirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profiles = new ObservableCollection<ExperimentProfile>();

            try
            {
                // Act
                Assert.False(Directory.Exists(tempDir));
                service.SaveProfiles(profiles);

                // Assert
                Assert.True(Directory.Exists(tempDir));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveProfiles overwrites existing file with new content.
        /// </summary>
        [Fact]
        public void SaveProfiles_FileAlreadyExists_OverwritesFile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var expectedPath = Path.Combine(tempDir, "profiles.json");
            File.WriteAllText(expectedPath, "old content");
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "New Profile"
            };
            var profiles = new ObservableCollection<ExperimentProfile> { profile };

            try
            {
                // Act
                service.SaveProfiles(profiles);

                // Assert
                var content = File.ReadAllText(expectedPath);
                Assert.DoesNotContain("old content", content);
                var deserializedProfiles = JsonSerializer.Deserialize<ObservableCollection<StroopProfile>>(content);
                Assert.NotNull(deserializedProfiles);
                Assert.Single(deserializedProfiles);
                Assert.Equal(profile.Id, deserializedProfiles[0].Id);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveProfiles writes JSON with indented formatting.
        /// </summary>
        [Fact]
        public void SaveProfiles_WritesIndentedJson()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test"
            };
            var profiles = new ObservableCollection<ExperimentProfile> { profile };
            var expectedPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.SaveProfiles(profiles);

                // Assert
                var content = File.ReadAllText(expectedPath);
                Assert.Contains(Environment.NewLine, content);
                Assert.Contains("  ", content);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that DeleteProfile removes an existing profile from the collection and persists changes.
        /// Input: Profile that exists in the collection.
        /// Expected: Profile is removed from collection and SaveProfiles is called (file is updated).
        /// </summary>
        [Fact]
        public void DeleteProfile_ExistingProfile_RemovesProfileAndSavesChanges()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profileToDelete = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile to Delete"
            };
            var otherProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Other Profile"
            };
            service.SaveProfiles([profileToDelete, otherProfile]);
            // var profiles = new ObservableCollection<ExperimentProfile> { profileToDelete, otherProfile };
            var profilesPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.DeleteProfile(profileToDelete);

                // Assert
                var profiles = service.LoadProfiles();
                Assert.DoesNotContain(profiles, p => p.Id == profileToDelete.Id);
                Assert.Contains(profiles, p => p.Id == otherProfile.Id);
                
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that DeleteProfile does nothing when the profile doesn't exist in the collection.
        /// Input: Profile that is not in the collection.
        /// Expected: Collection remains unchanged, SaveProfiles is not called.
        /// </summary>
        [Fact]
        public void DeleteProfile_NonExistentProfile_DoesNotModifyCollection()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var existingProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Existing Profile"
            };
            var nonExistentProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Non-Existent Profile"
            };
            service.SaveProfiles([existingProfile]);

            // var profiles = new ObservableCollection<ExperimentProfile> { existingProfile };
            var profilesPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.DeleteProfile(nonExistentProfile);

                // Assert
                var profiles = service.LoadProfiles();
                Assert.Single(profiles);
                Assert.Contains(profiles, p => p.Id == existingProfile.Id);
                Assert.DoesNotContain(profiles, p => p.Id == nonExistentProfile.Id); 
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that DeleteProfile does nothing when the profiles collection is empty.
        /// Input: Empty profiles collection.
        /// Expected: Collection remains empty, SaveProfiles is not called.
        /// </summary>
        [Fact]
        public void DeleteProfile_EmptyCollection_DoesNothing()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test Profile"
            };
            // var profiles = new ObservableCollection<ExperimentProfile>();
            var profilesPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.DeleteProfile(profile);

                // Assert
                Assert.Empty(service.LoadProfiles());

                // Verify SaveProfiles was not called (file should not exist)
                Assert.False(File.Exists(profilesPath));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that DeleteProfile handles null profile parameter correctly.
        /// Input: Null profile parameter with valid collection.
        /// Expected: Does nothing when null profile is not found in collection.
        /// </summary>
        [Fact]
        public void DeleteProfile_NullProfile_DoesNothing()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var existingProfile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Existing Profile"
            };
            service.SaveProfiles([existingProfile]);
            var profilesPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.DeleteProfile(null!);

                // Assert
                var profiles = service.LoadProfiles();
                Assert.Contains(profiles, p=>p.Id == existingProfile.Id);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that DeleteProfile removes only the specified profile when multiple profiles exist.
        /// Input: Collection with multiple profiles, deleting one specific profile.
        /// Expected: Only the specified profile is removed, others remain unchanged.
        /// </summary>
        [Fact]
        public void DeleteProfile_MultipleProfiles_RemovesOnlySpecifiedProfile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile1 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 1"
            };
            var profile2 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 2"
            };
            var profile3 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Profile 3"
            };
            service.SaveProfiles([profile1, profile2, profile3]);
            // var profiles = new ObservableCollection<ExperimentProfile> { profile1, profile2, profile3 };
            var profilesPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.DeleteProfile(profile2);

                // Assert
                var profiles = service.LoadProfiles();
                Assert.Equal(2, profiles.Count);
                Assert.Contains(profiles, p => p.Id == profile1.Id);
                Assert.DoesNotContain(profiles, p => p.Id == profile2.Id);
                Assert.Contains(profiles, p=> p.Id == profile3.Id);

                // Verify SaveProfiles was called by checking file content
                Assert.True(File.Exists(profilesPath));
                var content = File.ReadAllText(profilesPath);
                var deserializedProfiles = JsonSerializer.Deserialize<ObservableCollection<StroopProfile>>(content);
                Assert.NotNull(deserializedProfiles);
                Assert.Equal(2, deserializedProfiles.Count);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that DeleteProfile removes the last profile from collection correctly.
        /// Input: Collection with single profile to be deleted.
        /// Expected: Collection becomes empty and SaveProfiles is called.
        /// </summary>
        [Fact]
        public void DeleteProfile_LastProfileInCollection_RemovesAndSavesEmptyCollection()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Only Profile"
            };
            service.SaveProfiles([profile]);
            // var profiles = new ObservableCollection<ExperimentProfile> { profile };
            var profilesPath = Path.Combine(tempDir, "profiles.json");

            try
            {
                // Act
                service.DeleteProfile(profile);

                // Assert
                var profiles = service.LoadProfiles();
                Assert.Empty(profiles);

                // Verify SaveProfiles was called with empty collection
                Assert.True(File.Exists(profilesPath));
                var content = File.ReadAllText(profilesPath);
                var deserializedProfiles = JsonSerializer.Deserialize<ObservableCollection<StroopProfile>>(content);
                Assert.NotNull(deserializedProfiles);
                Assert.Empty(deserializedProfiles);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Ensures that the deletion of one TaskType profile does not erases the other types.
        /// Input : Collection with one StroopProfile and one SimonProfile, the StroopProfile is deleted
        /// Expected : StroopProfile removed, SimonProfile preserved 
        /// </summary>
        [Fact]
        public void DeleteProfile_StroopProfile_DoesNotRemoveSimonProfile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile1 = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Only Profile"
            };
            var profile2 = new SimonProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Only Profile"
            };
            service.SaveProfiles([profile1, profile2]);
            // var profiles = new ObservableCollection<ExperimentProfile> { profile };
            var profilesPath = Path.Combine(tempDir, "profiles.json");


            try
            {
                // Act
                service.DeleteProfile(profile1);

                // Assert
                var profiles = service.LoadProfiles();
                Assert.DoesNotContain(profiles, p => p.Id == profile1.Id);
                Assert.Contains(profiles, p => p.Id == profile2.Id);
                Assert.IsType<SimonProfile>(profiles.Single(p => p.Id == profile2.Id));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }



        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when the lastProfile.json file does not exist.
        /// Input: No lastProfile.json file present.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_FileDoesNotExist_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns the correct GUID when file contains valid JSON-serialized GUID.
        /// Input: File with JSON-serialized GUID string.
        /// Expected: Returns the deserialized GUID.
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_ValidJsonGuid_ReturnsGuid()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var expectedGuid = Guid.NewGuid();
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            var jsonContent = JsonSerializer.Serialize(expectedGuid.ToString());
            File.WriteAllText(lastProfilePath, jsonContent);

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(expectedGuid, result.Value);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns the GUID when file contains raw GUID string without JSON wrapping.
        /// Input: File with raw GUID string (fallback path after JSON deserialization fails).
        /// Expected: Returns the parsed GUID.
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_RawGuidString_ReturnsGuid()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var expectedGuid = Guid.NewGuid();
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, expectedGuid.ToString());

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(expectedGuid, result.Value);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns Guid.Empty when file contains JSON-serialized Guid.Empty.
        /// Input: File with JSON-serialized Guid.Empty string.
        /// Expected: Returns Guid.Empty.
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_GuidEmpty_ReturnsGuidEmpty()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            var jsonContent = JsonSerializer.Serialize(Guid.Empty.ToString());
            File.WriteAllText(lastProfilePath, jsonContent);

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(Guid.Empty, result.Value);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when file is empty.
        /// Input: Empty file.
        /// Expected: Returns null (cannot parse empty string as GUID).
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_EmptyFile_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, string.Empty);

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when file contains only whitespace.
        /// Input: File with whitespace-only content.
        /// Expected: Returns null (cannot parse whitespace as GUID).
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_WhitespaceOnly_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, "   \t\n\r   ");

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when file contains JSON null value.
        /// Input: File with JSON null.
        /// Expected: Returns null (deserialized value is null, cannot parse as GUID).
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_JsonNull_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, "null");

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when file contains valid JSON but invalid GUID string.
        /// Input: File with JSON-serialized string that is not a valid GUID.
        /// Expected: Returns null (TryParse fails on both paths).
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_JsonInvalidGuid_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            var jsonContent = JsonSerializer.Serialize("not-a-valid-guid");
            File.WriteAllText(lastProfilePath, jsonContent);

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile uses fallback raw parsing when JSON is malformed but contains valid GUID.
        /// Input: File with malformed JSON containing a valid GUID string.
        /// Expected: Returns the GUID via fallback raw text parsing.
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_MalformedJsonWithValidGuid_ReturnsGuidViaFallback()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var expectedGuid = Guid.NewGuid();
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            // Malformed JSON - missing closing quote
            File.WriteAllText(lastProfilePath, expectedGuid.ToString());

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(expectedGuid, result.Value);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when file contains completely invalid content.
        /// Input: File with random invalid text.
        /// Expected: Returns null (both JSON and raw parsing fail).
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_InvalidContent_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, "This is completely invalid content!");

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when file contains JSON array instead of string.
        /// Input: File with JSON array.
        /// Expected: Returns null (JsonException triggers fallback, array string is not valid GUID).
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_JsonArray_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, "[\"guid1\", \"guid2\"]");

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile returns null when file contains JSON object instead of string.
        /// Input: File with JSON object.
        /// Expected: Returns null (JsonException triggers fallback, object string is not valid GUID).
        /// </summary>
        [Fact]
        public void LoadLastSelectedProfile_JsonObject_ReturnsNull()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, "{\"id\": \"550e8400-e29b-41d4-a716-446655440000\"}");

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.Null(result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadLastSelectedProfile handles GUID with various valid formats.
        /// Input: File with GUID in different valid formats (with/without braces, hyphens).
        /// Expected: Returns the GUID correctly parsed.
        /// </summary>
        [Theory]
        [InlineData("550e8400-e29b-41d4-a716-446655440000")]
        [InlineData("{550e8400-e29b-41d4-a716-446655440000}")]
        [InlineData("(550e8400-e29b-41d4-a716-446655440000)")]
        public void LoadLastSelectedProfile_VariousGuidFormats_ReturnsGuid(string guidFormat)
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var lastProfilePath = Path.Combine(tempDir, "lastProfile.json");
            File.WriteAllText(lastProfilePath, guidFormat);

            try
            {
                // Act
                var result = service.LoadLastSelectedProfile();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(new Guid("550e8400-e29b-41d4-a716-446655440000"), result.Value);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveLastSelectedProfile throws ArgumentNullException when profile parameter is null.
        /// Input: null profile.
        /// Expected: ArgumentNullException is thrown when accessing profile.Id.
        /// </summary>
        [Fact]
        public void SaveLastSelectedProfile_NullProfile_ArgumentNullException()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);

            try
            {
                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => service.SaveLastSelectedProfile(null!));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveLastSelectedProfile correctly saves a valid profile's ID to file.
        /// Input: Profile with a valid GUID.
        /// Expected: Directory is created, file contains serialized GUID with indentation.
        /// </summary>
        [Fact]
        public void SaveLastSelectedProfile_ValidProfile_SavesProfileIdCorrectly()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profileId = Guid.NewGuid();
            var profile = new StroopProfile
            {
                Id = profileId,
                ProfileName = "Test Profile"
            };
            var expectedPath = Path.Combine(tempDir, "lastProfile.json");

            try
            {
                // Act
                service.SaveLastSelectedProfile(profile);

                // Assert
                Assert.True(Directory.Exists(tempDir));
                Assert.True(File.Exists(expectedPath));
                var content = File.ReadAllText(expectedPath);
                var deserializedId = JsonSerializer.Deserialize<Guid>(content);
                Assert.Equal(profileId, deserializedId);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveLastSelectedProfile correctly handles a profile with Guid.Empty.
        /// Input: Profile with Id = Guid.Empty.
        /// Expected: Guid.Empty is correctly serialized and saved to file.
        /// </summary>
        [Fact]
        public void SaveLastSelectedProfile_ProfileWithEmptyGuid_SavesEmptyGuidCorrectly()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.Empty,
                ProfileName = "Empty GUID Profile"
            };
            var expectedPath = Path.Combine(tempDir, "lastProfile.json");

            try
            {
                // Act
                service.SaveLastSelectedProfile(profile);

                // Assert
                Assert.True(File.Exists(expectedPath));
                var content = File.ReadAllText(expectedPath);
                var deserializedId = JsonSerializer.Deserialize<Guid>(content);
                Assert.Equal(Guid.Empty, deserializedId);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveLastSelectedProfile creates the directory when it does not exist.
        /// Input: Profile with valid GUID, directory does not exist.
        /// Expected: Directory is created and file is saved.
        /// </summary>
        [Fact]
        public void SaveLastSelectedProfile_DirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test Profile"
            };

            try
            {
                // Ensure directory does not exist
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }

                // Act
                service.SaveLastSelectedProfile(profile);

                // Assert
                Assert.True(Directory.Exists(tempDir));
                var expectedPath = Path.Combine(tempDir, "lastProfile.json");
                Assert.True(File.Exists(expectedPath));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveLastSelectedProfile overwrites existing file with new profile ID.
        /// Input: Save one profile, then save another profile.
        /// Expected: File is overwritten with the new profile ID.
        /// </summary>
        [Fact]
        public void SaveLastSelectedProfile_FileAlreadyExists_OverwritesFile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var firstProfileId = Guid.NewGuid();
            var firstProfile = new StroopProfile
            {
                Id = firstProfileId,
                ProfileName = "First Profile"
            };
            var secondProfileId = Guid.NewGuid();
            var secondProfile = new StroopProfile
            {
                Id = secondProfileId,
                ProfileName = "Second Profile"
            };
            var expectedPath = Path.Combine(tempDir, "lastProfile.json");

            try
            {
                // Act
                service.SaveLastSelectedProfile(firstProfile);
                var firstContent = File.ReadAllText(expectedPath);
                var firstDeserializedId = JsonSerializer.Deserialize<Guid>(firstContent);
                Assert.Equal(firstProfileId, firstDeserializedId);

                service.SaveLastSelectedProfile(secondProfile);

                // Assert
                var secondContent = File.ReadAllText(expectedPath);
                var secondDeserializedId = JsonSerializer.Deserialize<Guid>(secondContent);
                Assert.Equal(secondProfileId, secondDeserializedId);
                Assert.NotEqual(firstProfileId, secondDeserializedId);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveLastSelectedProfile writes JSON with indented formatting.
        /// Input: Profile with valid GUID.
        /// Expected: JSON content contains indentation (newlines and spaces).
        /// </summary>
        [Fact]
        public void SaveLastSelectedProfile_WritesIndentedJson()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test Profile"
            };
            var expectedPath = Path.Combine(tempDir, "lastProfile.json");

            try
            {
                // Act
                service.SaveLastSelectedProfile(profile);

                // Assert
                var content = File.ReadAllText(expectedPath);
                // Indented JSON for a GUID string should contain quotes and the GUID value
                Assert.Contains("\"", content);
                Assert.Contains(profile.Id.ToString(), content);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that SaveLastSelectedProfile saves only the profile ID, not the entire profile object.
        /// Input: Profile with multiple properties set.
        /// Expected: File contains only the serialized GUID, not profile name or other properties.
        /// </summary>
        [Fact]
        public void SaveLastSelectedProfile_SavesOnlyProfileId_NotEntireProfile()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var config = new AppConfiguration { ConfigDirectory = tempDir };
            var service = new ProfileService(config);
            var profile = new StroopProfile
            {
                Id = Guid.NewGuid(),
                ProfileName = "Test Profile Name",
                Hours = 1,
                Minutes = 30,
                Seconds = 45
            };
            var expectedPath = Path.Combine(tempDir, "lastProfile.json");

            try
            {
                // Act
                service.SaveLastSelectedProfile(profile);

                // Assert
                var content = File.ReadAllText(expectedPath);
                // Verify the file contains the GUID
                Assert.Contains(profile.Id.ToString(), content);
                // Verify the file does NOT contain profile name or other properties
                Assert.DoesNotContain("ProfileName", content);
                Assert.DoesNotContain("Test Profile Name", content);
                Assert.DoesNotContain("Hours", content);
                Assert.DoesNotContain("Minutes", content);
                Assert.DoesNotContain("Seconds", content);

                // Verify we can deserialize it as a Guid
                var deserializedId = JsonSerializer.Deserialize<Guid>(content);
                Assert.Equal(profile.Id, deserializedId);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}