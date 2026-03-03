using System.Text.Json;
using BruTile;
using BruTile.Predefined;
using BruTile.Web;
using EnterpriseDashboard.Models;
using EnterpriseDashboard.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using SkiaSharp;
using Windows.UI;

namespace EnterpriseDashboard.Views;

public sealed partial class DashboardPage : Page
{
    private readonly IDashboardService _service;
    private IImmutableList<ChartDataPoint>? _revenueData;
    private IImmutableList<ChartDataPoint>? _regionData;
    private bool _initialized;

    public DashboardPage()
    {
        this.InitializeComponent();

        _service = ((App)Application.Current).Host!.Services.GetRequiredService<IDashboardService>();
        DataContext = new DashboardViewModel(_service);

        this.Loaded += DashboardPage_Loaded;
    }

    private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        RunEntranceAnimations();

        // Apply initial palette if not monochrome
        if (ThemeManager.Current == DashboardTheme.Terminal)
        {
            SwapColorPalette(true);
            ThemeLabel.Text = "TERMINAL NEON";
        }

        // Cache data for theme switching
        var ct = CancellationToken.None;
        _revenueData = await _service.GetRevenueSeriesAsync(ct);
        _regionData = await _service.GetRegionRevenueBreakdownAsync(ct);

        ApplyChartTheme();
        await InitializeTableAsync();
        await InitializeMapAsync();
        ApplyXamlAccents();
        _initialized = true;
    }

    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_initialized) return;

        var isTerminal = ThemeToggle.IsOn;
        ThemeManager.Current = isTerminal ? DashboardTheme.Terminal : DashboardTheme.Monochrome;

        // Swap the Material color palette at runtime
        SwapColorPalette(isTerminal);

        // Re-apply all vendor control styling
        ApplyChartTheme();
        ApplyMapTheme();
        ApplyXamlAccents();

        ThemeLabel.Text = isTerminal ? "TERMINAL NEON" : "OBSIDIAN MONO";
    }

    private void SwapColorPalette(bool terminal)
    {
        var app = Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        // Find and remove the current MaterialToolkitTheme, then re-add with new palette
        // Since we can't easily swap ColorOverrideSource at runtime, we'll overlay
        // key color resources directly onto the application resources
        var palettePath = terminal
            ? "ms-appx:///Styles/TerminalPaletteOverride.xaml"
            : "ms-appx:///Styles/ColorPaletteOverride.xaml";

        var paletteDict = new ResourceDictionary();
        paletteDict.Source = new Uri(palettePath);

        // Remove any previously injected palette overlay
        for (int i = mergedDicts.Count - 1; i >= 0; i--)
        {
            if (mergedDicts[i] is ResourceDictionary rd && rd.Source?.OriginalString.Contains("PaletteOverride") == true)
            {
                mergedDicts.RemoveAt(i);
            }
        }

        mergedDicts.Add(paletteDict);

        // Force theme refresh on the page tree
        if (this.XamlRoot?.Content is FrameworkElement root)
        {
            root.RequestedTheme = ElementTheme.Light;
            root.RequestedTheme = ElementTheme.Dark;
        }
    }

    private void ApplyXamlAccents()
    {
        var colors = ThemeManager.GetColors();
        var accentColor = ParseColor(colors.AccentHex);
        var accentBgColor = ParseColor(colors.AccentBgHex);
        var glowColor = ParseColor(colors.GlowBorderHex);
        var altRowColor = ParseColor(colors.AlternateRowHex);

        // Update page-level badge brushes
        Resources["VendorBadgeBrush"] = new SolidColorBrush(accentColor);
        Resources["VendorBadgeBgBrush"] = new SolidColorBrush(accentBgColor);

        // Update table alternate row
        SalesTableView.AlternateRowBackground = new SolidColorBrush(altRowColor);

        // Update all card panel borders with glow effect
        var panels = new Border[] { ChartLeftPanel, ChartRightPanel, TablePanel, MapPanel };
        foreach (var panel in panels)
        {
            panel.BorderBrush = new SolidColorBrush(glowColor);
        }
    }

    private void ApplyChartTheme()
    {
        if (_revenueData == null || _regionData == null) return;

        var colors = ThemeManager.GetColors();

        // Revenue line chart
        RevenueChart.AnimationsSpeed = TimeSpan.FromMilliseconds(800);
        RevenueChart.EasingFunction = LiveChartsCore.EasingFunctions.CubicOut;

        RevenueChart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = _revenueData.Select(p => p.Value).ToArray(),
                Name = "Revenue",
                Stroke = new SolidColorPaint(colors.LineStroke) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(colors.LineFillTop),
                GeometryStroke = new SolidColorPaint(colors.LineStroke) { StrokeThickness = 2 },
                GeometrySize = 5,
                GeometryFill = new SolidColorPaint(colors.GeometryFill),
                LineSmoothness = 0.5
            }
        };
        RevenueChart.XAxes = new Axis[]
        {
            new Axis
            {
                Labels = _revenueData.Select(p => p.Date.ToString("MMM yy")).ToArray(),
                LabelsPaint = new SolidColorPaint(colors.Label),
                SeparatorsPaint = new SolidColorPaint(colors.GridLine) { StrokeThickness = 1 },
                LabelsRotation = 45
            }
        };
        RevenueChart.YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(colors.Label),
                SeparatorsPaint = new SolidColorPaint(colors.GridLine) { StrokeThickness = 1 }
            }
        };

        // Pie chart
        RegionPieChart.AnimationsSpeed = TimeSpan.FromMilliseconds(1000);
        RegionPieChart.EasingFunction = LiveChartsCore.EasingFunctions.BounceOut;
        RegionPieChart.Series = _regionData
            .Select((point, i) => new PieSeries<double>
            {
                Values = new[] { point.Value },
                Name = point.Category,
                Fill = new SolidColorPaint(colors.PiePalette[i % colors.PiePalette.Length]),
                Stroke = new SolidColorPaint(colors.Surface) { StrokeThickness = 2 }
            } as ISeries)
            .ToArray();
    }

    private void ApplyMapTheme()
    {
        var colors = ThemeManager.GetColors();
        var map = RegionMap.Map;

        // Update pin layer colors
        var pinLayer = map.Layers.FirstOrDefault(l => l.Name == "Region Pins") as MemoryLayer;
        if (pinLayer != null)
        {
            foreach (var feature in pinLayer.Features)
            {
                if (feature is PointFeature pf && pf.Styles.FirstOrDefault() is SymbolStyle ss)
                {
                    ss.Fill = new Mapsui.Styles.Brush(colors.PinFill);
                    ss.Outline = new Mapsui.Styles.Pen(colors.PinOutline, 2);
                }
            }
        }

        // Update route layer colors
        var routeLayer = map.Layers.FirstOrDefault(l => l.Name == "Driving Route") as MemoryLayer;
        if (routeLayer != null)
        {
            foreach (var feature in routeLayer.Features)
            {
                if (feature.Styles.FirstOrDefault() is VectorStyle vs)
                {
                    vs.Line = new Mapsui.Styles.Pen(colors.RouteLine, 3);
                }
            }
        }

        // Also check fallback route
        var fallbackLayer = map.Layers.FirstOrDefault(l => l.Name == "Route Fallback") as MemoryLayer;
        if (fallbackLayer != null)
        {
            foreach (var feature in fallbackLayer.Features)
            {
                if (feature.Styles.FirstOrDefault() is VectorStyle vs)
                {
                    vs.Line = new Mapsui.Styles.Pen(colors.RouteLine, 2) { PenStyle = PenStyle.Dash };
                }
            }
        }

        map.RefreshData();
    }

    private void RunEntranceAnimations()
    {
        var panels = new FrameworkElement[] { KpiPanel, ChartLeftPanel, ChartRightPanel, TablePanel, MapPanel };
        for (int i = 0; i < panels.Length; i++)
        {
            var panel = panels[i];
            panel.Opacity = 0;
            panel.RenderTransform = new TranslateTransform { Y = 24 };

            var storyboard = new Storyboard();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                BeginTime = TimeSpan.FromMilliseconds(i * 120),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeIn, panel);
            Storyboard.SetTargetProperty(fadeIn, "Opacity");
            storyboard.Children.Add(fadeIn);

            var slideUp = new DoubleAnimation
            {
                From = 24,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                BeginTime = TimeSpan.FromMilliseconds(i * 120),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slideUp, panel.RenderTransform);
            Storyboard.SetTargetProperty(slideUp, "Y");
            storyboard.Children.Add(slideUp);

            storyboard.Begin();
        }
    }

    private async Task InitializeTableAsync()
    {
        var ct = CancellationToken.None;
        var sales = await _service.GetSalesAsync(ct);
        SalesTableView.ItemsSource = sales.ToList();
    }

    private async Task InitializeMapAsync()
    {
        var ct = CancellationToken.None;
        var regions = await _service.GetRegionsAsync(ct);
        var colors = ThemeManager.GetColors();

        var map = RegionMap.Map;

        // CARTO Dark Matter tiles
        var cartoDarkSource = new HttpTileSource(
            new GlobalSphericalMercator(),
            "https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png",
            new[] { "a", "b", "c", "d" },
            name: "CARTO Dark Matter",
            attribution: new Attribution("CARTO, OpenStreetMap contributors", "https://carto.com/attributions"),
            configureHttpRequestMessage: r =>
                r.Headers.TryAddWithoutValidation("User-Agent", "EnterpriseDashboard/1.0")
        );
        map.Layers.Add(new TileLayer(cartoDarkSource) { Name = "Dark Basemap" });

        // Region pin layer
        var pinLayer = new MemoryLayer
        {
            Name = "Region Pins",
            Features = regions.Select(r =>
            {
                var point = SphericalMercator.FromLonLat(r.Longitude, r.Latitude);
                var feature = new PointFeature(new MPoint(point.x, point.y));
                feature["name"] = r.Name;
                feature["value"] = r.Value;
                feature.Styles.Add(new SymbolStyle
                {
                    SymbolScale = 0.4 + (r.Value / 5_000_000.0) * 0.6,
                    Fill = new Mapsui.Styles.Brush(colors.PinFill),
                    Outline = new Mapsui.Styles.Pen(colors.PinOutline, 2)
                });
                return feature as IFeature;
            }).ToList(),
            Style = null
        };
        map.Layers.Add(pinLayer);

        // Fetch real driving directions from OSRM
        var waypointCoords = new (double Lon, double Lat)[]
        {
            (2.3522, 48.8566),     // Paris
            (4.3517, 50.8503),     // Brussels
            (6.9603, 50.9375),     // Cologne
            (8.6821, 50.1109),     // Frankfurt
            (11.5820, 48.1351),    // Munich
            (13.4050, 52.5200),    // Berlin
        };

        var coordString = string.Join(";", waypointCoords.Select(c => $"{c.Lon},{c.Lat}"));
        var osrmUrl = $"https://router.project-osrm.org/route/v1/driving/{coordString}?overview=full&geometries=geojson";

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "EnterpriseDashboard/1.0");
            var json = await httpClient.GetStringAsync(osrmUrl);
            var doc = JsonDocument.Parse(json);

            var coordinates = doc.RootElement
                .GetProperty("routes")[0]
                .GetProperty("geometry")
                .GetProperty("coordinates");

            var routeMercatorPoints = new List<NetTopologySuite.Geometries.Coordinate>();
            foreach (var coord in coordinates.EnumerateArray())
            {
                var lon = coord[0].GetDouble();
                var lat = coord[1].GetDouble();
                var merc = SphericalMercator.FromLonLat(lon, lat);
                routeMercatorPoints.Add(new NetTopologySuite.Geometries.Coordinate(merc.x, merc.y));
            }

            if (routeMercatorPoints.Count >= 2)
            {
                var routeLine = new NetTopologySuite.Geometries.LineString(routeMercatorPoints.ToArray());
                var routeFeature = new GeometryFeature(routeLine);
                routeFeature.Styles.Add(new VectorStyle
                {
                    Line = new Mapsui.Styles.Pen(colors.RouteLine, 3)
                });

                map.Layers.Add(new MemoryLayer
                {
                    Name = "Driving Route",
                    Features = new List<IFeature> { routeFeature },
                    Style = null
                });
            }
        }
        catch
        {
            // Fallback: straight lines if OSRM is unreachable
            var fallbackPoints = waypointCoords
                .Select(c => SphericalMercator.FromLonLat(c.Lon, c.Lat))
                .Select(p => new NetTopologySuite.Geometries.Coordinate(p.x, p.y))
                .ToArray();

            if (fallbackPoints.Length >= 2)
            {
                var fallbackLine = new NetTopologySuite.Geometries.LineString(fallbackPoints);
                var fallbackFeature = new GeometryFeature(fallbackLine);
                fallbackFeature.Styles.Add(new VectorStyle
                {
                    Line = new Mapsui.Styles.Pen(colors.RouteLine, 2) { PenStyle = PenStyle.Dash }
                });

                map.Layers.Add(new MemoryLayer
                {
                    Name = "Route Fallback",
                    Features = new List<IFeature> { fallbackFeature },
                    Style = null
                });
            }
        }

        // Zoom to Western Europe
        var minBounds = SphericalMercator.FromLonLat(-1, 47);
        var maxBounds = SphericalMercator.FromLonLat(16, 54);
        map.Navigator.ZoomToBox(new MRect(minBounds.x, minBounds.y, maxBounds.x, maxBounds.y));
        map.BackColor = new Mapsui.Styles.Color(0, 0, 0);
    }

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
