namespace StroopApp.Services.Trial
{
	/// <summary>
	/// Minimal interface for trial generation configuration.
	/// </summary>
	public interface ITrialConfiguration
	{
		/// <summary>
		/// Current block number.
		/// </summary>
		int Block { get; }

		/// <summary>
		/// Participant identifier.
		/// </summary>
		string ParticipantId { get; }

		/// <summary>
		/// Total number of words/trials to generate.
		/// </summary>
		int WordCount { get; }

		/// <summary>
		/// Percentage of congruent trials (0-100).
		/// </summary>
		int CongruencePercent { get; }

		/// <summary>
		/// Percentage of trials with a round visual cue (0-100).
		/// </summary>
		int DominantPercent { get; }

		/// <summary>
		/// Percentage of transitions where the visual cue changes (0-100), null for no constraint.
		/// </summary>
		int? SwitchPercent { get; }

		/// <summary>
		/// Whether visual cue is enabled.
		/// </summary>
		bool HasVisualCue { get; }

		/// <summary>
		/// Language code for task stimuli (e.g., "en", "fr").
		/// </summary>
		string TaskLanguage { get; }
	}
}
