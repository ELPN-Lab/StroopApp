using System.IO;
using System.Text.Json;

using StroopApp.Models;

namespace StroopApp.Services.KeyMapping
{
    /// <summary>
    /// Service for loading and saving key mapping configuration from/to JSON files.
    /// </summary>
    public class KeyMappingService : IKeyMappingService
	{
		private readonly string _configDir;
		private readonly string _keyMappingPath;

		public KeyMappingService(AppConfiguration configDir)
		{
			_configDir = configDir.ConfigDirectory ?? throw new ArgumentNullException(nameof(configDir));
			_keyMappingPath = Path.Combine(_configDir, "keymappings.json");
		}
        /// <summary>
        /// Loads key mappings from configuration file, or returns default mappings if not found.
        /// </summary>
        public async Task<ExperimentKeyMappings> LoadKeyMappings()
		{
			if (!File.Exists(_keyMappingPath))
				return new ExperimentKeyMappings();

			var json = await File.ReadAllTextAsync(_keyMappingPath);
			return JsonSerializer.Deserialize<ExperimentKeyMappings>(json)
				   ?? new ExperimentKeyMappings();
		}
        /// <summary>
        /// Saves key mappings to configuration file.
        /// </summary>
        public async Task SaveKeyMappings(ExperimentKeyMappings keyMappings)
		{
			Directory.CreateDirectory(_configDir);
			var json = JsonSerializer.Serialize(keyMappings, new JsonSerializerOptions { WriteIndented = true });
			await File.WriteAllTextAsync(_keyMappingPath, json);
		}
	}
}
