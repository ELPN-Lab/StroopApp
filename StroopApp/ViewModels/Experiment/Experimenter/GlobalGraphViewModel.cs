using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using StroopApp.Core;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.ViewModels.State;
using System.Collections.ObjectModel;

namespace StroopApp.ViewModels.Experiment.Experimenter
{
    /// <summary>
    /// ViewModel for the global graph displaying all blocks' reaction times with trial sections.
    /// Calculates axis limits based on total trials and maximum reaction time across all blocks.
    /// </summary>
    public partial class GlobalGraphViewModel : ViewModelBase
    {
        public ObservableCollection<ISeries> Series
        {
            get;
        }
        public ObservableCollection<RectangularSection> Sections
        {
            get;
        }
        public Axis[] XAxes
        {
            get;
        }
        public Axis[] YAxes
        {
            get;
        }
        public GlobalGraphViewModel(ExperimentSettingsViewModel settings)
        {
            Series = settings.ExperimentContext.BlockSeries;
            Sections = settings.ExperimentContext.Sections;
            var totalTrials = settings.ExperimentContext.Blocks.Sum(b => b.TrialRecords.Count());
            XAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0.5,
                    MaxLimit = totalTrials + 0.5,
                    MinStep = 1,
                    Name = Strings.Header_Trials,
                    NamePadding = new LiveChartsCore.Drawing.Padding(0),
                    NameTextSize = 14
                }
        };

            double maxRt = settings.ExperimentContext.Blocks
                .SelectMany(b => b.TrialRecords)
                .Where(t => t.ReactionTime.HasValue)
                .Select(t => t.ReactionTime!.Value)
                .DefaultIfEmpty(0)
                .Max();
            YAxes = new[]
                {
                new Axis {
                    MinLimit = 0,
                    MaxLimit = Math.Max(maxRt, settings.CurrentProfile.GetLongestReactionTime()),
                    Name = Strings.Header_ResponseTime,
                    NamePadding = new LiveChartsCore.Drawing.Padding(0),
                    NameTextSize = 14
    }
};
        }

    }
}
