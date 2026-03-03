using EnterpriseDashboard.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using SkiaSharp;

namespace EnterpriseDashboard.Views;

public sealed partial class ObservatoryPage : Page
{
    private readonly IObservatoryService _service;
    private bool _initialized;

    // Cached data for theme switching
    private IImmutableList<double>? _signalData;
    private IImmutableList<(string Label, double Value)>? _throughputData;
    private IImmutableList<(string Label, double[] Values)>? _stackedData;
    private IImmutableList<(double X, double Y, double Weight)>? _scatterData;
    private IImmutableList<(string Category, double Value)>? _rankedData;
    private IImmutableList<(string Axis, double Value)>? _radarData;
    private IImmutableList<CandlestickPoint>? _candlestickData;

    public ObservatoryPage()
    {
        this.InitializeComponent();
        _service = ((App)Application.Current).Host!.Services.GetRequiredService<IObservatoryService>();
        this.Loaded += ObservatoryPage_Loaded;
    }

    private async void ObservatoryPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Sync toggle with current theme
        if (ThemeManager.Current == DashboardTheme.Terminal)
        {
            ThemeToggle.IsOn = true;
        }

        RunEntranceAnimations();

        var ct = CancellationToken.None;

        // Load all data
        _signalData = await _service.GetSignalAmplitudeAsync(ct);
        _throughputData = await _service.GetMonthlyThroughputAsync(ct);
        _stackedData = await _service.GetCumulativeLoadAsync(ct);
        _scatterData = await _service.GetCorrelationDataAsync(ct);
        _rankedData = await _service.GetRankedDistributionAsync(ct);
        _radarData = await _service.GetRadarMetricsAsync(ct);
        _candlestickData = await _service.GetCandlestickDataAsync(ct);

        // Apply LiveCharts2 theme
        ApplyLiveChartsTheme();

        // Load custom SkiaSharp charts
        var isTerminal = ThemeManager.Current == DashboardTheme.Terminal;
        await LoadCustomChartsAsync(ct, isTerminal);

