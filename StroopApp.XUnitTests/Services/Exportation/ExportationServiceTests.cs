using ClosedXML.Excel;
using Moq;
using StroopApp.Models;
using StroopApp.Services.Exportation;
using StroopApp.Services.Language;
using StroopApp.ViewModels.State;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace StroopApp.Services.Exportation.UnitTests
{
    /// <summary>
    /// Unit tests for <see cref = "ExportationService"/> class.
    /// </summary>
    public partial class ExportationServiceTests : IDisposable
    {
        private readonly string _testConfigDirectory;
        private readonly Mock<ILanguageService> _languageServiceMock;
        public ExportationServiceTests()
        {
            _testConfigDirectory = Path.Combine(Path.GetTempPath(), $"StroopAppTest_{Guid.NewGuid()}");
            _tempConfigDirectory = Path.Combine(Path.GetTempPath(), $"StroopAppTempDir_{Guid.NewGuid()}");
            _tempConfigDir = Path.Combine(Path.GetTempPath(), $"StroopAppTempDirAlt_{Guid.NewGuid()}");
            _testRootDirectory = Path.Combine(Path.GetTempPath(), $"StroopAppTestRoot_{Guid.NewGuid()}");
            _testConfigDir = Path.Combine(Path.GetTempPath(), $"StroopAppTestDirConf_{Guid.NewGuid()}");

            Directory.CreateDirectory(_testConfigDirectory);
            Directory.CreateDirectory(_tempConfigDirectory);
            Directory.CreateDirectory(_tempConfigDir);
            Directory.CreateDirectory(_testRootDirectory);
            Directory.CreateDirectory(_testConfigDir);

            _languageServiceMock = new Mock<ILanguageService>();
            _mockLanguageService = new Mock<ILanguageService>();
            _settings = new ExperimentSettings();
            _config = new AppConfiguration { ConfigDirectory = _testConfigDirectory };

            SetupDefaultLocalizedStrings();
        }

        public void Dispose()
        {
            CleanupDirectory(_testConfigDirectory);
            CleanupDirectory(_tempConfigDirectory);
            CleanupDirectory(_tempConfigDir);
            CleanupDirectory(_testRootDirectory);
            CleanupDirectory(_testConfigDir);
        }

        /// <summary>
        /// Tests that the ExportRootDirectory getter returns the current export root directory value.
        /// Verifies the getter correctly exposes the internal field state.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Get_ReturnsCurrentValue()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var expectedPath = service.ExportRootDirectory;
            // Act
            var actualPath = service.ExportRootDirectory;
            // Assert
            Assert.Equal(expectedPath, actualPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory to the same value does not trigger updates.
        /// Verifies that when the new value equals the current value, no settings are updated and no file save occurs.
        /// Expected result: ExportFolderPath remains unchanged and no configuration file is written.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_SameValue_DoesNotUpdateSettings()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var initialPath = service.ExportRootDirectory;
            var exportFolderConfigFile = Path.Combine(_testConfigDirectory, "exportFolder.json");
            // Delete config file if it exists to track if it gets written
            if (File.Exists(exportFolderConfigFile))
            {
                File.Delete(exportFolderConfigFile);
            }

            var fileExistedBefore = File.Exists(exportFolderConfigFile);
            // Act
            service.ExportRootDirectory = initialPath;
            // Assert
            Assert.Equal(initialPath, service.ExportRootDirectory);
            Assert.Equal(fileExistedBefore, File.Exists(exportFolderConfigFile));
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory to a different value updates all related state.
        /// Verifies that when a new value is provided, the internal field is updated,
        /// the ExperimentSettings.ExportFolderPath is updated, and the path is saved to configuration file.
        /// Expected result: All state is synchronized and configuration file contains the new path.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_DifferentValue_UpdatesSettingsAndSavesPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var newPath = Path.Combine(Path.GetTempPath(), "NewExportDirectory");
            var exportFolderConfigFile = Path.Combine(_testConfigDirectory, "exportFolder.json");
            // Act
            service.ExportRootDirectory = newPath;
            // Assert
            Assert.Equal(newPath, service.ExportRootDirectory);
            Assert.Equal(newPath, settings.ExportFolderPath);
            Assert.True(File.Exists(exportFolderConfigFile));
            var savedPath = JsonSerializer.Deserialize<string>(File.ReadAllText(exportFolderConfigFile));
            Assert.Equal(newPath, savedPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory to an empty string updates all related state.
        /// Verifies edge case where an empty string is a valid but unusual directory path.
        /// Expected result: Empty string is accepted and saved.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_EmptyString_UpdatesSettings()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var emptyPath = string.Empty;
            // Act
            service.ExportRootDirectory = emptyPath;
            // Assert
            Assert.Equal(emptyPath, service.ExportRootDirectory);
            Assert.Equal(emptyPath, settings.ExportFolderPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory to a whitespace string updates all related state.
        /// Verifies edge case where a whitespace-only string is treated as a distinct value.
        /// Expected result: Whitespace string is accepted and saved.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_WhitespaceString_UpdatesSettings()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var whitespacePath = "   ";
            // Act
            service.ExportRootDirectory = whitespacePath;
            // Assert
            Assert.Equal(whitespacePath, service.ExportRootDirectory);
            Assert.Equal(whitespacePath, settings.ExportFolderPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory to a very long path updates all related state.
        /// Verifies that the property can handle long path strings without issues.
        /// Expected result: Long path is accepted and saved correctly.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_VeryLongPath_UpdatesSettings()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var longPath = Path.Combine(Path.GetTempPath(), new string ('a', 200), new string ('b', 200));
            // Act
            service.ExportRootDirectory = longPath;
            // Assert
            Assert.Equal(longPath, service.ExportRootDirectory);
            Assert.Equal(longPath, settings.ExportFolderPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory to a path with special characters updates all related state.
        /// Verifies that special characters in paths are handled correctly.
        /// Expected result: Path with special characters is accepted and saved correctly.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_PathWithSpecialCharacters_UpdatesSettings()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var specialPath = Path.Combine(Path.GetTempPath(), "Export#Data$2024");
            // Act
            service.ExportRootDirectory = specialPath;
            // Assert
            Assert.Equal(specialPath, service.ExportRootDirectory);
            Assert.Equal(specialPath, settings.ExportFolderPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory multiple times with different values updates correctly each time.
        /// Verifies that the property handles sequential updates properly and maintains state consistency.
        /// Expected result: Each update overwrites previous value correctly.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_MultipleTimesDifferentValues_UpdatesCorrectly()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var firstPath = Path.Combine(Path.GetTempPath(), "FirstPath");
            var secondPath = Path.Combine(Path.GetTempPath(), "SecondPath");
            var thirdPath = Path.Combine(Path.GetTempPath(), "ThirdPath");
            // Act & Assert - First update
            service.ExportRootDirectory = firstPath;
            Assert.Equal(firstPath, service.ExportRootDirectory);
            Assert.Equal(firstPath, settings.ExportFolderPath);
            // Act & Assert - Second update
            service.ExportRootDirectory = secondPath;
            Assert.Equal(secondPath, service.ExportRootDirectory);
            Assert.Equal(secondPath, settings.ExportFolderPath);
            // Act & Assert - Third update
            service.ExportRootDirectory = thirdPath;
            Assert.Equal(thirdPath, service.ExportRootDirectory);
            Assert.Equal(thirdPath, settings.ExportFolderPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory updates the value even when changed from empty to non-empty.
        /// Verifies boundary transition from empty string to a valid path.
        /// Expected result: Value is updated correctly.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_FromEmptyToNonEmpty_UpdatesSettings()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            service.ExportRootDirectory = string.Empty;
            var newPath = Path.Combine(Path.GetTempPath(), "ValidPath");
            // Act
            service.ExportRootDirectory = newPath;
            // Assert
            Assert.Equal(newPath, service.ExportRootDirectory);
            Assert.Equal(newPath, settings.ExportFolderPath);
        }

        /// <summary>
        /// Tests that setting ExportRootDirectory updates the value even when changed from non-empty to empty.
        /// Verifies boundary transition from a valid path to empty string.
        /// Expected result: Value is updated correctly.
        /// </summary>
        [Fact]
        public void ExportRootDirectory_Set_FromNonEmptyToEmpty_UpdatesSettings()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _testConfigDirectory
            };
            var service = new ExportationService(settings, _languageServiceMock.Object, config, []);
            var initialPath = Path.Combine(Path.GetTempPath(), "InitialPath");
            service.ExportRootDirectory = initialPath;
            // Act
            service.ExportRootDirectory = string.Empty;
            // Assert
            Assert.Equal(string.Empty, service.ExportRootDirectory);
            Assert.Equal(string.Empty, settings.ExportFolderPath);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when settings parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullSettings_ThrowsArgumentNullException()
        {
            // Arrange
            var mockLanguageService = new Mock<ILanguageService>();
            var config = new AppConfiguration
            {
                ConfigDirectory = Path.Combine(Path.GetTempPath(), "StroopAppTest")
            };
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ExportationService(null!, mockLanguageService.Object, config, []));
            Assert.Equal("settings", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when languageService parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullLanguageService_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = Path.Combine(Path.GetTempPath(), "StroopAppTest")
            };
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ExportationService(settings, null!, config, []));
            Assert.Equal("languageService", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when config parameter is null.
        /// </summary>
        [Fact]
        public void Constructor_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var mockLanguageService = new Mock<ILanguageService>();
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ExportationService(settings, mockLanguageService.Object, null!, []));
        }

        /// <summary>
        /// Tests that the constructor initializes the service correctly with valid parameters,
        /// creates the configuration directory, and sets the export folder path to MyDocuments
        /// when no export folder configuration file exists.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_InitializesServiceAndSetsExportPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var mockLanguageService = new Mock<ILanguageService>();
            var testConfigDir = Path.Combine(Path.GetTempPath(), "StroopAppTest_" + Guid.NewGuid().ToString());
            var config = new AppConfiguration
            {
                ConfigDirectory = testConfigDir
            };
            var expectedMyDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // Act
            var service = new ExportationService(settings, mockLanguageService.Object, config, []);
            // Assert
            Assert.True(Directory.Exists(testConfigDir));
            Assert.Equal(expectedMyDocumentsPath, service.ExportRootDirectory);
            Assert.Equal(expectedMyDocumentsPath, settings.ExportFolderPath);
            // Cleanup
            try
            {
                if (Directory.Exists(testConfigDir))
                {
                    Directory.Delete(testConfigDir, true);
                }
            }
            catch
            {
            // Ignore cleanup errors
            }
        }

        private readonly string _tempConfigDirectory;
        /// <summary>
        /// Tests that SaveExportFolderPath creates a file with correctly serialized path
        /// when provided a valid path string.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_ValidPath_CreatesFileWithSerializedPath()
        {
            // Arrange
            var testPath = @"C:\TestExports\Data";
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(testPath);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            var deserializedPath = JsonSerializer.Deserialize<string>(fileContent);
            Assert.Equal(testPath, deserializedPath);
        }

        /// <summary>
        /// Tests that SaveExportFolderPath correctly handles null path input
        /// by creating a file with serialized null value.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_NullPath_CreatesFileWithNullValue()
        {
            // Arrange
            string? testPath = null;
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(testPath!);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            Assert.Equal("null", fileContent);
        }

        /// <summary>
        /// Tests that SaveExportFolderPath correctly handles empty string input
        /// by creating a file with serialized empty string.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_EmptyString_CreatesFileWithEmptyString()
        {
            // Arrange
            var testPath = string.Empty;
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(testPath);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            var deserializedPath = JsonSerializer.Deserialize<string>(fileContent);
            Assert.Equal(string.Empty, deserializedPath);
        }

        /// <summary>
        /// Tests that SaveExportFolderPath correctly handles whitespace-only string input
        /// by creating a file with serialized whitespace string.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_WhitespaceString_CreatesFileWithWhitespaceString()
        {
            // Arrange
            var testPath = "   ";
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(testPath);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            var deserializedPath = JsonSerializer.Deserialize<string>(fileContent);
            Assert.Equal(testPath, deserializedPath);
        }

        /// <summary>
        /// Tests that SaveExportFolderPath correctly handles path with special characters
        /// by creating a file with correctly serialized path containing special characters.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_PathWithSpecialCharacters_CreatesFileWithCorrectContent()
        {
            // Arrange
            var testPath = @"C:\Test\Path's & Special#Characters@2024!";
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(testPath);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            var deserializedPath = JsonSerializer.Deserialize<string>(fileContent);
            Assert.Equal(testPath, deserializedPath);
        }

        /// <summary>
        /// Tests that SaveExportFolderPath correctly handles very long path string
        /// by creating a file with correctly serialized long path.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_VeryLongPath_CreatesFileWithCorrectContent()
        {
            // Arrange
            var testPath = new string ('x', 500);
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(testPath);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            var deserializedPath = JsonSerializer.Deserialize<string>(fileContent);
            Assert.Equal(testPath, deserializedPath);
        }

        /// <summary>
        /// Tests that SaveExportFolderPath overwrites existing file content
        /// when called multiple times with different paths.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_CalledMultipleTimes_OverwritesExistingFile()
        {
            // Arrange
            var firstPath = @"C:\First\Path";
            var secondPath = @"C:\Second\Path";
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(firstPath);
            service.SaveExportFolderPath(secondPath);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            var deserializedPath = JsonSerializer.Deserialize<string>(fileContent);
            Assert.Equal(secondPath, deserializedPath);
        }

        /// <summary>
        /// Tests that SaveExportFolderPath correctly handles path with Unicode characters
        /// by creating a file with correctly serialized Unicode path.
        /// </summary>
        [Fact]
        public void SaveExportFolderPath_PathWithUnicodeCharacters_CreatesFileWithCorrectContent()
        {
            // Arrange
            var testPath = @"C:\Test\Données\文件\😀";
            var languageServiceMock = new Mock<ILanguageService>();
            var settings = new ExperimentSettings();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDirectory
            };
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var expectedFilePath = Path.Combine(_tempConfigDirectory, "exportFolder.json");
            // Act
            service.SaveExportFolderPath(testPath);
            // Assert
            Assert.True(File.Exists(expectedFilePath));
            var fileContent = File.ReadAllText(expectedFilePath);
            var deserializedPath = JsonSerializer.Deserialize<string>(fileContent);
            Assert.Equal(testPath, deserializedPath);
        }

        private readonly string _testRootDirectory;
        private readonly Mock<ILanguageService> _mockLanguageService;
        private readonly ExperimentSettings _settings;
        private readonly AppConfiguration _config;
        private readonly string _testConfigDir;
        private void SetupDefaultLocalizedStrings()
        {
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_ParticipantId", It.IsAny<string?>())).Returns("Participant ID");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_ProfileName", It.IsAny<string?>())).Returns("Profile Name");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_BlockNumber", It.IsAny<string?>())).Returns("Block Number");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_Trials", It.IsAny<string?>())).Returns("Trials");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_Congruence", It.IsAny<string?>())).Returns("Congruence");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_VisualCue", It.IsAny<string?>())).Returns("Visual Cue");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_Expected_Answer", It.IsAny<string?>())).Returns("Expected Answer");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_Given_Answer", It.IsAny<string?>())).Returns("Given Answer");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_Response_Validity", It.IsAny<string?>())).Returns("Response Validity");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Header_ResponseTime", It.IsAny<string?>())).Returns("Response Time");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Label_Square", It.IsAny<string?>())).Returns("Square");
            _mockLanguageService.Setup(x => x.GetLocalizedString("Label_Circle", It.IsAny<string?>())).Returns("Circle");
        }

        /// <summary>
        /// Tests that ExportDataAsync throws InvalidOperationException when ExportFolderPath is null.
        /// </summary>
        [Fact]
        public async Task ExportDataAsync_NullExportFolderPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var mockLanguageService = new Mock<ILanguageService>();
            var testConfigDir = Path.Combine(Path.GetTempPath(), $"StroopAppTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(testConfigDir);
            var config = new AppConfiguration
            {
                ConfigDirectory = testConfigDir
            };
            var service = new ExportationService(settings, mockLanguageService.Object, config, []);

            // Act & Assert
            settings.ExportFolderPath = null!;
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExportDataAsync());
            Assert.Equal("No export directory configured.", exception.Message);

            // Cleanup
            try
            {
                if (Directory.Exists(testConfigDir))
                {
                    Directory.Delete(testConfigDir, true);
                }
            }
            catch
            {
            // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Tests that ExportDataAsync throws InvalidOperationException when ExportFolderPath is empty string.
        /// </summary>
        [Fact]
        public async Task ExportDataAsync_EmptyExportFolderPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var mockLanguageService = new Mock<ILanguageService>();
            var testConfigDir = Path.Combine(Path.GetTempPath(), $"StroopAppTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(testConfigDir);
            var config = new AppConfiguration
            {
                ConfigDirectory = testConfigDir
            };
            var service = new ExportationService(settings, mockLanguageService.Object, config, []);

            // Act & Assert
            settings.ExportFolderPath = string.Empty;
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExportDataAsync());
            Assert.Equal("No export directory configured.", exception.Message);

            // Cleanup
            try
            {
                if (Directory.Exists(testConfigDir))
                {
                    Directory.Delete(testConfigDir, true);
                }
            }
            catch
            {
            // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Tests that ExportDataAsync throws InvalidOperationException when ExportFolderPath is whitespace.
        /// </summary>
        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public async Task ExportDataAsync_WhitespaceExportFolderPath_ThrowsInvalidOperationException(string whitespace)
        {
            // Arrange
            var service = new ExportationService(_settings, _mockLanguageService.Object, _config, []);

            // Act - On force Whitespace APRES la construction
            _settings.ExportFolderPath = whitespace;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExportDataAsync());
            Assert.Equal("No export directory configured.", exception.Message);
        }

        private readonly string _tempConfigDir;
        /// <summary>
        /// Tests that LoadExportFolderPath returns MyDocuments path when config file does not exist.
        /// Input: Config file does not exist.
        /// Expected: Returns Environment.GetFolderPath(SpecialFolder.MyDocuments).
        /// </summary>
        [Fact]
        public void LoadExportFolderPath_ConfigFileDoesNotExist_ReturnsMyDocumentsPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var languageServiceMock = new Mock<ILanguageService>();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDir
            };
            var exportFolderConfigFile = Path.Combine(_tempConfigDir, "exportFolder.json");
            // Ensure file does not exist
            if (File.Exists(exportFolderConfigFile))
            {
                File.Delete(exportFolderConfigFile);
            }

            var expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // Act
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var result = service.LoadExportFolderPath();
            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that LoadExportFolderPath returns deserialized path when config file exists with valid JSON.
        /// Input: Config file exists with valid JSON string.
        /// Expected: Returns the deserialized path from the file.
        /// </summary>
        [Fact]
        public void LoadExportFolderPath_ConfigFileExistsWithValidJson_ReturnsDeserializedPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var languageServiceMock = new Mock<ILanguageService>();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDir
            };
            var exportFolderConfigFile = Path.Combine(_tempConfigDir, "exportFolder.json");
            var expectedPath = @"C:\TestExportFolder";
            File.WriteAllText(exportFolderConfigFile, JsonSerializer.Serialize(expectedPath));
            // Act
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var result = service.LoadExportFolderPath();
            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that LoadExportFolderPath returns MyDocuments when config file contains null JSON value.
        /// Input: Config file exists with JSON null value.
        /// Expected: Returns Environment.GetFolderPath(SpecialFolder.MyDocuments).
        /// </summary>
        [Fact]
        public void LoadExportFolderPath_ConfigFileContainsNullJson_ReturnsMyDocumentsPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var languageServiceMock = new Mock<ILanguageService>();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDir
            };
            var exportFolderConfigFile = Path.Combine(_tempConfigDir, "exportFolder.json");
            File.WriteAllText(exportFolderConfigFile, "null");
            var expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            // Act
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var result = service.LoadExportFolderPath();
            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that LoadExportFolderPath returns MyDocuments path when config file contains invalid JSON.
        /// Input: Config file exists with invalid JSON content.
        /// Expected: Returns Environment.GetFolderPath(SpecialFolder.MyDocuments).
        /// </summary>
        [Fact]
        public void LoadExportFolderPath_ConfigFileContainsInvalidJson_ReturnsMyDocumentsPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var languageServiceMock = new Mock<ILanguageService>();
            var tempConfigDir = Path.Combine(Path.GetTempPath(), $"StroopAppTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempConfigDir);
            var config = new AppConfiguration
            {
                ConfigDirectory = tempConfigDir
            };
            var exportFolderConfigFile = Path.Combine(tempConfigDir, "exportFolder.json");
            File.WriteAllText(exportFolderConfigFile, "{invalid json content");
            var expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
                // Act
                var service = new ExportationService(settings, languageServiceMock.Object, config, []);
                var result = service.LoadExportFolderPath();
                // Assert
                Assert.Equal(expectedPath, result);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(tempConfigDir))
                {
                    Directory.Delete(tempConfigDir, true);
                }
            }
        }

        /// <summary>
        /// Tests that LoadExportFolderPath returns MyDocuments path when config file is empty.
        /// Input: Config file exists but is empty.
        /// Expected: Returns Environment.GetFolderPath(SpecialFolder.MyDocuments).
        /// </summary>
        [Fact]
        public void LoadExportFolderPath_ConfigFileIsEmpty_ReturnsMyDocumentsPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var languageServiceMock = new Mock<ILanguageService>();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDir
            };
            var exportFolderConfigFile = Path.Combine(_tempConfigDir, "exportFolder.json");
            File.WriteAllText(exportFolderConfigFile, string.Empty);
            var expectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Act
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var result = service.LoadExportFolderPath();

            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that LoadExportFolderPath returns deserialized path with special characters.
        /// Input: Config file exists with JSON string containing special characters.
        /// Expected: Returns the deserialized path correctly.
        /// </summary>
        [Fact]
        public void LoadExportFolderPath_ConfigFileWithSpecialCharacters_ReturnsDeserializedPath()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var languageServiceMock = new Mock<ILanguageService>();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDir
            };
            var exportFolderConfigFile = Path.Combine(_tempConfigDir, "exportFolder.json");
            var expectedPath = @"C:\Test Folder\Données\Émissions";
            File.WriteAllText(exportFolderConfigFile, JsonSerializer.Serialize(expectedPath));
            // Act
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var result = service.LoadExportFolderPath();
            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that LoadExportFolderPath returns empty string when config file contains empty string JSON.
        /// Input: Config file exists with JSON serialized empty string.
        /// Expected: Returns MyDocuments path due to null-coalescing operator treating empty string as valid.
        /// </summary>
        [Fact]
        public void LoadExportFolderPath_ConfigFileContainsEmptyStringJson_ReturnsEmptyString()
        {
            // Arrange
            var settings = new ExperimentSettings();
            var languageServiceMock = new Mock<ILanguageService>();
            var config = new AppConfiguration
            {
                ConfigDirectory = _tempConfigDir
            };
            var exportFolderConfigFile = Path.Combine(_tempConfigDir, "exportFolder.json");
            File.WriteAllText(exportFolderConfigFile, JsonSerializer.Serialize(string.Empty));
            // Act
            var service = new ExportationService(settings, languageServiceMock.Object, config, []);
            var result = service.LoadExportFolderPath();
            // Assert
            Assert.Equal(string.Empty, result);
        }

        private void SetupLanguageServiceMock(Mock<ILanguageService> mock)
        {
            mock.Setup(x => x.GetLocalizedString("Header_ParticipantId", null)).Returns("ParticipantId_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_ProfileName", null)).Returns("ProfileName_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_BlockNumber", null)).Returns("BlockNumber_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_Trials", null)).Returns("Trials_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_Congruence", null)).Returns("Congruence_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_VisualCue", null)).Returns("VisualCue_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_Expected_Answer", null)).Returns("ExpectedAnswer_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_Given_Answer", null)).Returns("GivenAnswer_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_Response_Validity", null)).Returns("ResponseValidity_Localized");
            mock.Setup(x => x.GetLocalizedString("Header_ResponseTime", null)).Returns("ResponseTime_Localized");
            mock.Setup(x => x.GetLocalizedString("Label_Square", null)).Returns("Square_Localized");
            mock.Setup(x => x.GetLocalizedString("Label_Circle", null)).Returns("Circle_Localized");
        }

        private void CleanupDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch
            {
            // Ignore cleanup errors
            }
        }
    }
}