using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Nexus.Presentation;

public partial record OverviewModel
{
    private static readonly SKColor SuccessColor = SKColor.Parse("#4ade80");
    private static readonly SKColor InfoColor = SKColor.Parse("#60a5fa");
    private static readonly SKColor WarningColor = SKColor.Parse("#fbbf24");
    private static readonly SKColor DangerColor = SKColor.Parse("#f87171");

    public ISeries[] ThroughputSeries { get; } = CreateSparklineSeries(
        [780, 810, 795, 820, 835, 850, 830, 847],
        SuccessColor);

    public ISeries[] EfficiencySeries { get; } = CreateSparklineSeries(
        [92.1, 93.5, 93.8, 94.2, 94.0, 94.5, 94.3, 94.7],
        SuccessColor);

    public ISeries[] UptimeSeries { get; } = CreateSparklineSeries(
        [99.8, 99.5, 99.6, 99.4, 99.3, 99.5, 99.1, 99.2],
        InfoColor);

    public ISeries[] EnergySeries { get; } = CreateSparklineSeries(
        [1.35, 1.32, 1.30, 1.28, 1.26, 1.25, 1.24, 1.24],
        SuccessColor);

    public Axis[] SparklineXAxis { get; } =
    [
        new Axis
        {
            IsVisible = false,
            ShowSeparatorLines = false,
            Padding = new LiveChartsCore.Drawing.Padding(0)
        }
    ];

    public Axis[] SparklineYAxis { get; } =
    [
        new Axis
        {
            IsVisible = false,
            ShowSeparatorLines = false,
            Padding = new LiveChartsCore.Drawing.Padding(0)
        }
    ];

    private static ISeries[] CreateSparklineSeries(double[] values, SKColor color)
    {
        return
        [
            new LineSeries<double>
            {
                Values = values,
                Fill = new SolidColorPaint(color.WithAlpha(30)),
                Stroke = new SolidColorPaint(color, 2),
                GeometryFill = null,
                GeometryStroke = null,
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        ];
    }
}
