using StroopApp.Models;
using StroopApp.Models.Simon;
using StroopApp.ViewModels.State;

namespace StroopApp.Services.Trial
{
    public class SimonTrialGenerationService : ISimonTrialGenerationService
    {
        private readonly Random _random = new Random();

        public SimonTrialGenerationService() { }
        public List<ITrial> GenerateTrials(ExperimentSettingsViewModel settings)
        {
            if (settings?.CurrentProfile is null)
                throw new ArgumentException("CurrentProfile cannot be null", nameof(settings));
            if (settings?.Participant is null)
                throw new ArgumentException("Participant cannot be null", nameof(settings));

            SimonProfile profile = (SimonProfile)settings.CurrentProfile;
            int total = profile.WordCount;
            int congruentCount = total * profile.CongruencePercent / 100;
            int reversedMappingCount = profile.ReversalCuePresentation == ReversalCuePresentation.None ? 0 : total * profile.ReversedMappingPercent / 100;

            List<bool> congruenceFlags = GenerateCongruenceFlags(total, congruentCount, profile.StimulusPositionMode);
            List<bool> reversedFlags = GenerateReversedMappingFlags(total, reversedMappingCount);

            var trials = new List<ITrial>();
            bool isPositionModeLeftRight = profile.StimulusPositionMode is not SimonStimulusPositionMode.Center;
            for (int i = 0; i < total; i++)
            {
                bool isSpatialCongruent = congruenceFlags[i];
                bool isReversed = reversedFlags[i];
                bool isAnswerCongruent = isPositionModeLeftRight &&(isSpatialCongruent ^ isReversed);

                SimonAnswer answer = ComputeAnswer(profile.AnswerMode, isReversed);
                string color = ComputeColor(answer, isReversed, profile);
                StimulusPosition position = ComputePosition(answer, isAnswerCongruent, profile.AnswerMode, profile.StimulusPositionMode);
                SimonStimulusShape shape = ComputeShape(answer, isReversed, profile);

                trials.Add(new SimonTrial
                {
                    TrialNumber = i + 1,
                    Block = settings.Block,
                    ParticipantId = settings.Participant.Id,
                    CongruencePercent = profile.CongruencePercent,
                    ReversedMappingPercent = profile.ReversalCuePresentation == ReversalCuePresentation.None ? (int?)null : profile.ReversedMappingPercent,
                    IsSpatialCongruent = isSpatialCongruent,
                    IsAnswerCongruent = isAnswerCongruent,
                    IsReversedMapping = isReversed,
                    ExpectedAnswer = answer,
                    Stimulus = new SimonStimulus(color, position, shape)
                });

            }
            return trials;
        }

        private List<bool> GenerateCongruenceFlags(int totalStimulusCount,int congruentCount, SimonStimulusPositionMode mode )
        {
            return mode == SimonStimulusPositionMode.Center ?
                [.. Enumerable.Repeat(false, totalStimulusCount)] :
                [.. Enumerable.Repeat(true, congruentCount)
                .Concat(
                    Enumerable.Repeat(false, totalStimulusCount - congruentCount))
                .OrderBy(_ => _random.Next())];
        }

        private List<bool> GenerateReversedMappingFlags(int totalStimulusCount,int reversedCount)
        {
            return [.. Enumerable.Repeat(true, reversedCount)
                .Concat(
                Enumerable.Repeat(false, totalStimulusCount - reversedCount))
                .OrderBy(_=>_random.Next())];
        }
        private SimonAnswer ComputeAnswer(SimonAnswerMode answerMode, bool isReversed)
        {
            int draw = _random.Next(2);
            draw = isReversed ? 1 - draw : draw;
            return (answerMode, draw) switch
            {
                (SimonAnswerMode.GoNoGo, 0) => SimonAnswer.Go,
                (SimonAnswerMode.GoNoGo, 1) => SimonAnswer.NoGo,
                (SimonAnswerMode.LeftRight, 0) => SimonAnswer.Left,
                (SimonAnswerMode.LeftRight, 1) => SimonAnswer.Right,

                _ => throw new NotImplementedException(),
            };
        }
        private StimulusPosition ComputePosition(SimonAnswer expectedAnswer,bool isAnswerCongruent, SimonAnswerMode answerMode, SimonStimulusPositionMode positionMode)
        {
            if (positionMode == SimonStimulusPositionMode.Center)
                return StimulusPosition.Center;

            if(answerMode == SimonAnswerMode.GoNoGo)
                return _random.Next(2) == 0 ? StimulusPosition.Left : StimulusPosition.Right;

            return expectedAnswer == SimonAnswer.Left ? 
                (isAnswerCongruent  ? StimulusPosition.Left : StimulusPosition.Right) : 
                (isAnswerCongruent  ? StimulusPosition.Right : StimulusPosition.Left);
        }
        private string ComputeColor(SimonAnswer expectedAnswer, bool isReversed, SimonProfile profile)
        {

            if (profile.StimulusMode is SimonStimulusMode.Color)
            {
                return isReversed ?
                    (expectedAnswer is SimonAnswer.Left or SimonAnswer.Go ? profile.RightColor : profile.LeftColor) :
                    (expectedAnswer is SimonAnswer.Left or SimonAnswer.Go ? profile.LeftColor : profile.RightColor);
            }
            if (profile.ReversalCuePresentation is ReversalCuePresentation.Integrated)
            {
                    return isReversed ? profile.ReversedColor : profile.StandardColor;
            }
            else
                return profile.BaseColor;
        }
        private SimonStimulusShape ComputeShape(SimonAnswer expectedAnswer, bool isReversed, SimonProfile profile)
        {
            if (profile.StimulusMode is SimonStimulusMode.Shape)
            {
                return isReversed ?
                        (expectedAnswer is SimonAnswer.Left or SimonAnswer.Go ? profile.RightShape : profile.LeftShape) :
                        (expectedAnswer is SimonAnswer.Left or SimonAnswer.Go ? profile.LeftShape : profile.RightShape);
            }
            else if (profile.StimulusMode is SimonStimulusMode.Arrow)
            {
                return isReversed ?
                        (expectedAnswer is SimonAnswer.Left or SimonAnswer.Go ? SimonStimulusShape.RightArrow : SimonStimulusShape.LeftArrow) :
                        (expectedAnswer is SimonAnswer.Left or SimonAnswer.Go ? SimonStimulusShape.LeftArrow : SimonStimulusShape.RightArrow);
            }
            else if (profile.ReversalCuePresentation is ReversalCuePresentation.Integrated)
            {
                return isReversed ? profile.ReversedShape : profile.StandardShape;
            }
            else
                return profile.BaseShape;   

        }
    }
}
