using ClosedXML.Excel;
using StroopApp.Models;
using StroopApp.Services.Language;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;

namespace StroopApp.Services.Exportation
{
    /// <summary>
    /// Service for exporting experiment data to Excel files with localized headers.
    /// Manages export directory configuration and file generation.
    /// </summary>
    public class ExportationService : IExportationService
    {
        private readonly ExperimentSettings _settings;
        private readonly string _configDir;
        private readonly string _exportFolderConfigFile;
        private string _exportRootDirectory;
        private readonly ILanguageService _languageService;
        private readonly IEnumerable<TrialExportFormatter> _exportFormatters;

        public string ExportRootDirectory
        {
            get => _exportRootDirectory;
            set
            {
                if (_exportRootDirectory != value)
                {
                    _exportRootDirectory = value;
                    _settings.ExportFolderPath = value;
                    SaveExportFolderPath(value);
                }
            }
        }

        public ExportationService(
            ExperimentSettings settings, 
            ILanguageService languageService, 
            AppConfiguration config,
            IEnumerable<TrialExportFormatter> exportFormatters)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
            _exportFormatters = exportFormatters;

            ArgumentNullException.ThrowIfNull(config);
            ArgumentException.ThrowIfNullOrWhiteSpace(config.ConfigDirectory);
            _configDir = config.ConfigDirectory;


            if (!string.IsNullOrWhiteSpace(_configDir))
            {
                Directory.CreateDirectory(_configDir);
            }
            _exportFolderConfigFile = Path.Combine(_configDir, "exportFolder.json");
            _exportRootDirectory = LoadExportFolderPath();
            _settings.ExportFolderPath = _exportRootDirectory;
        }

        /// <summary>
        /// Loads the export folder path from configuration file, or returns MyDocuments if not found.
        /// </summary>
        public string LoadExportFolderPath()
        {
            if (!File.Exists(_exportFolderConfigFile))
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            try
            {
                var json = File.ReadAllText(_exportFolderConfigFile);
                if (string.IsNullOrWhiteSpace(json))
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                return JsonSerializer.Deserialize<string>(json)
                       ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            catch (JsonException)
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        /// <summary>
        /// Saves the export folder path to configuration file.
        /// </summary>
        public void SaveExportFolderPath(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    // Ignore les créations de dossier impossibles (caractères spéciaux dans les tests / paths trop longs)
                }
            }
            File.WriteAllText(_exportFolderConfigFile, JsonSerializer.Serialize(path));
        }

        /// <summary>
        /// Exports all experiment data to an Excel file in the configured directory structure.
        /// </summary>
        /// <returns>Full path to the generated Excel file.</returns>
        public async Task<string> ExportDataAsync()
        {
            var root = _settings.ExportFolderPath;
            if (string.IsNullOrWhiteSpace(root))
                throw new InvalidOperationException("No export directory configured.");

            Directory.CreateDirectory(root);
            var resultsDir = Path.Combine(root, "Results");
            var archived = Path.Combine(root, "Archived");
            Directory.CreateDirectory(resultsDir);
            Directory.CreateDirectory(archived);

            var p = _settings.Participant;
            var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            var partDir = Path.Combine(resultsDir, p.Id, dateFolder);
            Directory.CreateDirectory(partDir);

            var ts = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var filePath = Path.Combine(partDir, $"{p.Id}_{ts}.xlsx");

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Export");

            var formatter = GetFormatter();
            var headers = formatter.GetColumnHeaders();
            
            for (int col = 1; col <= headers.Count; col++)
                ws.Cell(1, col).Value = headers[col - 1];
            
            var row = 2;
            foreach(Block block in _settings.ExperimentContext.Blocks)
            {
                foreach(ITrial? trial in block.TrialRecords)
                {
                    if (trial is null) continue;
                    formatter.WriteRow(ws, row, trial, _settings.CurrentProfile.ProfileName, block.BlockNumber);
                    row++;
                }
            }
            

            await Task.Run(() => wb.SaveAs(filePath));
            return await Task.FromResult(filePath);
        }
        private TrialExportFormatter GetFormatter()
        {
            var taskType = _settings.CurrentProfile.TaskType;
            return _exportFormatters.FirstOrDefault(f => f.TaskType == taskType)
                ?? throw new InvalidOperationException($"Erreur : pas de formatteur pour la tâche {taskType}");
        }
    }
}