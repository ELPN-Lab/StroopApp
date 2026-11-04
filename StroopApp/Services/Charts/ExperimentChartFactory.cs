using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using StroopApp.Models;
using StroopApp.Resources;

namespace StroopApp.Services.Charts
{
	/// <summary>
	/// Factory for creating LiveCharts visualization objects for experiments.
	/// </summary>
	public class ExperimentChartFactory
	{
		/// <summary>
		/// Creates a column series bound to a live collection of reaction time points.
		/// </summary>
		/// <param name="reactionPoints">Live collection to bind to (modifications propagate to chart).</param>
		/// <returns>Observable collection containing a single column series.</returns>
		public ObservableCollection<ISeries> CreateLiveColumnSerie(ObservableCollection<ReactionTimePoint> reactionPoints)
		{
			return new ObservableCollection<ISeries>
			{
				new ColumnSeries<ReactionTimePoint>
				{
					Values = reactionPoints,
					DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
					DataLabelsSize = 16,
					DataLabelsPaint = new SolidColorPaint(SKColors.Black),
					DataLabelsFormatter = point => point.Coordinate.SecondaryValue.Equals(double.NaN) 
						? Strings.Label_NoResponse 
						: point.Coordinate.PrimaryValue.ToString("N0"),
					XToolTipLabelFormatter = point =>
					{
						var trial = (int)(point.Coordinate.SecondaryValue + 1);
						return string.Format(Strings.Label_TrialNumber, trial);
					},
					YToolTipLabelFormatter = point =>
					{
						var value = point.Coordinate.PrimaryValue;
						return double.IsNaN(value)
							? Strings.Label_NoResponse
							: string.Format(Strings.Label_ResponseTime, value);
					},
					Mapping = (point, index) => new Coordinate(point.TrialNumber - 1, point.ReactionTime != null ? point.ReactionTime.Value : double.NaN)
				}.OnPointCreated(p =>
				{
					if (p.Visual is null) return;
					var model = p.Model;
					if (model != null && model.IsValidResponse.HasValue)
					{
						// Orange (wrong answer)
						var orange = new SKColor(255, 166, 0);      // #FFA600
						// Purple (right answer)
						var purple = new SKColor(91, 46, 255);      // #5B2EFF
						if (model.IsValidResponse.Value)
						{
							p.Visual.Fill = new SolidColorPaint(purple);
							p.Visual.Stroke = new SolidColorPaint(purple);
						}
						else
						{
							p.Visual.Fill = new SolidColorPaint(orange);
							p.Visual.Stroke = new SolidColorPaint(orange);
						}
					}
				})
			};
		}

		/// <summary>
		/// Creates a column series from a snapshot of reaction time points.
		/// </summary>
		/// <param name="snapshot">Snapshot collection (isolated from original data).</param>
		/// <returns>Observable collection containing a single column series.</returns>
		public ObservableCollection<ISeries> CreateSnapshotColumnSerie(ObservableCollection<ReactionTimePoint> snapshot)
		{
			return new ObservableCollection<ISeries>
			{
				new ColumnSeries<ReactionTimePoint>
				{
					Values = snapshot,
					DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
					DataLabelsSize = 16,
					DataLabelsPaint = new SolidColorPaint(SKColors.Black),
					DataLabelsFormatter = point =>
						point.Coordinate.PrimaryValue.Equals(Double.NaN)
							? Strings.Label_NoResponse
							: point.Coordinate.PrimaryValue.ToString("N0"),
					Mapping = (point, index) => new Coordinate(
						point.TrialNumber - 1,
						point.ReactionTime != null ? point.ReactionTime.Value : double.NaN
					)
				}
				.OnPointCreated(p =>
				{
					if (p.Visual is null) return;
					var model = p.Model;
					if (model != null && model.IsValidResponse.HasValue)
					{
						var orange = new SKColor(255, 166, 0);
						var violet = new SKColor(91, 46, 255);
						if (model.IsValidResponse.Value)
						{
							p.Visual.Fill = new SolidColorPaint(violet);
							p.Visual.Stroke = new SolidColorPaint(violet);
						}
						else
						{
							p.Visual.Fill = new SolidColorPaint(orange);
							p.Visual.Stroke = new SolidColorPaint(orange);
						}
					}
				})
			};
		}

		/// <summary>
		/// Creates a line series for a block's reaction times.
		/// </summary>
		/// <param name="trialTimes">Collection of trial times from the current block.</param>
		/// <param name="start">Start index for X-axis mapping.</param>
		/// <param name="firstIndex">Index of the first trial drawn; earlier trials are left out.</param>
		/// <param name="lastIndex">Index of the last trial drawn; later trials are left out.</param>
		/// <returns>Configured line series.</returns>
		public LineSeries<double?> CreateBlockLineSeries(ObservableCollection<double?> trialTimes, int start, int firstIndex = 0, int lastIndex = int.MaxValue)
		{
			return new LineSeries<double?>
			{
				Values = trialTimes,
				Stroke = new SolidColorPaint(SKColors.Black, 2),
				Fill = new SolidColorPaint(SKColors.Black.WithAlpha(60)),
				LineSmoothness = 0.4f,
				GeometrySize = 6,
				GeometryStroke = new SolidColorPaint(SKColors.Black, 2),
				GeometryFill = new SolidColorPaint(SKColors.White),
				// Collection typed as double? but runtime usage ensures non-null values
				Mapping = (pt, idx) => idx < firstIndex || idx > lastIndex ? Coordinate.Empty : new Coordinate(start + idx, pt!.Value)
			};
		}

		/// <summary>
		/// Creates a rectangular section representing a block's trial range on the chart.
		/// </summary>
		/// <param name="xi">Start trial index.</param>
		/// <param name="xj">End trial index.</param>
		/// <param name="label">Text shown at the top of the section.</param>
		/// <param name="fillColor">Fill color with alpha channel.</param>
		/// <returns>Configured rectangular section.</returns>
		public RectangularSection CreateBlockSection(double xi, double xj, string label, SKColor fillColor)
		{
			return new RectangularSection
			{
				Xi = xi,
				Xj = xj,
				Fill = new SolidColorPaint(fillColor),
				Label = label,
				LabelSize = 16,
				LabelPaint = new SolidColorPaint(SKColors.Black)
			};
		}

		/// <summary>
		/// Creates a dashed vertical line marking a change of phase inside a block.
		/// </summary>
		/// <param name="x">Position of the line on the trial axis.</param>
		/// <returns>Configured zero-width section drawn as a line.</returns>
		public RectangularSection CreatePhaseSeparator(double x)
		{
			return new RectangularSection
			{
				Xi = x,
				Xj = x,
				Stroke = new SolidColorPaint(SKColors.DimGray, 2) { PathEffect = new DashEffect(new float[] { 6, 4 }) }
			};
		}
	}
}
