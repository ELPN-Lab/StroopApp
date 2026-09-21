using StroopApp.Models;

namespace StroopApp.ViewModels.Experiment.Participant.Common
{
    /// <summary>
    /// ViewModel for displaying visual cues during Stroop test trials.
    /// </summary>
    public class VisualCueControlViewModel
    {
        public VisualCueType VisualCue { get; set; }

        public VisualCueControlViewModel(VisualCueType visualCue)
        {
            VisualCue = visualCue;
        }
    }
}
