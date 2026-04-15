using Microsoft.UI.Xaml.Media.Animation;
using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class StudioPage : Page
{
    private IStudioService? _studioService;

    public StudioPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += (_, _) => AnimationHelper.StopBorderBreathe(LicenseCard);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Entrance animations
        AnimationHelper.FadeUp(LicenseCard, 0);
        AnimationHelper.FadeUp(FeaturesSection, 100);
        AnimationHelper.FadeUp(ConnectorsSection, 200);

        // Border breathe on license card
        AnimationHelper.StartBorderBreathe(LicenseCard);

        var host = Helpers.HostHelper.GetHost();
        if (host is null) return;
        _studioService = host.Services.GetRequiredService<IStudioService>();

        // License card is now MVUX-bound; update badge foreground when validity changes
        UpdateBadgeForeground();

        // Load features + connectors (dynamic UI, still code-behind)
        var featuresTask = _studioService.GetFeaturesAsync(CancellationToken.None).AsTask();
        var connectorsTask = _studioService.GetConnectorsAsync(CancellationToken.None).AsTask();
        await Task.WhenAll(featuresTask, connectorsTask);

        PopulateFeatures(featuresTask.Result);
        PopulateConnectors(connectorsTask.Result);

        // Wire button handler
        RefreshButton.Click += OnRefreshClick;
    }

    private async void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        if (_studioService is null) return;
        RefreshButton.IsEnabled = false;
        RefreshButton.Content = "Refreshing...";
        try
        {
            // License card refreshes automatically via MVUX feeds
            // Update badge foreground color after refresh
            var status = await _studioService.GetStatusAsync(CancellationToken.None);
            UpdateBadgeForeground(status.IsValid);

            var features = await _studioService.GetFeaturesAsync(CancellationToken.None);
            PopulateFeatures(features);

            var connectors = await _studioService.GetConnectorsAsync(CancellationToken.None);
            PopulateConnectors(connectors);
        }
        finally
        {
            RefreshButton.Content = "Refresh";
            RefreshButton.IsEnabled = true;
        }
    }

    private async void UpdateBadgeForeground()
    {
        if (_studioService is null) return;
        var status = await _studioService.GetStatusAsync(CancellationToken.None);
        UpdateBadgeForeground(status.IsValid);
    }

    private void UpdateBadgeForeground(bool isValid)
    {
        LicenseBadgeText.Foreground = isValid
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"]
            : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalZinc500Brush"];
    }

    private void PopulateFeatures(ImmutableList<StudioFeature> features)
    {
        FeaturesGrid.Children.Clear();

        for (var i = 0; i < features.Count; i++)
        {
            var feature = features[i];
            var col = i % 3;
            var row = i / 3;

            // Ensure we have enough rows
            while (FeaturesGrid.RowDefinitions.Count <= row)
                FeaturesGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var card = new Border
            {
                Background = feature.IsEnabled
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface1Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface05Brush"],
                BorderBrush = feature.IsEnabled
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface3Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12),
                Margin = new Thickness(col == 0 ? 0 : 6, row == 0 ? 0 : 12, col == 2 ? 0 : 6, 0),
                Opacity = feature.IsEnabled ? 1.0 : 0.5,
            };

            var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };

            // Icon box
            var iconBox = new Border
            {
                Width = 32,
                Height = 32,
                CornerRadius = new CornerRadius(6),
                Background = feature.IsEnabled
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald500_15Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalZinc500_10Brush"],
            };
            iconBox.Child = new FontIcon
            {
                Glyph = feature.IsEnabled ? "\uE73E" : "\uE711",
                FontSize = 14,
                Foreground = feature.IsEnabled
                    ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalEmerald400Brush"]
                    : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalZinc500Brush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            content.Children.Add(iconBox);

            // Text
            var textStack = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
            textStack.Children.Add(new TextBlock
            {
                Text = feature.Name,
                Style = (Style)Application.Current.Resources["OrbitalTabLabel"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText75Brush"],
                FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            });
            textStack.Children.Add(new TextBlock
            {
                Text = feature.Description,
                Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText35Brush"],
            });
            content.Children.Add(textStack);

            card.Child = content;
            Grid.SetColumn(card, col);
            Grid.SetRow(card, row);
            FeaturesGrid.Children.Add(card);
        }
    }

    private void PopulateConnectors(ImmutableList<McpConnector> connectors)
    {
        ConnectorsList.Children.Clear();
        foreach (var connector in connectors)
        {
            var row = new Border
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface15Brush"],
                BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalSurface2Brush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12),
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(16) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition { Width = new GridLength(12) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                },
            };

            // Status dot
            var dot = new Controls.StatusDot
            {
                Status = connector.Connected ? "ok" : "idle",
                DotSize = 8,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(dot, 0);
            grid.Children.Add(dot);

            // Text
            var textStack = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
            textStack.Children.Add(new TextBlock
            {
                Text = connector.Name,
                Style = (Style)Application.Current.Resources["OrbitalBody"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText75Brush"],
            });
            textStack.Children.Add(new TextBlock
            {
                Text = connector.Connected ? $"{connector.Url} · {connector.ToolCount} tools" : connector.Url,
                Style = (Style)Application.Current.Resources["OrbitalMonoSmall"],
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["OrbitalText35Brush"],
            });
            Grid.SetColumn(textStack, 2);
            grid.Children.Add(textStack);

            // Pulsing bars (if connected)
            if (connector.Connected)
            {
                var bars = new Controls.PulsingBars
                {
                    BarCount = 3,
                    BarColor = "emerald",
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(bars, 3);
                grid.Children.Add(bars);
            }

            // Action button
            var connectorUrl = connector.Url;
            var button = new Button
            {
                Content = connector.Connected ? "Configure" : "Connect",
                Style = connector.Connected
                    ? (Style)Application.Current.Resources["OrbitalGhostButtonSm"]
                    : (Style)Application.Current.Resources["OrbitalSecondaryButtonSm"],
            };
            button.Click += async (s, _) =>
            {
                // Open the connector URL or MCP config
                if (!string.IsNullOrEmpty(connectorUrl))
                {
                    try
                    {
                        var uri = connectorUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                            ? new Uri(connectorUrl)
                            : new Uri($"https://{connectorUrl}");
                        await Windows.System.Launcher.LaunchUriAsync(uri);
                    }
                    catch { /* URL not launchable */ }
                }
            };
            Grid.SetColumn(button, 5);
            grid.Children.Add(button);

            row.Child = grid;
            ConnectorsList.Children.Add(row);
        }
    }
}
