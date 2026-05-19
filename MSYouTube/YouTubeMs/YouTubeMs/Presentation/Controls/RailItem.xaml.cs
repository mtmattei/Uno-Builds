using System.Windows.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using YouTubeMs.Services.VideoPlayer;

namespace YouTubeMs.Presentation.Controls;

public sealed partial class RailItem : UserControl
{
    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
        nameof(Item), typeof(RailVideo), typeof(RailItem),
        new PropertyMetadata(null, OnItemChanged));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(RailItem),
        new PropertyMetadata(null));

    public RailVideo? Item
    {
        get => (RailVideo?)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public RailItem()
    {
        this.InitializeComponent();
        RailButton.Click += OnClick;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (Item is { } v)
            VideoPlayer.Open(v);
    }

    private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RailItem item && e.NewValue is RailVideo r)
        {
            item.ApplyChannelTheme(r.Channel.AccentTop, r.Channel.AccentBottom);
        }
    }

    private void ApplyChannelTheme(Color top, Color bottom)
    {
        Thumb.Background = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(1, 1),
            GradientStops =
            {
                new GradientStop { Color = top, Offset = 0.0 },
                new GradientStop { Color = bottom, Offset = 1.0 },
            },
        };
    }
}
