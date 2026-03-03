using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;

namespace Vitalis.Controls;

public sealed partial class VitalsBarChart : UserControl
{
    public VitalsBarChart()
    {
        this.InitializeComponent();
        SetupChart();
    }

    private void SetupChart()
    {
        var values = new double[] { 65, 80, 45, 90, 70, 85, 75 };
        var labels = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        var limeGreen = SKColor.Parse("#a3e635");

        Chart.Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = values,
                Fill = new SolidColorPaint(limeGreen),
                Stroke = null,
                MaxBarWidth = 24,
                Rx = 4,
                Ry = 4
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
                LabelsPaint = null,
                SeparatorsPaint = null,
                TicksPaint = null,
                ShowSeparatorLines = false
            }
        };

        Chart.DrawMargin = new LiveChartsCore.Measure.Margin(10, 10, 10, 30);
    }
}
