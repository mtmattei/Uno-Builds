using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using UnoWallet.Models;

namespace UnoWallet.Presentation;

public partial class AnalyticsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isFourInstallmentSelected = true;

    [ObservableProperty]
    private IReadOnlyList<Installment> _currentInstallments;

    public AnalyticsViewModel()
    {
        TotalSpendingFormatted = AnalyticsMockData.TotalSpending.ToString("N2");
        OnProgressFormatted = AnalyticsMockData.OnProgress.ToString("N2");
        OverdueFormatted = AnalyticsMockData.Overdue.ToString("N2");
        TotalInstallmentsFormatted = AnalyticsMockData.TotalInstallments.ToString("N2");
        _currentInstallments = AnalyticsMockData.FourInstallments;

        // Initialize LiveCharts series
        InitializeChart();
    }

    public string TotalSpendingFormatted { get; }
    public string OnProgressFormatted { get; }
    public string OverdueFormatted { get; }
    public string TotalInstallmentsFormatted { get; }

    // LiveCharts properties
    public ISeries[] Series { get; private set; } = [];
    public Axis[] XAxes { get; private set; } = [];
    public Axis[] YAxes { get; private set; } = [];

    private void InitializeChart()
    {
        var values = AnalyticsMockData.ChartData
            .Select(d => (double)d.Amount)
            .ToArray();

        // Gradient colors for area fill (cyan to transparent)
        var gradientColors = new[]
        {
            new SKColor(77, 208, 225, 120),  // #4DD0E1 with 47% opacity
            new SKColor(77, 208, 225, 20)    // #4DD0E1 with 8% opacity (nearly transparent)
        };

        Series =
        [
            new LineSeries<double>
            {
                Values = values,
                // Gradient fill under the line
                Fill = new LinearGradientPaint(
                    gradientColors,
                    new SKPoint(0.5f, 0),   // Start from top center
                    new SKPoint(0.5f, 1)    // End at bottom center
                ),
                Stroke = new SolidColorPaint(new SKColor(77, 208, 225), 3), // #4DD0E1
                GeometryFill = new SolidColorPaint(new SKColor(77, 208, 225)),
                GeometryStroke = new SolidColorPaint(SKColors.White, 2),
                GeometrySize = 10,
                LineSmoothness = 0.3
            }
        ];

        XAxes =
        [
            new Axis
            {
                Labels = ["Nov 1", "", "", "", "", "", "Nov 30"],
                LabelsPaint = new SolidColorPaint(new SKColor(142, 142, 147)), // #8E8E93
                TextSize = 12,
                SeparatorsPaint = new SolidColorPaint(new SKColor(229, 229, 234, 80)) { StrokeThickness = 1 }, // Light grid lines
                TicksPaint = null,
                Position = LiveChartsCore.Measure.AxisPosition.End
            }
        ];

        YAxes =
        [
            new Axis
            {
                LabelsPaint = new SolidColorPaint(new SKColor(142, 142, 147)), // #8E8E93
                TextSize = 10,
                SeparatorsPaint = new SolidColorPaint(new SKColor(229, 229, 234, 80)) { StrokeThickness = 1 }, // Light horizontal grid lines
                TicksPaint = null,
                Labeler = value => $"${value / 1000:N1}k", // Format as $1.2k, $2.0k, etc.
                MinLimit = 0
            }
        ];
    }

    [RelayCommand]
    private void SelectFourInstallment()
    {
        IsFourInstallmentSelected = true;
        CurrentInstallments = AnalyticsMockData.FourInstallments;
    }

    [RelayCommand]
    private void SelectSixInstallment()
    {
        IsFourInstallmentSelected = false;
        CurrentInstallments = AnalyticsMockData.SixInstallments;
    }
}
