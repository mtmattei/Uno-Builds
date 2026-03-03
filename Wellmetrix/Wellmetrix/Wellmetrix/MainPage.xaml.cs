using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Wellmetrix.Controls;
using Wellmetrix.Models;
using Wellmetrix.Presentation;

namespace Wellmetrix;

public sealed partial class MainPage : Page
{
    private BodyExplorerViewModel? _viewModel;
    private readonly Dictionary<string, Button> _organMarkers = new();

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnPageLoaded;
        this.Unloaded += OnPageUnloaded;
        this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = args.NewValue as BodyExplorerViewModel;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        DetachOrganChipHandlers();

        this.Loaded -= OnPageLoaded;
        this.Unloaded -= OnPageUnloaded;
        this.DataContextChanged -= OnDataContextChanged;
    }

    private void DetachOrganChipHandlers()
    {
        foreach (var child in OrganSelector.Children)
        {
            if (child is Button button)
            {
                button.Click -= OnOrganChipClick;
            }
        }
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Cache organ markers
        _organMarkers["brain"] = BrainMarker;
        _organMarkers["heart"] = HeartMarker;
        _organMarkers["lungs"] = LungsMarker;
        _organMarkers["liver"] = LiverMarker;
        _organMarkers["kidneys"] = KidneysMarker;
        _organMarkers["pancreas"] = PancreasMarker;

        // Build organ selector chips
        BuildOrganSelector();

        // Update UI with initial selection
        UpdateUI();
    }

    private void BuildOrganSelector()
    {
        if (_viewModel == null) return;

        DetachOrganChipHandlers();
        OrganSelector.Children.Clear();

        foreach (var organ in _viewModel.Organs)
        {
            var isSelected = _viewModel.SelectedOrgan?.Id == organ.Id;
            var chip = CreateOrganChip(organ, isSelected);
            OrganSelector.Children.Add(chip);
        }
    }

    private Button CreateOrganChip(Organ organ, bool isSelected)
    {
        var accentBrush = GetAccentBrush(organ.AccentColorKey);

        var stack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        stack.Children.Add(new FontIcon
        {
            Glyph = organ.Icon,
            FontSize = 16,
            Foreground = isSelected ? accentBrush : (Brush)Application.Current.Resources["TextSecondaryBrush"]
        });

        stack.Children.Add(new TextBlock
        {
            Text = organ.Name,
            Style = (Style)Application.Current.Resources["BodyTextStyle"],
            Foreground = isSelected ? (Brush)Application.Current.Resources["TextPrimaryBrush"] : (Brush)Application.Current.Resources["TextSecondaryBrush"],
            VerticalAlignment = VerticalAlignment.Center
        });

        var border = new Border
        {
            Style = isSelected
                ? (Style)Application.Current.Resources["GlassPillSelectedStyle"]
                : (Style)Application.Current.Resources["GlassPillStyle"],
            Child = stack
        };

        if (isSelected)
        {
            border.BorderBrush = accentBrush;
        }

        var button = new Button
        {
            Content = border,
            Tag = organ.Id,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0)
        };

        button.Click += OnOrganChipClick;
        return button;
    }

    private void OnOrganChipClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string organId)
        {
            _viewModel?.SelectOrgan(organId);
        }
    }

    private void OnOrganMarkerClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string organId)
        {
            _viewModel?.SelectOrgan(organId);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BodyExplorerViewModel.SelectedOrgan))
        {
            UpdateUI();
            BuildOrganSelector(); // Rebuild to update selection state
        }
    }

    private void UpdateUI()
    {
        var organ = _viewModel?.SelectedOrgan;
        if (organ == null) return;

        var accentBrush = GetAccentBrush(organ.AccentColorKey);

        // Update score card
        OrganNameDisplay.Text = organ.Name;
        OrganIconDisplay.Glyph = organ.Icon;
        OrganIconDisplay.Foreground = accentBrush;
        StatusText.Text = organ.Status;
        ScoreProgress.Value = organ.HealthScore;
        ScoreProgress.AccentBrush = accentBrush;

        // Update trend chart
        WeeklyTrendChart.Data = organ.WeeklyTrend;
        WeeklyTrendChart.AccentBrush = accentBrush;

        // Update metrics
        UpdateMetrics(organ.Metrics, accentBrush);

        // Update insights
        UpdateInsights(organ.Insights);

        // Update organ marker highlights
        UpdateMarkerHighlights(organ.Id);

        // Update glow color
        UpdateGlowColor(organ.AccentColorKey);
    }

    private void UpdateMetrics(IReadOnlyList<HealthMetric> metrics, Brush accentBrush)
    {
        var metricCards = new[] { Metric1, Metric2, Metric3, Metric4 };

        for (int i = 0; i < metricCards.Length && i < metrics.Count; i++)
        {
            var card = metricCards[i];
            var metric = metrics[i];

            card.Label = metric.Label;
            card.Value = metric.Value;
            card.Unit = metric.Unit;
            card.Trend = metric.Trend;
            card.ChangePercentage = metric.ChangePercentage;
            card.MinRange = metric.MinRange;
            card.MaxRange = metric.MaxRange;
            card.SparklineData = metric.SparklineData;
            card.AccentBrush = accentBrush;
        }
    }

    private void UpdateInsights(IReadOnlyList<Insight> insights)
    {
        InsightsContainer.Children.Clear();

        foreach (var insight in insights)
        {
            var insightCard = new InsightCard
            {
                Message = insight.Message,
                InsightType = insight.Type
            };
            InsightsContainer.Children.Add(insightCard);
        }
    }

    private void UpdateMarkerHighlights(string selectedId)
    {
        foreach (var kvp in _organMarkers)
        {
            var isSelected = kvp.Key == selectedId;
            var marker = kvp.Value;

            if (marker.Content is Grid grid)
            {
                // Scale effect for selected marker
                var scale = isSelected ? 1.2 : 1.0;
                grid.RenderTransform = new ScaleTransform { ScaleX = scale, ScaleY = scale };
                grid.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            }
        }
    }

    private void UpdateGlowColor(string accentColorKey)
    {
        var colorKey = accentColorKey.Replace("Brush", "Color");
        if (Application.Current.Resources.TryGetValue(colorKey, out var colorObj) && colorObj is Windows.UI.Color color)
        {
            var gradient = new RadialGradientBrush();
            gradient.GradientStops.Add(new GradientStop { Color = color, Offset = 0 });
            gradient.GradientStops.Add(new GradientStop { Color = Microsoft.UI.Colors.Transparent, Offset = 1 });
            OrganGlow.Background = gradient;
        }
    }

    private Brush GetAccentBrush(string accentColorKey)
    {
        if (Application.Current.Resources.TryGetValue(accentColorKey, out var brush) && brush is Brush b)
        {
            return b;
        }
        return (Brush)Application.Current.Resources["LungsAccentBrush"];
    }
}
