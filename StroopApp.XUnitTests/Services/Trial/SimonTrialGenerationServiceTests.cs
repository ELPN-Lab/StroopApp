using StroopApp.Models.Simon;
using StroopApp.Models;
using StroopApp.ViewModels.State;
using Xunit;
using StroopApp.Services.Trial;


namespace StroopApp.XUnitTests;

public class SimonTrialGenerationServiceTests
{
    private static ExperimentSettingsViewModel CreateSettings(Action<SimonProfile>? configure = null)
    {
        var profile = new SimonProfile
        {
            WordCount = 100,
            CongruencePercent = 50,
            ReversedMappingPercent = 0,
        };
        configure?.Invoke(profile);
        return new ExperimentSettingsViewModel(new ExperimentSettings()
        {
            CurrentProfile = profile,
            Participant = new Participant { Id = "Test" },
            Block = 1,
        });
    }

    [Fact]
    public void GenerateTrial_CongruencePercent70_ProducesExactly70CongruentTrials()
    {
        var settings = CreateSettings(p => p.CongruencePercent = 70);
        var service = new SimonTrialGenerationService();

        var trials = service.GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.Equal(70, trials.Count(t => t.IsAnswerCongruent));
    }

    [Fact]
    public void GenerateTrials_ShapeModeReversed_StimulusShowsOppositeSideOfExpectedAnswer()
    {
        var settings = CreateSettings(p =>
        {
            p.StimulusMode = SimonStimulusMode.Shape;
            p.ReversalCuePresentation = ReversalCuePresentation.Before;
            p.ReversedMappingPercent = 100;
            p.LeftShape = SimonStimulusShape.Circle;
            p.RightShape = SimonStimulusShape.Square;
        });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.All(trials, t =>
        {
            var drawnSideShape = t.ExpectedAnswer == SimonAnswer.Left
            ? SimonStimulusShape.Square
            : SimonStimulusShape.Circle;
            Assert.Equal(t.Stimulus.Shape, drawnSideShape);
        });
    }

    [Fact]
    public void GenerateTrials_ColorModeReversed_StimulusShowsOppositeSideColor()
    {
        var settings = CreateSettings(p =>
        {
            p.StimulusMode = SimonStimulusMode.Color;
            p.ReversalCuePresentation = ReversalCuePresentation.Before;
            p.ReversedMappingPercent = 100;
            p.LeftColor = "#111111";
            p.RightColor = "#222222";
        });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.All(trials, t =>
        {
            var drawnSideColor = t.ExpectedAnswer == SimonAnswer.Left ? "#222222" : "#111111";
            Assert.Equal(drawnSideColor, t.Stimulus.Color);
        });
    }
    [Fact]
    public void GenerateTrials_ShapeMode_AnswerGoNoGo_StimulusShowsCorrectSideShape()
    {
        var settings = CreateSettings(p =>
        {
            p.AnswerMode = SimonAnswerMode.GoNoGo;
            p.StimulusMode = SimonStimulusMode.Shape;
            p.ReversalCuePresentation = ReversalCuePresentation.Before;
            p.ReversedMappingPercent = 0;
            p.LeftShape = SimonStimulusShape.Circle;
            p.RightShape = SimonStimulusShape.Square;
        });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.All(trials, t =>
        {
            var drawnSide = t.ExpectedAnswer == SimonAnswer.Go ? SimonStimulusShape.Circle : SimonStimulusShape.Square;
            Assert.Equal(drawnSide, t.Stimulus.Shape);
        });
    }

    [Fact]
    public void GenerateTrials_PositionModeCenter_IsAnswserCongruentAlwaysFalse()
    {
        var settings = CreateSettings(p =>
        {
            p.StimulusMode = SimonStimulusMode.Shape;
            p.StimulusPositionMode = SimonStimulusPositionMode.Center;
            p.LeftShape = SimonStimulusShape.Circle;
            p.RightShape = SimonStimulusShape.Square;
        });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.All(trials, t =>
            Assert.False(t.IsAnswerCongruent));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    [InlineData(50)]
    [InlineData(100)]
    public void GenerateTrials_ReversedMappingPercent_ProducesExactCount(int percent)
    {
        var settings = CreateSettings(p => p.ReversedMappingPercent = percent);

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.Equal(percent, trials.Count(t => t.IsReversedMapping));
    }

    [Fact]
    public void GenerateTrials_SpatialCongruence_IsAnswerCongruenceXorReversal()
    {
        var settings = CreateSettings(p => { p.CongruencePercent = 50; p.ReversedMappingPercent = 50; });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.All(trials, t =>
            Assert.Equal(t.IsAnswerCongruent ^ t.IsReversedMapping, t.IsSpatialCongruent));
    }

    [Fact]
    public void GenerateTrials_LeftRightMode_PositionMatchesExpectedAnswerIffCongruent()
    {
        var settings = CreateSettings(p => { p.CongruencePercent = 50; p.ReversedMappingPercent = 50; });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.All(trials, t =>
        {
            var expectedSide = t.ExpectedAnswer == SimonAnswer.Left ? StimulusPosition.Left : StimulusPosition.Right;
            if (t.IsAnswerCongruent)
                Assert.Equal(expectedSide, t.Stimulus.Position);
            else
                Assert.NotEqual(expectedSide, t.Stimulus.Position);
        });
    }

    [Fact]
    public void GenerateTrials_ColorModeBeforeCue_ShapeStaysStandardOnReversedTrials()
    {
        var settings = CreateSettings(p =>
        {
            p.StimulusMode = SimonStimulusMode.Color;
            p.ReversalCuePresentation = ReversalCuePresentation.Before;
            p.ReversedMappingPercent = 100;
            p.StandardShape = SimonStimulusShape.Circle;
            p.ReversedShape = SimonStimulusShape.Square;
        });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        // …donc le stimulus ne doit PAS le porter aussi
        Assert.All(trials, t => Assert.Equal(SimonStimulusShape.Circle, t.Stimulus.Shape));
    }

    [Fact]
    public void GenerateTrials_ShapeModeBeforeCue_ColorStaysStandardOnReversedTrials()
    {
        var settings = CreateSettings(p =>
        {
            p.StimulusMode = SimonStimulusMode.Shape;
            p.ReversalCuePresentation = ReversalCuePresentation.Before;  // l'indice est dans l'amorce…
            p.ReversedMappingPercent = 100;
            p.BaseColor = "#101010";
        });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        // …donc le stimulus ne doit PAS le porter aussi
        Assert.All(trials, t => Assert.Equal("#101010", t.Stimulus.Color));
    }

    [Fact]
    public void GenerateTrials_ArrowMode_IgnoresConfiguredShapes()
    {
        var settings = CreateSettings(p =>
        {
            p.StimulusMode = SimonStimulusMode.Arrow;
            p.LeftShape = SimonStimulusShape.Circle;
            p.RightShape = SimonStimulusShape.Triangle;
        });

        var trials = new SimonTrialGenerationService().GenerateTrials(settings).Cast<SimonTrial>().ToList();

        Assert.All(trials, t =>
            Assert.True(t.Stimulus.Shape is SimonStimulusShape.LeftArrow or SimonStimulusShape.RightArrow));
    }


}
