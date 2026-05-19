using Microsoft.UI.Xaml.Media.Animation;
using YouTubeMs.Services.Motion;

namespace YouTubeMs.Presentation.Controls;

public sealed partial class VideoCardSkeleton : UserControl
{
    public VideoCardSkeleton()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Motion.ReducedMotion) return;
        // §5.1 Shimmer: linear gradient sweeps across at 1.5s, infinite
        var anim = new DoubleAnimation
        {
            From = -300,
            To = 600,
            Duration = new Duration(TimeSpan.FromMilliseconds(1500)),
            RepeatBehavior = RepeatBehavior.Forever,
        };
        Storyboard.SetTarget(anim, ThumbShimmerTranslate);
        Storyboard.SetTargetProperty(anim, "X");
        new Storyboard { Children = { anim } }.Begin();
    }
}
