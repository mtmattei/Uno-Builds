using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;
using Vitalis.Models;

namespace Vitalis.Controls;

public sealed partial class TrendChart : UserControl
{
    public TrendChart()
    {
        this.InitializeComponent();
        SetupChart();
        SetDefaultData();
    }

    private void SetupChart()
    {
        Chart.XAxes = new[]
        {
            new Axis
            {
                ShowSeparatorLines = false,
                LabelsPaint = null
            }
        };
        Chart.YAxes = new[]
        {
            new Axis
            {
                ShowSeparatorLines = false,
                LabelsPaint = null
            }
        };
    }

    private void SetDefaultData()
    {
        UpdateChart(OrganData.Heart);
    }

    public void UpdateChart(Organ organ)
    {
        var color = SKColor.Parse(organ.Color);
        var values = organ.History.Select(h => h.Value).ToArray();

        Chart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values,
                Fill = new LinearGradientPaint(
                    new[] { color.WithAlpha(128), color.WithAlpha(0) },
                    new SKPoint(0.5f, 0),
                    new SKPoint(0.5f, 1)),
                Stroke = new SolidColorPaint(color, 2),
                GeometryFill = null,
                GeometryStroke = null,
                LineSmoothness = 0.65
            }
        };
    }
}
