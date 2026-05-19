using System.Windows.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;
using YouTubeMs.Services.VideoPlayer;

namespace YouTubeMs.Presentation.Controls;

public sealed partial class VideoCard : UserControl
{
    public static readonly DependencyProperty VideoProperty = DependencyProperty.Register(
        nameof(Video), typeof(Video), typeof(VideoCard),
        new PropertyMetadata(null, OnVideoChanged));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(VideoCard),
        new PropertyMetadata(null));

    public Video? Video
    {
        get => (Video?)GetValue(VideoProperty);
        set => SetValue(VideoProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public Visibility VerifiedVisibility =>
        Video?.Channel.IsVerified == true ? Visibility.Visible : Visibility.Collapsed;

    public VideoCard()
    {
        this.InitializeComponent();
        ClickHost.Click += OnClick;
        this.Loaded += (_, _) => AnimateVerifiedTick();
    }

    private void AnimateVerifiedTick()
    {
        if (VerifiedIcon.Visibility != Visibility.Visible) return;
        if (Services.Motion.Motion.ReducedMotion)
        {
            VerifiedIconScale.ScaleX = 1;
            VerifiedIconScale.ScaleY = 1;
            return;
        }
        var spring = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.6 };
        var sb = new Storyboard();
        foreach (var prop in new[] { "ScaleX", "ScaleY" })
        {
            var anim = new DoubleAnimation
            {
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(350)),
                EasingFunction = spring,
            };
            Storyboard.SetTarget(anim, VerifiedIconScale);
            Storyboard.SetTargetProperty(anim, prop);
            sb.Children.Add(anim);
        }
        sb.Begin();
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (Video is { } v)
            VideoPlayer.Open(v);
    }

    private static void OnVideoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VideoCard card && e.NewValue is Video v)
        {
            card.ApplyChannelTheme(v.Channel.AccentTop, v.Channel.AccentBottom);
        }
    }

    private void ApplyChannelTheme(Color top, Color bottom)
    {
        ThumbFill.Background = new LinearGradientBrush
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
