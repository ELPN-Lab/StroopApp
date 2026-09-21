using System.Collections.Generic;

using StroopApp.ViewModels.State;
using StroopApp.Models;
using StroopApp.Services.Trial;

namespace StroopApp.XUnitTests.TestDummies
{
	public class DummyTrialGenerationService : ITrialGenerationService
	{
		public bool GenerateTrialsCalled { get; private set; }
		public bool GenerateVisualCueSequenceCalled { get; private set; }
		public int LastTrialCount { get; private set; }

		public List<ITrial> GenerateTrials(ExperimentSettingsViewModel settings)
		{
			GenerateTrialsCalled = true;

			if (settings?.CurrentProfile == null)
				return new List<ITrial>();

			var trials = new List<ITrial>();
			int trialCount = settings.CurrentProfile.WordCount;
			LastTrialCount = trialCount;

			for (int i = 0; i < trialCount; i++)
			{
				trials.Add(new StroopTrial
				{
					TrialNumber = i + 1,
					Block = settings.Block,
					ParticipantId = settings.Participant?.Id ?? "TestParticipant",
					IsCongruent = i % 2 == 0,
					ExpectedAnswer = "TestAnswer",
					Stimulus = new Word("Red", "Red", "RED")
				});
			}

			return trials;
		}

		public List<VisualCueType> GenerateVisualCueSequence(int count, int switchPercentage)
		{
			GenerateVisualCueSequenceCalled = true;

			var sequence = new List<VisualCueType>();
			for (int i = 0; i < count; i++)
			{
				sequence.Add(i % 2 == 0 ? VisualCueType.Square : VisualCueType.Round);
			}

			return sequence;
		}
	}
}