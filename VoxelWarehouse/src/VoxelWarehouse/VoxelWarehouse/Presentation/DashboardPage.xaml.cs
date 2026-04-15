using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;

namespace VoxelWarehouse.Presentation;

public sealed partial class DashboardPage : Page
{
    private static readonly SolidColorBrush PanelBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x11, 0x13, 0x18));
    private static readonly SolidColorBrush RaisedBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x18, 0x1B, 0x22));
    private static readonly SolidColorBrush BorderBrush_ = new(Windows.UI.Color.FromArgb(0xFF, 0x1F, 0x23, 0x2D));
    private static readonly SolidColorBrush AccentBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xD4, 0xAA));
    private static readonly SolidColorBrush AmberBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xE8, 0xA2, 0x30));
    private static readonly SolidColorBrush RedBrush = new(Windows.UI.Color.FromArgb(0xFF, 0xE8, 0x48, 0x55));
    private static readonly SolidColorBrush BlueBrush = new(Windows.UI.Color.FromArgb(0xFF, 0x4A, 0x9E, 0xF5));
    private static readonly SolidColorBrush T1Brush = new(Windows.UI.Color.FromArgb(0xFF, 0xE8, 0xE6, 0xE1));
    private static readonly SolidColorBrush T2Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x93, 0x98, 0xA3));
    private static readonly SolidColorBrush T3Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x5D, 0x63, 0x70));
    private static readonly SolidColorBrush T4Brush = new(Windows.UI.Color.FromArgb(0xFF, 0x3A, 0x3F, 0x4B));

    public DashboardPage()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildKpiCards();
        BuildActivityFeed();
        BuildAttentionCards();
        AnimateKpiCards();
    }

    private void BuildKpiCards()
    {
        KpiPanel.Children.Clear();

        var orders = SeedDataService.GetPurchaseOrders();
        var world = Presets.WarehouseA();
        var metrics = MetricsCalculator.Compute(world);

        int openPOs = orders.Count(o => o.Status != POStatus.Approved);
        int pending = orders.Count(o => o.Status == POStatus.Pending);
        decimal monthlySpend = orders.Sum(o => o.Amount) / 1000m;
        int atRisk = orders.Count(o => o.Risk == RiskLevel.High);

        var cards = new (string Label, string Value, SolidColorBrush Color)[]
        {
            ("OPEN POs", openPOs.ToString(), AmberBrush),
            ("PENDING APPROVAL", pending.ToString(), RedBrush),
            ("FLOOR UTILIZATION", $"{metrics.FloorUtilPercent}%", AccentBrush),
            ("MONTHLY SPEND", $"${monthlySpend:F0}K", BlueBrush),
            ("AT-RISK ORDERS", atRisk.ToString(), RedBrush),
        };

        foreach (var (label, value, color) in cards)
        {
            var card = CreateKpiCard(label, value, color);
            card.Opacity = 0;
            card.RenderTransform = new TranslateTransform { Y = 16 };
            KpiPanel.Children.Add(card);
        }
    }

    private static Border CreateKpiCard(string label, string value, SolidColorBrush accentColor)
    {
        var dimColor = new SolidColorBrush(Windows.UI.Color.FromArgb(0x14,
            accentColor.Color.R, accentColor.Color.G, accentColor.Color.B));

        return new Border
        {
            Background = PanelBrush,
            BorderBrush = BorderBrush_,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(16, 14, 16, 14),
            MinWidth = 160,
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            new Ellipse { Width = 6, Height = 6, Fill = accentColor, VerticalAlignment = VerticalAlignment.Center },
                            new TextBlock
                            {
                                Text = label,
                                FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                                FontSize = 7, FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                                CharacterSpacing = 200, Foreground = T3Brush
                            }
                        }
                    },
                    new TextBlock
                    {
                        Text = value,
                        FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                        FontSize = 22, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        Foreground = T1Brush
                    },
                    new Border
                    {
                        Height = 3, CornerRadius = new CornerRadius(1),
                        Background = dimColor,
                        Child = new Border
                        {
                            Height = 3, CornerRadius = new CornerRadius(1),
                            Background = accentColor, Opacity = 0.4,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Width = 40
                        }
                    }
                }
            }
        };
    }

    private async void AnimateKpiCards()
    {
        foreach (var child in KpiPanel.Children)
        {
            if (child is Border card)
            {
                var storyboard = new Storyboard();

                var fadeIn = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(fadeIn, card);
                Storyboard.SetTargetProperty(fadeIn, "Opacity");

                var slideUp = new DoubleAnimation
                {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                Storyboard.SetTarget(slideUp, card.RenderTransform);
                Storyboard.SetTargetProperty(slideUp, "Y");

                storyboard.Children.Add(fadeIn);
                storyboard.Children.Add(slideUp);
                storyboard.Begin();

                await Task.Delay(80);
            }
        }
    }

    private void BuildActivityFeed()
    {
        ActivityPanel.Children.Clear();
        var feed = SeedDataService.GetActivityFeed();

        foreach (var entry in feed)
        {
            var dotColor = entry.Type switch
            {
                "approval" => AccentBrush,
                "alert" => RedBrush,
                "warning" => AmberBrush,
                "update" => BlueBrush,
                _ => T4Brush
            };

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = entry.TimeAgo,
                        FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                        FontSize = 7, FontWeight = Microsoft.UI.Text.FontWeights.Light,
                        CharacterSpacing = 80, Foreground = T4Brush,
                        Width = 60, VerticalAlignment = VerticalAlignment.Center
                    },
                    new Ellipse { Width = 5, Height = 5, Fill = dotColor, VerticalAlignment = VerticalAlignment.Center },
                    new TextBlock
                    {
                        Text = entry.Message,
                        FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                        FontSize = 8, FontWeight = Microsoft.UI.Text.FontWeights.Light,
                        CharacterSpacing = 60, Foreground = T2Brush,
                        VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis
                    }
                }
            };

            ActivityPanel.Children.Add(row);
        }
    }

    private void BuildAttentionCards()
    {
        AttentionPanel.Children.Clear();
        var orders = SeedDataService.GetPurchaseOrders();
        var attention = orders.Where(o => o.Risk == RiskLevel.High || o.Status == POStatus.Flagged).Take(3);

        foreach (var po in attention)
        {
            var accentColor = po.Status == POStatus.Flagged ? RedBrush : AmberBrush;
            var dimColor = new SolidColorBrush(Windows.UI.Color.FromArgb(0x0A,
                accentColor.Color.R, accentColor.Color.G, accentColor.Color.B));

            var card = new Border
            {
                Background = PanelBrush,
                BorderBrush = BorderBrush_,
                BorderThickness = new Thickness(1, 1, 1, 1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(14, 12),
                Child = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(3) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    Children =
                    {
                        SetColumn(new Border
                        {
                            Background = accentColor, CornerRadius = new CornerRadius(1),
                            Width = 3, Margin = new Thickness(0, 0, 12, 0)
                        }, 0),
                        SetColumn(new StackPanel
                        {
                            Spacing = 4,
                            Children =
                            {
                                new StackPanel
                                {
                                    Orientation = Orientation.Horizontal, Spacing = 8,
                                    Children =
                                    {
                                        new TextBlock
                                        {
                                            Text = po.Id,
                                            FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                                            FontSize = 9, FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                                            CharacterSpacing = 80, Foreground = T1Brush
                                        },
                                        new TextBlock
                                        {
                                            Text = po.VendorName,
                                            FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                                            FontSize = 8, FontWeight = Microsoft.UI.Text.FontWeights.Light,
                                            CharacterSpacing = 60, Foreground = T3Brush
                                        }
                                    }
                                },
                                new TextBlock
                                {
                                    Text = po.AiAlertText ?? po.Detail,
                                    FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                                    FontSize = 7, FontWeight = Microsoft.UI.Text.FontWeights.Light,
                                    CharacterSpacing = 60, Foreground = T4Brush,
                                    TextTrimming = TextTrimming.CharacterEllipsis
                                }
                            }
                        }, 1),
                        SetColumn(new TextBlock
                        {
                            Text = $"${po.Amount:N0}",
                            FontFamily = new FontFamily("Cascadia Code, Cascadia Mono, Consolas, monospace"),
                            FontSize = 9, FontWeight = Microsoft.UI.Text.FontWeights.Medium,
                            CharacterSpacing = 60, Foreground = accentColor,
                            VerticalAlignment = VerticalAlignment.Center
                        }, 2)
                    }
                }
            };

            AttentionPanel.Children.Add(card);
        }
    }

    private static T SetColumn<T>(T element, int column) where T : FrameworkElement
    {
        Grid.SetColumn(element, column);
        return element;
    }
}
