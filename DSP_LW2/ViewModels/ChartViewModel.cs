using LiveChartsCore;
using LiveChartsCore.Easing;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DSP_LW2.ViewModels
{
    public class ChartViewModel
    {
        public ObservableCollection<ISeries> SignalSeries { get; set; }
        public ObservableCollection<ISeries> AmplitudeSeries { get; set; }
        public ObservableCollection<ISeries> PhaseSeries { get; set; }

        public ObservableCollection<Axis> XSignalAxes { get; set; }
        public ObservableCollection<Axis> YSignalAxes { get; set; }
        public ObservableCollection<Axis> XSpectrumAxes { get; set; }
        public ObservableCollection<Axis> YSpectrumAxes { get; set; }

        public ChartViewModel()
        {
            SignalSeries = new ObservableCollection<ISeries>();
            AmplitudeSeries = new ObservableCollection<ISeries>();
            PhaseSeries = new ObservableCollection<ISeries>();

            XSignalAxes = new ObservableCollection<Axis> { new Axis() };
            YSignalAxes = new ObservableCollection<Axis> { new Axis() };

            XSpectrumAxes = new ObservableCollection<Axis> { new Axis() };
            YSpectrumAxes = new ObservableCollection<Axis> { new Axis() };
        }

        public void CreateSignalsChart(params IEnumerable<float>[] values)
        {
            SignalSeries.Clear();
            XSignalAxes.Clear();
            YSignalAxes.Clear();

            XSignalAxes.Add(new Axis
            {
                Labeler = value => $"{value:0.0#}"
            });

            YSignalAxes.Add(new Axis());

            foreach (var value in values)
            {
                var lineSeries = new LineSeries<float>
                {
                    Name = string.Empty,
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null,
                    Values = value
                };

                SignalSeries.Add(lineSeries);
            }
        }

        public void CreateSpectrumChart(IEnumerable<float> amplitudeValues, IEnumerable<float> phaseValues)
        {
            AmplitudeSeries.Clear();
            PhaseSeries.Clear();
            XSpectrumAxes.Clear();
            YSpectrumAxes.Clear();

            XSpectrumAxes.Add(new Axis());
            YSpectrumAxes.Add(new Axis());

            var columnAmplitudeSeries = new ColumnSeries<float>
            {
                GroupPadding = 1,
                MaxBarWidth = 8,
                Name = string.Empty,
                Stroke = null,
                Values = amplitudeValues
            };

            var columnPhaseSeries = new ColumnSeries<float>
            {
                GroupPadding = 1,
                MaxBarWidth = 8,
                Name = string.Empty,
                Stroke = null,
                Values = phaseValues
            };

            columnAmplitudeSeries.PointMeasured += OnPointMeasured;
            columnPhaseSeries.PointMeasured += OnPointMeasured;

            AmplitudeSeries.Add(columnAmplitudeSeries);
            PhaseSeries.Add(columnPhaseSeries);
        }

        private void OnPointMeasured(TypedChartPoint<float, RoundedRectangleGeometry, LabelGeometry, SkiaSharpDrawingContext> point)
        {
            var visual = point.Visual;
            var delayedFunction = new DelayedFunction(EasingFunctions.BuildCustomElasticOut(1.5f, 0.60f), point, 20f);

            _ = visual
                .TransitionateProperties(
                    nameof(visual.Y),
                    nameof(visual.Height))
                .WithAnimation(animation =>
                    animation
                        .WithDuration(delayedFunction.Speed)
                        .WithEasingFunction(delayedFunction.Function));
        }
    }
}
