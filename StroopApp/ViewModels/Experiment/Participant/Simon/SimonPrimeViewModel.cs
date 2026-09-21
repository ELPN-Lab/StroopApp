using StroopApp.Models.Simon;

namespace StroopApp.ViewModels.Experiment.Participant.Simon
{
    /// <summary>
    /// Step ViewModel for the reversal cue prime shown when the reversalCueMode is set to Before
    /// Can switch between a square, triangle, or circle depending on the situation
    /// </summary>
    internal class SimonPrimeViewModel
    {
        public SimonStimulusShape CueShape { get; }

        public SimonPrimeViewModel(SimonStimulusShape cueShape)
        {
            CueShape = cueShape;
        }
    }
}
