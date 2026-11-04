using System.Globalization;
using System.IO;
using System.Text.Json;

using StroopApp.Resources;

namespace StroopApp.Services.Language
{
    /// <summary>
    /// Service for managing application language settings and retrieving localized strings.
    /// Persists language preference in AppData.
    /// </summary>
    public class LanguageService : ILanguageService
	{
		private const string ConfigFileName = "language.json";

		private readonly string _configPath;
		private AppConfig _config;
        /// <summary>
        /// Gets the current language code (e.g., "en", "fr").
        /// </summary>
        public string CurrentLanguageCode => _config.Language;
		public LanguageService()
		{
			_configPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"StroopApp",
				ConfigFileName);

			_config = LoadConfig();
			ApplyCulture(_config.Language);
		}
        /// <summary>
        /// Sets the application language and persists the preference.
        /// </summary>
        public void SetLanguage(string languageCode)
		{
			if (string.IsNullOrWhiteSpace(languageCode))
				return;

			ApplyCulture(languageCode);
			_config.Language = languageCode;
			SaveConfig();
		}

		private void ApplyCulture(string languageCode)
		{
			var culture = new CultureInfo(languageCode);
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
			Strings.Culture = culture;
		}

		private AppConfig LoadConfig()
		{
			if (!File.Exists(_configPath))
				return new AppConfig();

			try
			{
				var json = File.ReadAllText(_configPath);
				return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
			}
			catch
			{
				return new AppConfig();
			}
		}

		private void SaveConfig()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
			var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(_configPath, json);
		}
        /// <summary>
        /// Retrieves a localized string for the given resource key.
        /// Falls back to current UI culture, then to the key itself if not found.
        /// </summary>
        public string GetLocalizedString(string resourceKey, string? cultureCode = null)
		{
			CultureInfo targetCulture;

			if (string.IsNullOrWhiteSpace(cultureCode))
			{
				targetCulture = Strings.Culture ?? Thread.CurrentThread.CurrentUICulture;
			}
			else
			{
				targetCulture = new CultureInfo(cultureCode);
			}

			return Strings.ResourceManager.GetString(resourceKey, targetCulture)
				   ?? Strings.ResourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture)
				   ?? resourceKey;
		}
		private class AppConfig
		{
			public string Language { get; set; } = "en";
		}
	}
}