        ApplyBadgeColors();
        _initialized = true;
    }

    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_initialized) return;

        var isTerminal = ThemeToggle.IsOn;
        ThemeManager.Current = isTerminal ? DashboardTheme.Terminal : DashboardTheme.Monochrome;

        // Swap Material palette
        SwapColorPalette(isTerminal);

        // Re-apply all chart themes
        ApplyLiveChartsTheme();
        UpdateCustomChartThemes(isTerminal);
        ApplyBadgeColors();
    }

    private void SwapColorPalette(bool terminal)
    {
        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        var palettePath = terminal
            ? "ms-appx:///Styles/TerminalPaletteOverride.xaml"
            : "ms-appx:///Styles/ColorPaletteOverride.xaml";

        var paletteDict = new ResourceDictionary();
        paletteDict.Source = new Uri(palettePath);

        for (int i = mergedDicts.Count - 1; i >= 0; i--)
        {
            if (mergedDicts[i] is ResourceDictionary rd && rd.Source?.OriginalString.Contains("PaletteOverride") == true)
            {
                mergedDicts.RemoveAt(i);
            }
        }

        mergedDicts.Add(paletteDict);

        if (this.XamlRoot?.Content is FrameworkElement root)
        {
            root.RequestedTheme = ElementTheme.Light;
            root.RequestedTheme = ElementTheme.Dark;
        }
    }

    private void ApplyBadgeColors()
    {
        var colors = ThemeManager.GetColors();
        var accentColor = ParseColor(colors.AccentHex);
        var accentBgColor = ParseColor(colors.AccentBgHex);
        var glowColor = ParseColor(colors.GlowBorderHex);

        Resources["VendorBadgeBrush"] = new SolidColorBrush(accentColor);
        Resources["VendorBadgeBgBrush"] = new SolidColorBrush(accentBgColor);

        var cards = new Border[]
        {
            Card01, Card02, Card03, Card04, Card05, Card06,
            Card07, Card08, Card09, Card10, Card11, Card12,
            Card13, Card14
        };
        foreach (var card in cards)
        {
            card.BorderBrush = new SolidColorBrush(glowColor);
        }
    }

    #region LiveCharts2 Charts

    private void ApplyLiveChartsTheme()
    {
        if (_signalData == null) return;

        var colors = ThemeManager.GetColors();
        var animSpeed = TimeSpan.FromMilliseconds(800);

        ApplySignalChart(colors, animSpeed);
        ApplyThroughputChart(colors, animSpeed);
        ApplyStackedAreaChart(colors, animSpeed);
        ApplyScatterChart(colors, animSpeed);
        ApplyHBarChart(colors, animSpeed);
        ApplyRadarChart(colors, animSpeed);
        ApplyCandlestickChart(colors, animSpeed);
    }

    private void ApplySignalChart(ThemeColors c, TimeSpan speed)
    {
        SignalChart.AnimationsSpeed = speed;
        SignalChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;
        SignalChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
        SignalChart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = _signalData!.ToArray(),
                Stroke = new SolidColorPaint(c.LineStroke) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(c.LineFillTop),
                GeometryStroke = new SolidColorPaint(c.LineStroke) { StrokeThickness = 2 },
                GeometrySize = 4,
                GeometryFill = new SolidColorPaint(c.GeometryFill),
                LineSmoothness = 0.6
            }
        };
        SignalChart.XAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
        SignalChart.YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
    }

    private void ApplyThroughputChart(ThemeColors c, TimeSpan speed)
    {
        ThroughputChart.AnimationsSpeed = speed;
        ThroughputChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;
        ThroughputChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
        ThroughputChart.Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = _throughputData!.Select(t => t.Value).ToArray(),
                Fill = new SolidColorPaint(new SKColor(0xCC, 0xCC, 0xCC)),
                Stroke = new SolidColorPaint(c.LineStroke) { StrokeThickness = 1 },
                MaxBarWidth = 24
            }
        };
        ThroughputChart.XAxes = new Axis[]
        {
            new Axis
            {
                Labels = _throughputData!.Select(t => t.Label).ToArray(),
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
        ThroughputChart.YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
    }

    private void ApplyStackedAreaChart(ThemeColors c, TimeSpan speed)
    {
        if (_stackedData == null) return;

        StackedAreaChart.AnimationsSpeed = speed;
        StackedAreaChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;
        StackedAreaChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;

        var seriesColors = new SKColor[]
        {
            new(0xFF, 0xFF, 0xFF, 0x90),
            new(0xCC, 0xCC, 0xCC, 0x70),
            new(0x99, 0x99, 0x99, 0x50)
        };
        var strokeColors = new SKColor[]
        {
            new(0xFF, 0xFF, 0xFF),
            new(0xCC, 0xCC, 0xCC),
            new(0x99, 0x99, 0x99)
        };

        var series = new List<ISeries>();
        for (int s = 0; s < 3; s++)
        {
            int si = s;
            series.Add(new StackedAreaSeries<double>
            {
                Values = _stackedData.Select(d => d.Values[si]).ToArray(),
                Fill = new SolidColorPaint(seriesColors[si]),
                Stroke = new SolidColorPaint(strokeColors[si]) { StrokeThickness = 1.5f },
                GeometrySize = 0,
                LineSmoothness = 0.5
            });
        }
        StackedAreaChart.Series = series.ToArray();
        StackedAreaChart.XAxes = new Axis[]
        {
            new Axis
            {
                Labels = _stackedData.Select(d => d.Label).ToArray(),
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
        StackedAreaChart.YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
    }

    private void ApplyScatterChart(ThemeColors c, TimeSpan speed)
    {
        if (_scatterData == null) return;

        ScatterChart.AnimationsSpeed = speed;
        ScatterChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;
        ScatterChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
        ScatterChart.Series = new ISeries[]
        {
            new ScatterSeries<ObservablePoint>
            {
                Values = _scatterData.Select(p => new ObservablePoint(p.X, p.Y)).ToArray(),
                GeometrySize = 8,
                Stroke = new SolidColorPaint(new SKColor(0xFF, 0xFF, 0xFF, 0xCC)) { StrokeThickness = 1 },
                Fill = new SolidColorPaint(new SKColor(0xFF, 0xFF, 0xFF, 0x60))
            }
        };
        ScatterChart.XAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
        ScatterChart.YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
    }

    private void ApplyHBarChart(ThemeColors c, TimeSpan speed)
    {
        if (_rankedData == null) return;

        HBarChart.AnimationsSpeed = speed;
        HBarChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;
        HBarChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
        HBarChart.Series = new ISeries[]
        {
            new RowSeries<double>
            {
                Values = _rankedData.Select(r => r.Value).ToArray(),
                Fill = new SolidColorPaint(new SKColor(0xCC, 0xCC, 0xCC)),
                Stroke = new SolidColorPaint(c.LineStroke) { StrokeThickness = 1 },
                MaxBarWidth = 20
            }
        };
        HBarChart.YAxes = new Axis[]
        {
            new Axis
            {
                Labels = _rankedData.Select(r => r.Category).ToArray(),
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
        HBarChart.XAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
    }

    private void ApplyRadarChart(ThemeColors c, TimeSpan speed)
    {
        if (_radarData == null) return;

        RadarChart.AnimationsSpeed = speed;
        RadarChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;
        RadarChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;
        RadarChart.Series = new ISeries[]
        {
            new PolarLineSeries<double>
            {
                Values = _radarData.Select(r => r.Value).ToArray(),
                Stroke = new SolidColorPaint(c.LineStroke) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(c.LineFillTop),
                GeometrySize = 6,
                GeometryFill = new SolidColorPaint(c.GeometryFill),
                GeometryStroke = new SolidColorPaint(c.LineStroke) { StrokeThickness = 2 },
                LineSmoothness = 0,
                IsClosed = true
            }
        };
        RadarChart.AngleAxes = new PolarAxis[]
        {
            new PolarAxis
            {
                Labels = _radarData.Select(r => r.Axis).ToArray(),
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 },
                MinStep = 1,
                ForceStepToMin = true
            }
        };
        RadarChart.RadiusAxes = new PolarAxis[]
        {
            new PolarAxis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
    }

    private void ApplyCandlestickChart(ThemeColors c, TimeSpan speed)
    {
        if (_candlestickData == null) return;

        CandlestickChart.AnimationsSpeed = speed;
        CandlestickChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;
        CandlestickChart.TooltipPosition = LiveChartsCore.Measure.TooltipPosition.Hidden;

        var financialData = _candlestickData.Select(p =>
            new FinancialPoint(p.Date, p.High, p.Open, p.Close, p.Low)).ToArray();

        CandlestickChart.Series = new ISeries[]
        {
            new CandlesticksSeries<FinancialPoint>
            {
                Values = financialData,
                UpStroke = new SolidColorPaint(c.LineStroke) { StrokeThickness = 1.5f },
                UpFill = new SolidColorPaint(SKColors.Transparent),
                DownStroke = new SolidColorPaint(c.Label) { StrokeThickness = 1.5f },
                DownFill = new SolidColorPaint(c.Label)
            }
        };
        CandlestickChart.XAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 },
                Labeler = val =>
                {
                    var idx = (int)val;
                    if (idx >= 0 && idx < _candlestickData.Count)
                        return _candlestickData[idx].Date.ToString("dd");
                    return "";
                }
            }
        };
        CandlestickChart.YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(c.Label),
                SeparatorsPaint = new SolidColorPaint(c.GridLine) { StrokeThickness = 1 }
            }
        };
    }

    #endregion

    #region Custom SkiaSharp Charts

    private async Task LoadCustomChartsAsync(CancellationToken ct, bool terminal)
    {
        var heatmapData = await _service.GetHeatmapDataAsync(ct);
        HeatmapCanvas.SetData(heatmapData, terminal);

        var arcData = await _service.GetArcGaugeDataAsync(ct);
        ArcGaugeCanvas.SetData(arcData, terminal);

        var (filled, total) = await _service.GetWaffleDataAsync(ct);
        WaffleCanvas.SetData(filled, total, terminal);

        var (value, min, max) = await _service.GetGaugeValueAsync(ct);
        GaugeCanvas.SetData(value, min, max, terminal);

        var (nodes, edges) = await _service.GetNetworkNodesAsync(ct);
        NetworkCanvas.SetData(nodes, edges, terminal);

        var treemapData = await _service.GetTreemapDataAsync(ct);
        TreemapCanvas.SetData(treemapData, terminal);

        var funnelData = await _service.GetFunnelDataAsync(ct);
        FunnelCanvas.SetData(funnelData, terminal);
    }

    private void UpdateCustomChartThemes(bool terminal)
    {
        HeatmapCanvas.SetTheme(terminal);
        ArcGaugeCanvas.SetTheme(terminal);
        WaffleCanvas.SetTheme(terminal);
        GaugeCanvas.SetTheme(terminal);
        NetworkCanvas.SetTheme(terminal);
        TreemapCanvas.SetTheme(terminal);
        FunnelCanvas.SetTheme(terminal);
    }

    #endregion

    #region Entrance Animations

    private void RunEntranceAnimations()
    {
        var cards = new FrameworkElement[]
        {
            Card01, Card02, Card03, Card04, Card05, Card06,
            Card07, Card08, Card09, Card10, Card11, Card12,
            Card13, Card14
        };

        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            card.Opacity = 0;
            card.RenderTransform = new TranslateTransform { Y = 24 };

            var storyboard = new Storyboard();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                BeginTime = TimeSpan.FromMilliseconds(i * 80),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeIn, card);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");
            storyboard.Children.Add(fadeIn);

            var slideUp = new DoubleAnimation
            {
                From = 24,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                BeginTime = TimeSpan.FromMilliseconds(i * 80),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slideUp, card.RenderTransform);
            Storyboard.SetTargetProperty(slideUp, "Y");
            storyboard.Children.Add(slideUp);

            storyboard.Begin();
        }
    }

    #endregion

    private static Windows.UI.Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        byte a = byte.Parse(hex[..2], System.Globalization.NumberStyles.HexNumber);
        byte r = byte.Parse(hex[2..4], System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex[4..6], System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex[6..8], System.Globalization.NumberStyles.HexNumber);
        return Windows.UI.Color.FromArgb(a, r, g, b);
    }
}
