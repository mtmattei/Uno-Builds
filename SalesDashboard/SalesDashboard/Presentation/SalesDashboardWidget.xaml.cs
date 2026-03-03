using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.UI;

namespace SalesDashboard.Presentation;

public sealed partial class SalesDashboardWidget : UserControl
{
    private const int GridColumns = 12;
    private const int GridRows = 8;
    private const double CellSize = 20;
    private const double CellGap = 3;

    private readonly Random _random = new();
    private readonly Rectangle[,] _heatmapCells = new Rectangle[GridColumns, GridRows];
    private readonly double[,] _currentIntensities = new double[GridColumns, GridRows];
    private readonly double[,] _targetIntensities = new double[GridColumns, GridRows];

    // Animation targets
    private double _monthlyTarget = 312134;
    private double _monthlyCurrent = 312134;
    private double _monthlyChangeTarget = 12.4;
    private double _monthlyChangeCurrent = 12.4;

    private double _yearlyTarget = 3745608;
    private double _yearlyCurrent = 3745608;
    private double _yearlyChangeTarget = 8.2;
    private double _yearlyChangeCurrent = 8.2;

    private double _laTarget = 98420;
    private double _laCurrent = 98420;
    private double _laChangeTarget = 15.2;
    private double _laChangeCurrent = 15.2;

    private double _nyTarget = 87650;
    private double _nyCurrent = 87650;
    private double _nyChangeTarget = -3.8;
    private double _nyChangeCurrent = -3.8;

    private double _caTarget = 65230;
    private double _caCurrent = 65230;
    private double _caChangeTarget = 22.1;
    private double _caChangeCurrent = 22.1;

    private DispatcherTimer? _heatmapTimer;      // 150ms - intensity drift
    private DispatcherTimer? _dataUpdateTimer;   // 2s - random data updates
    private DispatcherTimer? _interpolationTimer; // 16ms - smooth number interpolation
    private DispatcherTimer? _liveIndicatorTimer; // 500ms - pulsing live indicator

    private bool _liveIndicatorOn = true;

