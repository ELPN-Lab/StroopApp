using StroopApp.Models;
using Xunit;

namespace StroopApp.XUnitTests.ViewModels
{
	public class ExperimentProfileBasicTests
	{
		[Fact]
		public void DefaultConstructor_SetsDefaultValues()
		{
			// Arrange & Act
			var profile = new StroopProfile();

			// Assert
			Assert.Equal(50, profile.CongruencePercent);
			Assert.Null(profile.SwitchPercent);
			Assert.False(profile.HasVisualCue);
			Assert.Equal(100, profile.FixationDuration);
			Assert.Equal(400, profile.MaxReactionTime);
			Assert.Equal(0, profile.VisualCueDuration);
			Assert.Equal(CalculationMode.WordCount, profile.CalculationMode);
			Assert.Equal(10, profile.WordCount);
		}
	}
}

