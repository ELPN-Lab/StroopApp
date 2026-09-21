using StroopApp.ViewModels.State;
using System.Collections.ObjectModel;
namespace StroopApp.Models
{
    /// <summary>
    /// Represents a block of trials in a Stroop experiment, containing trial records and calculated statistics.
    /// </summary>
    public class Block(ExperimentProfile profile, int blockNumber)
    {
        public int BlockNumber { get; set; } = blockNumber;
        public int TrialsPerBlock { get; set; }
        public double Accuracy { get; set; }
        public double? ResponseTimeMean { get; set; }
        public ExperimentProfile Profile { get; } = profile;
        public ObservableCollection<ITrial?> TrialRecords { get; } = new();
        public ObservableCollection<double?> TrialTimes { get; } = new();

        /// <summary>
        /// Calculates block statistics including trial count, accuracy percentage, and mean response time.
        /// </summary>
        public void CalculateValues()
        {
            TrialsPerBlock = TrialRecords.Count;
            Accuracy = TrialsPerBlock > 0
            ? TrialRecords.Count(t => t.IsValidResponse == true) / (double)TrialsPerBlock * 100
            : 0;
            ResponseTimeMean = TrialRecords
                                        .Where(trial => trial.ReactionTime.HasValue && trial.Block == BlockNumber)
                                        .Select(trial => trial.ReactionTime)
                                        .Average();
        }
    }
}
