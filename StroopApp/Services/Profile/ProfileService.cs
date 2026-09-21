using StroopApp.Models;
using StroopApp.Models.Simon;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace StroopApp.Services.Profile
{
    /// <summary>
    /// Service for managing experiment profile persistence (CRUD operations) and last selected profile tracking.
    /// </summary>
    public class ProfileService : IProfileService
	{
		private readonly string _configDir;
		private readonly string _profilesPath;
		private readonly string _lastProfileFile;

		public ProfileService(AppConfiguration configDir)
		{
            ArgumentNullException.ThrowIfNull(configDir);
            ArgumentNullException.ThrowIfNull(configDir.ConfigDirectory);
			_configDir = configDir.ConfigDirectory;
            _profilesPath = Path.Combine(_configDir, "profiles.json");
			_lastProfileFile = Path.Combine(_configDir, "lastProfile.json");
		}

        /// <summary>
        /// Loads all experiment profiles from JSON configuration file.
        /// </summary>
        public ObservableCollection<ExperimentProfile> LoadProfiles()
		{
			if (!File.Exists(_profilesPath))
				return new ObservableCollection<ExperimentProfile>();

			var json = File.ReadAllText(_profilesPath);
            var doc = JsonDocument.Parse(json);
			var profiles = new ObservableCollection<ExperimentProfile>();

			foreach (var element in doc.RootElement.EnumerateArray())
			{
                // if the TaskType attribute didn't exist, it was necessarily a Stroop task
                TaskType taskType = element.TryGetProperty("TaskType", out var taskTypeElement)
					? (TaskType)taskTypeElement.GetInt32() : TaskType.Stroop;

				var rawText = element.GetRawText();
				
				ExperimentProfile profile = taskType switch
				{
					TaskType.Stroop => JsonSerializer.Deserialize<StroopProfile>(rawText)!,
					_ => JsonSerializer.Deserialize<SimonProfile>(rawText)!,
				};
				profiles.Add(profile);
			}

			bool needsMigration = doc.RootElement.EnumerateArray().Any(e => !e.TryGetProperty("TaskType", out _));
			if (needsMigration)
				SaveProfiles(profiles);

			return profiles;

		}

        /// <summary>
        /// Saves all experiment profiles to JSON configuration file.
        /// </summary>
        public void SaveProfiles(ObservableCollection<ExperimentProfile> profiles)
        {
			ArgumentNullException.ThrowIfNull(profiles);

            if (!string.IsNullOrWhiteSpace(_configDir))
            {
                Directory.CreateDirectory(_configDir);
            }

			var elements = profiles.Select(p => JsonSerializer.SerializeToElement(p, p.GetType()));

            var json = JsonSerializer.Serialize(elements, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_profilesPath, json);
        }

        /// <summary>
        /// Inserts a new profile or updates an existing one by ID, then returns the refreshed collection.
        /// </summary>
        public ObservableCollection<ExperimentProfile> UpsertProfile(ExperimentProfile profile)
		{
            ArgumentNullException.ThrowIfNull(profile);

            var allProfiles = LoadProfiles();
			var existing = allProfiles.FirstOrDefault(p => p.Id == profile.Id);

			if (existing == null)
			{
				if (profile.Id == Guid.Empty)
					profile.Id = Guid.NewGuid();
				allProfiles.Add(profile);
			}
			else
			{
				existing.UpdateFrom(profile);
				existing.UpdateDerivedValues();
			}

			SaveProfiles(allProfiles);
			return LoadProfiles();
		}

        /// <summary>
        /// Deletes a profile from the collection and persists changes.
        /// </summary>
        public void DeleteProfile(ExperimentProfile profile)
		{
			ObservableCollection<ExperimentProfile> profiles = LoadProfiles();
            ArgumentNullException.ThrowIfNull(profiles);
            if (profile == null) return;
			ExperimentProfile? existingProfile = profiles.FirstOrDefault(p => p.Id == profile.Id);
			if (existingProfile is not null)
			{
				profiles.Remove(existingProfile);
				SaveProfiles(profiles);
			}
		}

        /// <summary>
        /// Loads the last selected profile ID from configuration, or null if not found.
        /// </summary>
        public Guid? LoadLastSelectedProfile()
		{
			if (File.Exists(_lastProfileFile))
			{
				var text = File.ReadAllText(_lastProfileFile);
				try
				{
					var guidString = JsonSerializer.Deserialize<string>(text);
					if (Guid.TryParse(guidString, out var id))
						return id;
				}
				catch (JsonException)
				{
					if (Guid.TryParse(text, out var id))
						return id;
				}
			}
			return null;
		}

        /// <summary>
        /// Saves the currently selected profile ID to configuration.
        /// </summary>
        public void SaveLastSelectedProfile(ExperimentProfile profile)
        {
			ArgumentNullException.ThrowIfNull(profile);

            if (!string.IsNullOrWhiteSpace(_configDir))
            {
                Directory.CreateDirectory(_configDir);	
            }

            var json = JsonSerializer.Serialize(profile.Id, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_lastProfileFile, json);
        }
    }
}
