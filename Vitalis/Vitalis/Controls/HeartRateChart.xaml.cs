using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;

namespace Vitalis.Controls;

public sealed partial class HeartRateChart : UserControl
{
    public HeartRateChart()
    {
        this.InitializeComponent();
        SetupChart();
    }

    private void SetupChart()
    {
        var values = new double[] { 68, 62, 75, 98, 82, 72, 70 };
        var labels = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        var limeGreen = SKColor.Parse("#a3e635");

        Chart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                Fill = new LinearGradientPaint(
                    new[] { limeGreen.WithAlpha(100), limeGreen.WithAlpha(0) },
                    new SKPoint(0.5f, 0),
                    new SKPoint(0.5f, 1)),
                Stroke = new SolidColorPaint(limeGreen, 2),
                GeometryFill = new SolidColorPaint(limeGreen),
                GeometryStroke = null,
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        };

        Chart.XAxes = new[]
        {
            new Axis
            {
                Labels = labels,
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#606060")),
                TextSize = 11,
                SeparatorsPaint = null,
                TicksPaint = null
            }
        };

        Chart.YAxes = new[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#606060")),
                TextSize = 11,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#262626")) { StrokeThickness = 1 },
                TicksPaint = null,
                MinLimit = 40,
                MaxLimit = 180,
                Labeler = value => $"{value}bpm"
            }
        };

        Chart.DrawMargin = new LiveChartsCore.Measure.Margin(50, 10, 10, 30);
    }
}