    public SalesDashboardWidget()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeHeatmap();
        InitializeTimers();
        UpdateDisplay();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _heatmapTimer?.Stop();
        _dataUpdateTimer?.Stop();
        _interpolationTimer?.Stop();
        _liveIndicatorTimer?.Stop();
    }

    private void InitializeHeatmap()
    {
        HeatmapGrid.ColumnDefinitions.Clear();
        HeatmapGrid.RowDefinitions.Clear();
        HeatmapGrid.Children.Clear();

        for (int col = 0; col < GridColumns; col++)
        {
            HeatmapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize + CellGap) });
        }

        for (int row = 0; row < GridRows; row++)
        {
            HeatmapGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize + CellGap) });
        }

        for (int col = 0; col < GridColumns; col++)
        {
            for (int row = 0; row < GridRows; row++)
            {
                double intensity = _random.NextDouble();
                _currentIntensities[col, row] = intensity;
                _targetIntensities[col, row] = intensity;

                var cell = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    RadiusX = 3,
                    RadiusY = 3,
                    Fill = GetIntensityBrush(intensity),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };

                Grid.SetColumn(cell, col);
                Grid.SetRow(cell, row);
                HeatmapGrid.Children.Add(cell);
                _heatmapCells[col, row] = cell;
            }
        }
    }

    private static SolidColorBrush GetIntensityBrush(double intensity)
    {
        // Map intensity 0-1 to lightness 12%-85%
        double lightness = 0.12 + (intensity * 0.73);
        byte gray = (byte)(lightness * 255);
        return new SolidColorBrush(Color.FromArgb(255, gray, gray, gray));
    }

    private void InitializeTimers()
    {
        // 150ms heatmap cell intensity drift
        _heatmapTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _heatmapTimer.Tick += OnHeatmapTick;
        _heatmapTimer.Start();

        // 2s random data updates
        _dataUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _dataUpdateTimer.Tick += OnDataUpdateTick;
        _dataUpdateTimer.Start();

        // 16ms smooth number interpolation (60fps)
        _interpolationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _interpolationTimer.Tick += OnInterpolationTick;
        _interpolationTimer.Start();

        // 500ms pulsing live indicator
        _liveIndicatorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _liveIndicatorTimer.Tick += OnLiveIndicatorTick;
        _liveIndicatorTimer.Start();
    }

    private void OnHeatmapTick(object? sender, object e)
    {
        // Randomly drift some cells toward new target intensities
        int cellsToUpdate = _random.Next(5, 15);
        for (int i = 0; i < cellsToUpdate; i++)
        {
            int col = _random.Next(GridColumns);
            int row = _random.Next(GridRows);
            _targetIntensities[col, row] = _random.NextDouble();
        }

        // Interpolate all cells toward their targets
        for (int col = 0; col < GridColumns; col++)
        {
            for (int row = 0; row < GridRows; row++)
            {
                double current = _currentIntensities[col, row];
                double target = _targetIntensities[col, row];
                double newValue = current + (target - current) * 0.15;
                _currentIntensities[col, row] = newValue;
                _heatmapCells[col, row].Fill = GetIntensityBrush(newValue);
            }
        }
    }

    private void OnDataUpdateTick(object? sender, object e)
    {
        // Generate random changes for values
        _monthlyTarget = _monthlyCurrent * (1 + (_random.NextDouble() - 0.5) * 0.1);
        _monthlyChangeTarget = (_random.NextDouble() - 0.3) * 30;

        _yearlyTarget = _yearlyCurrent * (1 + (_random.NextDouble() - 0.5) * 0.05);
        _yearlyChangeTarget = (_random.NextDouble() - 0.3) * 20;

        _laTarget = _laCurrent * (1 + (_random.NextDouble() - 0.5) * 0.15);
        _laChangeTarget = (_random.NextDouble() - 0.3) * 40;

        _nyTarget = _nyCurrent * (1 + (_random.NextDouble() - 0.5) * 0.15);
        _nyChangeTarget = (_random.NextDouble() - 0.4) * 30;

        _caTarget = _caCurrent * (1 + (_random.NextDouble() - 0.5) * 0.15);
        _caChangeTarget = (_random.NextDouble() - 0.2) * 35;
    }

    private void OnInterpolationTick(object? sender, object e)
    {
        const double lerpFactor = 0.10; // 10% lerp per frame

        _monthlyCurrent = Lerp(_monthlyCurrent, _monthlyTarget, lerpFactor);
        _monthlyChangeCurrent = Lerp(_monthlyChangeCurrent, _monthlyChangeTarget, lerpFactor);

        _yearlyCurrent = Lerp(_yearlyCurrent, _yearlyTarget, lerpFactor);
        _yearlyChangeCurrent = Lerp(_yearlyChangeCurrent, _yearlyChangeTarget, lerpFactor);

        _laCurrent = Lerp(_laCurrent, _laTarget, lerpFactor);
        _laChangeCurrent = Lerp(_laChangeCurrent, _laChangeTarget, lerpFactor);

        _nyCurrent = Lerp(_nyCurrent, _nyTarget, lerpFactor);
        _nyChangeCurrent = Lerp(_nyChangeCurrent, _nyChangeTarget, lerpFactor);

        _caCurrent = Lerp(_caCurrent, _caTarget, lerpFactor);
        _caChangeCurrent = Lerp(_caChangeCurrent, _caChangeTarget, lerpFactor);

        UpdateDisplay();
    }

    private void OnLiveIndicatorTick(object? sender, object e)
    {
        _liveIndicatorOn = !_liveIndicatorOn;
        LiveIndicator.Opacity = _liveIndicatorOn ? 1.0 : 0.3;
    }

    private static double Lerp(double current, double target, double factor)
    {
        return current + (target - current) * factor;
    }

    private void UpdateDisplay()
    {
        // Monthly
        MonthlyValue.Text = FormatCurrency(_monthlyCurrent);
        UpdateChangeIndicator(MonthlyChangeIcon, MonthlyChangeValue, _monthlyChangeCurrent);

        // Yearly
        YearlyValue.Text = FormatCurrency(_yearlyCurrent);
        UpdateChangeIndicator(YearlyChangeIcon, YearlyChangeValue, _yearlyChangeCurrent);

        // Cities
        LAValue.Text = FormatCurrency(_laCurrent);
        UpdateCityChange(LAChange, _laChangeCurrent);

        NYValue.Text = FormatCurrency(_nyCurrent);
        UpdateCityChange(NYChange, _nyChangeCurrent);

        CAValue.Text = FormatCurrency(_caCurrent);
        UpdateCityChange(CAChange, _caChangeCurrent);
    }

    private static string FormatCurrency(double value)
    {
        return "$" + ((int)value).ToString("N0");
    }

    private static void UpdateChangeIndicator(TextBlock iconBlock, TextBlock valueBlock, double change)
    {
        bool isPositive = change >= 0;
        var color = isPositive
            ? Color.FromArgb(255, 0, 255, 136)   // Green #00FF88
            : Color.FromArgb(255, 255, 68, 68);  // Red #FF4444

        var brush = new SolidColorBrush(color);
        iconBlock.Foreground = brush;
        valueBlock.Foreground = brush;

        iconBlock.Text = isPositive ? "▲" : "▼";
        valueBlock.Text = Math.Abs(change).ToString("F1") + "%";
    }

    private static void UpdateCityChange(TextBlock changeBlock, double change)
    {
        bool isPositive = change >= 0;
        var color = isPositive
            ? Color.FromArgb(255, 0, 255, 136)   // Green #00FF88
            : Color.FromArgb(255, 255, 68, 68);  // Red #FF4444

        changeBlock.Foreground = new SolidColorBrush(color);
        changeBlock.Text = (isPositive ? "▲ +" : "▼ ") + Math.Abs(change).ToString("F1") + "%";
    }
}
