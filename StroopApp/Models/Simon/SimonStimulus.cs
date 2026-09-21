namespace StroopApp.Models.Simon
{
    public class SimonStimulus
    {
        /// <summary>
        /// Color name associated with the direction (Left/Right vs. Go/No-Go) or Validation (Normal vs. Opposite command).
        /// </summary>
        public string Color { get; set; } = null!;

        /// <summary>
        /// Position associated with the Position mode (Left/Right vs. Go/No-Go)
        /// </summary>
        public StimulusPosition Position { get; set; }

        public SimonStimulusShape Shape { get; set; }

        /// <summary>
        /// Initializes a circle stimulus with color and position.
        /// </summary>
        public SimonStimulus(string color, StimulusPosition position, SimonStimulusShape shape)
        {
            Color = color;
            Position = position;
            Shape = shape;
        }
    }
}
