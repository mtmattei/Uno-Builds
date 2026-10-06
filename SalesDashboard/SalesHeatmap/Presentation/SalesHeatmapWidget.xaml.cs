using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace SalesHeatmap.Presentation;

public sealed partial class SalesHeatmapWidget : UserControl
{
    private const int GridColumns = 12;
    private const int GridRows = 8;
    private const double CellSize = 20;
    private const double CellGap = 3;
    private const int PaletteBuckets = 32;
    private const double InterpolationFactor = 0.10;
    private const double ConvergenceEpsilon = 0.5;

    private static readonly SolidColorBrush[] HeatmapPalette = BuildHeatmapPalette();
    private static readonly SolidColorBrush PositiveBrush =
        new(Color.FromArgb(255, 0, 255, 136));
    private static readonly SolidColorBrush NegativeBrush =
        new(Color.FromArgb(255, 255, 68, 68));

    public static readonly DependencyProperty SnapshotProperty =
        DependencyProperty.Register(
            nameof(Snapshot),
            typeof(SalesSnapshot),
            typeof(SalesHeatmapWidget),
            new PropertyMetadata(null, OnSnapshotChanged));

    public SalesSnapshot? Snapshot
    {
        get => (SalesSnapshot?)GetValue(SnapshotProperty);
        set => SetValue(SnapshotProperty, value);
    }

    private readonly Random _random = new();
    private readonly Rectangle[,] _heatmapCells = new Rectangle[GridColumns, GridRows];
    private readonly double[,] _currentIntensities = new double[GridColumns, GridRows];
    private readonly double[,] _targetIntensities = new double[GridColumns, GridRows];
    private readonly int[,] _appliedBuckets = new int[GridColumns, GridRows];

    private double _monthlyTarget, _monthlyCurrent;
    private double _monthlyChangeTarget, _monthlyChangeCurrent;
    private double _yearlyTarget, _yearlyCurrent;
    private double _yearlyChangeTarget, _yearlyChangeCurrent;
    private double _laTarget, _laCurrent;
    private double _laChangeTarget, _laChangeCurrent;
    private double _nyTarget, _nyCurrent;
    private double _nyChangeTarget, _nyChangeCurrent;
    private double _caTarget, _caCurrent;
    private double _caChangeTarget, _caChangeCurrent;

    private string? _lastMonthlyText, _lastYearlyText, _lastLAText, _lastNYText, _lastCAText;
    private string? _lastMonthlyChangeText, _lastYearlyChangeText;
    private string? _lastLAChangeText, _lastNYChangeText, _lastCAChangeText;
    private int _lastMonthlySign = int.MinValue, _lastYearlySign = int.MinValue;
    private int _lastLASign = int.MinValue, _lastNYSign = int.MinValue, _lastCASign = int.MinValue;

    private DispatcherTimer? _heatmapTimer;
    private DispatcherTimer? _interpolationTimer;
    private DispatcherTimer? _liveIndicatorTimer;
    private bool _liveIndicatorOn = true;

