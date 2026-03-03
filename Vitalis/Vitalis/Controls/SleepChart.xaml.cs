using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Controls;
using SkiaSharp;

namespace Vitalis.Controls;

public sealed partial class SleepChart : UserControl
{
    public SleepChart()
    {
        this.InitializeComponent();
        SetupChart();
    }

    private void SetupChart()
    {
        // Stacked bar data for sleep stages
        var awake = new double[] { 0.5, 0.3, 0.8, 0.4, 0.6, 0.3 };
        var rem = new double[] { 1.5, 1.8, 1.2, 1.6, 1.4, 1.7 };
        var core = new double[] { 3.0, 3.2, 2.8, 3.5, 3.1, 3.3 };
        var deep = new double[] { 2.0, 1.7, 2.2, 1.5, 1.9, 1.7 };
        var labels = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri" };

        Chart.Series = new ISeries[]
        {
            new StackedColumnSeries<double>
            {
                Values = deep,
                Fill = new SolidColorPaint(SKColor.Parse("#d9f99d")),
                Stroke = null,
                MaxBarWidth = 28,
                Rx = 0,
                Ry = 0,
                Name = "Deep"
            },
            new StackedColumnSeries<double>
            {
                Values = core,
                Fill = new SolidColorPaint(SKColor.Parse("#a3e635")),
                Stroke = null,
                MaxBarWidth = 28,
                Rx = 0,
                Ry = 0,
                Name = "Core"
            },
            new StackedColumnSeries<double>
            {
                Values = rem,
                Fill = new SolidColorPaint(SKColor.Parse("#65a30d")),
                Stroke = null,
                MaxBarWidth = 28,
                Rx = 0,
                Ry = 0,
                Name = "Rem"
            },
            new StackedColumnSeries<double>
            {
                Values = awake,
                Fill = new SolidColorPaint(SKColor.Parse("#404040")),
                Stroke = null,
                MaxBarWidth = 28,
                Rx = 4,
                Ry = 4,
                Name = "Awake"
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
                TextSize = 10,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#262626")) { StrokeThickness = 1 },
                TicksPaint = null,
                MinLimit = 0,
                MaxLimit = 8
            }
        };

        Chart.DrawMargin = new LiveChartsCore.Measure.Margin(30, 10, 10, 30);
    }
}
