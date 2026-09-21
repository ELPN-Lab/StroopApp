namespace StroopApp.Models.Simon
{
    /// <summary>
    /// Indicates how the reversal cue is presented
    /// </summary>
    public enum ReversalCuePresentation
    {
        Before, // appears after the fixation cross and before the stimulus
        Around, // appears at the same time as the stimulus, with a shape around it
        Integrated, // introduced depending on the StimulusMode (color : the shape changes; shape & arrow : the color changes)
        None,
    }
}