    public SalesHeatmapWidget()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SeedFromSnapshot(SalesSnapshot.Initial, animate: false);
    }

    private static SolidColorBrush[] BuildHeatmapPalette()
    {
        var palette = new SolidColorBrush[PaletteBuckets];
        for (int i = 0; i < PaletteBuckets; i++)
        {
            double intensity = (double)i / (PaletteBuckets - 1);
            double lightness = 0.12 + intensity * 0.73;
            byte gray = (byte)(lightness * 255);
            palette[i] = new SolidColorBrush(Color.FromArgb(255, gray, gray, gray));
        }
        return palette;
    }

    private static int IntensityToBucket(double intensity)
    {
        int bucket = (int)(Math.Clamp(intensity, 0, 1) * (PaletteBuckets - 1));
        return bucket;
    }

    private static void OnSnapshotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SalesHeatmapWidget widget && e.NewValue is SalesSnapshot snapshot)
        {
            widget.SeedFromSnapshot(snapshot, animate: true);
        }
    }

    private void SeedFromSnapshot(SalesSnapshot snapshot, bool animate)
    {
        _monthlyTarget = snapshot.Monthly;
        _monthlyChangeTarget = snapshot.MonthlyChange;
        _yearlyTarget = snapshot.Yearly;
        _yearlyChangeTarget = snapshot.YearlyChange;
        _laTarget = snapshot.LosAngeles;
        _laChangeTarget = snapshot.LosAngelesChange;
        _nyTarget = snapshot.NewYork;
        _nyChangeTarget = snapshot.NewYorkChange;
        _caTarget = snapshot.Canada;
        _caChangeTarget = snapshot.CanadaChange;

        if (!animate)
        {
            _monthlyCurrent = _monthlyTarget;
            _monthlyChangeCurrent = _monthlyChangeTarget;
            _yearlyCurrent = _yearlyTarget;
            _yearlyChangeCurrent = _yearlyChangeTarget;
            _laCurrent = _laTarget;
            _laChangeCurrent = _laChangeTarget;
            _nyCurrent = _nyTarget;
            _nyChangeCurrent = _nyChangeTarget;
            _caCurrent = _caTarget;
            _caChangeCurrent = _caChangeTarget;
        }
        else if (_interpolationTimer is { IsEnabled: false })
        {
            _interpolationTimer.Start();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeHeatmap();
        InitializeTimers();
        UpdateDisplay();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_heatmapTimer is not null)
        {
            _heatmapTimer.Stop();
            _heatmapTimer.Tick -= OnHeatmapTick;
            _heatmapTimer = null;
        }
        if (_interpolationTimer is not null)
        {
            _interpolationTimer.Stop();
            _interpolationTimer.Tick -= OnInterpolationTick;
            _interpolationTimer = null;
        }
        if (_liveIndicatorTimer is not null)
        {
            _liveIndicatorTimer.Stop();
            _liveIndicatorTimer.Tick -= OnLiveIndicatorTick;
            _liveIndicatorTimer = null;
        }
    }

    private void InitializeHeatmap()
    {
        // Reset on re-Loaded (e.g., theme change re-mounts the template).
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

                int bucket = IntensityToBucket(intensity);
                _appliedBuckets[col, row] = bucket;

                var cell = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    RadiusX = 3,
                    RadiusY = 3,
                    Fill = HeatmapPalette[bucket],
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

    private void InitializeTimers()
    {
        _heatmapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _heatmapTimer.Tick += OnHeatmapTick;
        _heatmapTimer.Start();

        _interpolationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _interpolationTimer.Tick += OnInterpolationTick;
        _interpolationTimer.Start();

        _liveIndicatorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _liveIndicatorTimer.Tick += OnLiveIndicatorTick;
        _liveIndicatorTimer.Start();
    }

    private void OnHeatmapTick(object? sender, object e)
    {
        int cellsToUpdate = _random.Next(5, 15);
        for (int i = 0; i < cellsToUpdate; i++)
        {
            int col = _random.Next(GridColumns);
            int row = _random.Next(GridRows);
            _targetIntensities[col, row] = _random.NextDouble();
        }

        for (int col = 0; col < GridColumns; col++)
        {
            for (int row = 0; row < GridRows; row++)
            {
                double current = _currentIntensities[col, row];
                double target = _targetIntensities[col, row];
                double newValue = current + (target - current) * 0.15;
                _currentIntensities[col, row] = newValue;

                int bucket = IntensityToBucket(newValue);
                if (bucket != _appliedBuckets[col, row])
                {
                    _appliedBuckets[col, row] = bucket;
                    _heatmapCells[col, row].Fill = HeatmapPalette[bucket];
                }
            }
        }
    }

    private void OnInterpolationTick(object? sender, object e)
    {
        bool converged = true;

        converged &= StepToward(ref _monthlyCurrent, _monthlyTarget);
        converged &= StepToward(ref _monthlyChangeCurrent, _monthlyChangeTarget);
        converged &= StepToward(ref _yearlyCurrent, _yearlyTarget);
        converged &= StepToward(ref _yearlyChangeCurrent, _yearlyChangeTarget);
        converged &= StepToward(ref _laCurrent, _laTarget);
        converged &= StepToward(ref _laChangeCurrent, _laChangeTarget);
        converged &= StepToward(ref _nyCurrent, _nyTarget);
        converged &= StepToward(ref _nyChangeCurrent, _nyChangeTarget);
        converged &= StepToward(ref _caCurrent, _caTarget);
        converged &= StepToward(ref _caChangeCurrent, _caChangeTarget);

        UpdateDisplay();

        if (converged)
        {
            _interpolationTimer?.Stop();
        }
    }

    private static bool StepToward(ref double current, double target)
    {
        double delta = target - current;
        if (Math.Abs(delta) < ConvergenceEpsilon)
        {
            current = target;
            return true;
        }
        current += delta * InterpolationFactor;
        return false;
    }

    private void OnLiveIndicatorTick(object? sender, object e)
    {
        _liveIndicatorOn = !_liveIndicatorOn;
        LiveIndicator.Opacity = _liveIndicatorOn ? 1.0 : 0.3;
    }

    private void UpdateDisplay()
    {
        SetTextIfChanged(MonthlyValue, FormatCurrency(_monthlyCurrent), ref _lastMonthlyText);
        SetTextIfChanged(YearlyValue, FormatCurrency(_yearlyCurrent), ref _lastYearlyText);
        SetTextIfChanged(LAValue, FormatCurrency(_laCurrent), ref _lastLAText);
        SetTextIfChanged(NYValue, FormatCurrency(_nyCurrent), ref _lastNYText);
        SetTextIfChanged(CAValue, FormatCurrency(_caCurrent), ref _lastCAText);

        UpdateChangeIndicator(MonthlyChangeIcon, MonthlyChangeValue, _monthlyChangeCurrent,
            ref _lastMonthlyChangeText, ref _lastMonthlySign);
        UpdateChangeIndicator(YearlyChangeIcon, YearlyChangeValue, _yearlyChangeCurrent,
            ref _lastYearlyChangeText, ref _lastYearlySign);

        UpdateCityChange(LAChange, _laChangeCurrent, ref _lastLAChangeText, ref _lastLASign);
        UpdateCityChange(NYChange, _nyChangeCurrent, ref _lastNYChangeText, ref _lastNYSign);
        UpdateCityChange(CAChange, _caChangeCurrent, ref _lastCAChangeText, ref _lastCASign);
    }

    private static void SetTextIfChanged(TextBlock target, string value, ref string? cache)
    {
        if (cache == value) return;
        cache = value;
        target.Text = value;
    }

    private static string FormatCurrency(double value) =>
        "$" + ((int)value).ToString("N0");

    private static void UpdateChangeIndicator(
        TextBlock iconBlock,
        TextBlock valueBlock,
        double change,
        ref string? cachedValueText,
        ref int cachedSign)
    {
        int sign = change >= 0 ? 1 : -1;
        if (sign != cachedSign)
        {
            cachedSign = sign;
            var brush = sign >= 0 ? PositiveBrush : NegativeBrush;
            iconBlock.Foreground = brush;
            valueBlock.Foreground = brush;
            iconBlock.Text = sign >= 0 ? "▲" : "▼";
        }

        string valueText = Math.Abs(change).ToString("F1") + "%";
        if (cachedValueText == valueText) return;
        cachedValueText = valueText;
        valueBlock.Text = valueText;
    }

    private static void UpdateCityChange(
        TextBlock changeBlock,
        double change,
        ref string? cachedText,
        ref int cachedSign)
    {
        int sign = change >= 0 ? 1 : -1;
        if (sign != cachedSign)
        {
            cachedSign = sign;
            changeBlock.Foreground = sign >= 0 ? PositiveBrush : NegativeBrush;
        }

        string text = (sign >= 0 ? "▲ +" : "▼ ") + Math.Abs(change).ToString("F1") + "%";
        if (cachedText == text) return;
        cachedText = text;
        changeBlock.Text = text;
    }
}
