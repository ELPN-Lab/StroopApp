namespace StroopApp.Models
{
	/// <summary>
	/// Represents static configuration for an experiment.
	/// Contains profile settings, key mappings, and export folder path.
	/// This is a simple POCO without logic, extracted from ExperimentSettings.
	/// </summary>
	public class ExperimentConfiguration
	{
		/// <summary>
		/// Experiment profile containing durations, timings, and task parameters.
		/// </summary>
		public ExperimentProfile? Profile { get; set; }

		/// <summary>
		/// Key mappings for color responses.
		/// </summary>
		public ExperimentKeyMappings KeyMappings { get; set; }

		/// <summary>
		/// Folder path where experiment results will be exported.
		/// </summary>
		public string ExportFolderPath { get; set; }

		/// <summary>
		/// Default constructor initializing with empty/default values.
		/// </summary>
		public ExperimentConfiguration()
		{
			KeyMappings = new ExperimentKeyMappings();
			ExportFolderPath = "";
		}
	}
}
