using StroopApp.Models;
using StroopApp.Services.Language;
using StroopApp.ViewModels.State;
using System.Globalization;

namespace StroopApp.Services.Trial
{
    /// <summary>
    /// Service for generating randomized Stroop trial sequences with localized stimuli and optional visual cues.
    /// </summary>
    public class TrialGenerationService : ITrialGenerationService
    {
		private readonly Random _random = new Random();
		private readonly ILanguageService _languageService;

		public TrialGenerationService(ILanguageService languageService)
		{
			_languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
		}

		/// <summary>
		/// Legacy method for backward compatibility.
		/// Delegates to the interface-based overload via adapter.
		/// </summary>
		public List<ITrial> GenerateTrials(ExperimentSettingsViewModel settings)
		{
			if (settings?.CurrentProfile == null)
				throw new ArgumentException("Settings and  CurrentProfile cannot be null", nameof(settings));

			var config = new ExperimentSettingsTrialConfigurationAdapter(settings);
			return GenerateTrials(config);
		}

		/// <summary>
		/// Generates trials based on minimal configuration interface.
		/// This is the primary implementation.
		/// </summary>
		public List<ITrial> GenerateTrials(ITrialConfiguration config)
		{
			if (config == null)
				throw new ArgumentNullException(nameof(config));

			var trials = new List<ITrial>();
			var wordColors = new[] { "Blue", "Red", "Green", "Yellow" };

			var taskCultureCode = string.IsNullOrWhiteSpace(config.TaskLanguage)
								? Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
								: config.TaskLanguage;

			var taskCulture = new CultureInfo(taskCultureCode);

			var wordTexts = new[]
			{
				_languageService.GetLocalizedString("Word_BLUE", taskCultureCode),
				_languageService.GetLocalizedString("Word_RED", taskCultureCode),
				_languageService.GetLocalizedString("Word_GREEN", taskCultureCode),
				_languageService.GetLocalizedString("Word_YELLOW", taskCultureCode)
			};

			int total = config.WordCount;
			int congruentCount = total * config.CongruencePercent / 100;
			int incongruentCount = total - congruentCount;

			var congruenceFlags = new List<bool>();
			congruenceFlags.AddRange(Enumerable.Repeat(true, congruentCount));
			congruenceFlags.AddRange(Enumerable.Repeat(false, incongruentCount));
			congruenceFlags = congruenceFlags.OrderBy(_ => _random.Next()).ToList();

			List<VisualCueType>? visualCueSequence = null;
			if (config.HasVisualCue)
				visualCueSequence = GenerateVisualCueSequence(total, config.SwitchPercent, config.DominantPercent);

			for (int i = 0; i < total; i++)
			{
				var trial = new StroopTrial
				{
					TrialNumber = i + 1,
					Block = config.Block,
					ParticipantId = config.ParticipantId,
					HasVIsualCue = config.HasVisualCue,
					SwitchPercent = config.SwitchPercent,
					CongruencePercent = config.CongruencePercent,
					DominancePercent = config.DominantPercent,
				};

				bool isCongruent = congruenceFlags[i];

				if (isCongruent)
				{
					int idx = _random.Next(wordColors.Length);
					trial.Stimulus = new Word(wordColors[idx], wordColors[idx], wordTexts[idx]);
					trial.IsCongruent = true;
				}
				else
				{
					var indices = Enumerable.Range(0, wordColors.Length)
						.OrderBy(_ => _random.Next())
						.Take(2)
						.ToArray();
					trial.Stimulus = new Word(wordColors[indices[0]], wordColors[indices[1]], wordTexts[indices[1]]);
					trial.IsCongruent = false;
				}

				if (visualCueSequence != null)
					trial.VisualCue = visualCueSequence[i];
				trial.DetermineExpectedAnswer();

				trials.Add(trial);
			}

			return trials;
		}

        /// <summary>
        /// Generates a sequence of visual cues with an exact round cue count and an exact switch count.
        /// When both constraints cannot hold together, the round cue count wins and the switch count is clamped to the nearest feasible value.
        /// </summary>
        /// <param name="count">Number of cues to generate</param>
        /// <param name="switchPercentage">Percentage of the count - 1 transitions where the cue changes, null for a free shuffle</param>
        /// <param name="dominantPercentage">Percentage of round cues</param>
        /// <returns>Sequence of cues</returns>
        public List<VisualCueType> GenerateVisualCueSequence(int count, int? switchPercentage, int dominantPercentage)
		{
			if (count <= 0)
				throw new ArgumentException("Count of visual cues must be positive", nameof(count));

			int roundCount = count * Math.Clamp(dominantPercentage, 0, 100) / 100;
			int squareCount = count - roundCount;

			if (switchPercentage == null)
			{
				return Enumerable.Repeat(VisualCueType.Round, roundCount)
					.Concat(Enumerable.Repeat(VisualCueType.Square, squareCount))
					.OrderBy(_ => _random.Next())
					.ToList();
			}

			int minSwitches = roundCount > 0 && squareCount > 0 ? 1 : 0;
			int maxSwitches = roundCount == squareCount ? 2 * roundCount - 1 : 2 * Math.Min(roundCount, squareCount);
			int switchCount = Math.Clamp((count - 1) * Math.Clamp(switchPercentage.Value, 0, 100) / 100, minSwitches, maxSwitches);

			int runCount = switchCount + 1;
			int firstCueRuns = (runCount + 1) / 2;
			int secondCueRuns = runCount / 2;
			bool roundCanStart = roundCount >= firstCueRuns && squareCount >= secondCueRuns;
			bool squareCanStart = squareCount >= firstCueRuns && roundCount >= secondCueRuns;
			bool roundStarts = roundCanStart && (!squareCanStart || _random.Next(2) == 0);

			var firstCue = roundStarts ? VisualCueType.Round : VisualCueType.Square;
			var secondCue = roundStarts ? VisualCueType.Square : VisualCueType.Round;
			var firstCueLengths = SplitIntoRuns(roundStarts ? roundCount : squareCount, firstCueRuns);
			var secondCueLengths = SplitIntoRuns(roundStarts ? squareCount : roundCount, secondCueRuns);

			var sequence = new List<VisualCueType>(count);
			for (int run = 0; run < runCount; run++)
			{
				if (run % 2 == 0)
					sequence.AddRange(Enumerable.Repeat(firstCue, firstCueLengths[run / 2]));
				else
					sequence.AddRange(Enumerable.Repeat(secondCue, secondCueLengths[run / 2]));
			}

			return sequence;
		}

		private List<int> SplitIntoRuns(int total, int runCount)
		{
			if (runCount == 0)
				return new List<int>();

			var bounds = Enumerable.Range(1, total - 1)
				.OrderBy(_ => _random.Next())
				.Take(runCount - 1)
				.Append(0)
				.Append(total)
				.OrderBy(b => b)
				.ToList();

			return bounds.Zip(bounds.Skip(1), (start, end) => end - start).ToList();
		}
	}
}