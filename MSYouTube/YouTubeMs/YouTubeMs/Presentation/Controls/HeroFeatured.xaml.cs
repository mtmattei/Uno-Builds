using System.Windows.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;
using YouTubeMs.Services.VideoPlayer;

namespace YouTubeMs.Presentation.Controls;

public sealed partial class HeroFeatured : UserControl
{
    public static readonly DependencyProperty FeaturedProperty = DependencyProperty.Register(
        nameof(Featured), typeof(FeaturedVideo), typeof(HeroFeatured),
        new PropertyMetadata(null, OnFeaturedChanged));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(HeroFeatured),
        new PropertyMetadata(null));

    public FeaturedVideo? Featured
    {
        get => (FeaturedVideo?)GetValue(FeaturedProperty);
        set => SetValue(FeaturedProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public HeroFeatured()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        PlayCta.Click += OnPlayClicked;
    }

    private void OnPlayClicked(object sender, RoutedEventArgs e)
    {
        if (Featured is { } f)
            VideoPlayer.Open(f);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Services.Motion.Motion.ReducedMotion)
        {
            HeroRootBorder.Opacity = 1;
            HeroEntranceTransform.ScaleX = 1;
            HeroEntranceTransform.ScaleY = 1;
            return;
        }

        var sb = new Storyboard();

        var fade = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(220)), EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }, BeginTime = TimeSpan.FromMilliseconds(40) };
        Storyboard.SetTarget(fade, HeroRootBorder);
        Storyboard.SetTargetProperty(fade, "Opacity");

        var scaleX = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(220)), EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }, BeginTime = TimeSpan.FromMilliseconds(40) };
        Storyboard.SetTarget(scaleX, HeroEntranceTransform);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");

        var scaleY = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(220)), EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }, BeginTime = TimeSpan.FromMilliseconds(40) };
        Storyboard.SetTarget(scaleY, HeroEntranceTransform);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");

        sb.Children.Add(fade);
        sb.Children.Add(scaleX);
        sb.Children.Add(scaleY);
        sb.Begin();
    }

    private static void OnFeaturedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HeroFeatured hero && e.NewValue is FeaturedVideo f)
        {
            hero.ApplyChannelTheme(f.Channel.AccentTop, f.Channel.AccentBottom);
        }
    }

    private void ApplyChannelTheme(Color top, Color bottom)
    {
        HeroFill.Background = new LinearGradientBrush
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
