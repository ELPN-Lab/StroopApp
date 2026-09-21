using StroopApp.Models.Simon;

namespace StroopApp.Models
{
    public class ExperimentKeyMappings
    {
        public KeyMappings Stroop { get; set; } = new();
        public SimonResponseMappings Simon { get; set; } = new();
    }
}
