using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using StroopApp.Models;
using StroopApp.Resources;
using StroopApp.ViewModels.State;
using System.Collections.ObjectModel;

namespace StroopApp.ViewModels.Experiment.Experimenter
{

    /// <summary>
    /// ViewModel for the column chart displaying reaction times per trial with a sliding 10-trial window.
    /// Provides formatted tooltips showing trial number, reaction time, and response validity.
    /// </summary>
    public class ColumnGraphViewModel
    {
        public ObservableCollection<ISeries> ColumnSerie
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
        public Func<ChartPoint, string> TooltipFormatter => chartPoint =>
        {
            if (chartPoint.Context.DataSource is not ReactionTimePoint model)
                return string.Empty;

            var trial = string.Format(Strings.Label_TrialNumber, model.TrialNumber);


            if (model.ReactionTime is null)
                return string.Format("{0}\n{1}", trial, Strings.Label_NoResponse);

            var rt = string.Format(Strings.Label_ResponseTime, model.ReactionTime.Value);
            var validity = model.IsValidResponse switch
            {
                true => Strings.Label_ResponseCorrect,
                false => Strings.Label_ResponseIncorrect,
                _ => Strings.Label_ResponseValidityUnknown

            };

            return $"{trial}\n{rt}\n{validity}";
        };



        public ColumnGraphViewModel(ExperimentSettingsViewModel settings)
        {
            ColumnSerie = settings.ExperimentContext.ColumnSerie;
            XAxes = new[]
            {
                new Axis
                {
                    MinLimit = -0.5,
                    MaxLimit = 10,
                    MinStep = 1,
                    Labeler = value => ((int)(value + 1)).ToString(),
                    Name = Strings.Header_Trials,
                    NamePadding = new LiveChartsCore.Drawing.Padding(0),
                    NameTextSize = 14
                }
            };
            YAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = settings.CurrentProfile.GetLongestReactionTime() * 1.1,
                    Name = Strings.Header_ResponseTime,
                    NamePadding = new LiveChartsCore.Drawing.Padding(0),
                    NameTextSize = 14
                }
            };
            settings.ExperimentContext.ReactionPoints.CollectionChanged += (s, e) =>
            {
                int count = settings.ExperimentContext.ReactionPoints.Count;
                const int window = 10;

                if (count > window)
                {
                    XAxes.First().MinLimit = count - window;
                    XAxes.First().MaxLimit = count;
                }
                ;
            };
        }
    }
}