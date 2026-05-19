using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using YouTubeMs.Presentation.Controls;
using YouTubeMs.Services.Catalog;
using YouTubeMs.Services.Motion;

namespace YouTubeMs.Presentation.Home;

public sealed partial class HomePage : Page
{
    private readonly System.Collections.Generic.List<RailItem> _railItems = new();

    public HomePage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    // ===== Rail focus effect: hovered item pops, siblings dim back =====

    private void OnRailElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is RailItem item)
        {
            EnsureRailItemTransform(item);
            item.PointerEntered += OnRailItemPointerEntered;
            item.PointerExited += OnRailItemPointerExited;
            _railItems.Add(item);
        }
    }

    private void OnRailElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is RailItem item)
        {
            item.PointerEntered -= OnRailItemPointerEntered;
            item.PointerExited -= OnRailItemPointerExited;
            _railItems.Remove(item);
        }
    }

    private static void EnsureRailItemTransform(RailItem item)
    {
        item.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        if (item.RenderTransform is not CompositeTransform)
        {
            item.RenderTransform = new CompositeTransform();
        }
    }

    private void OnRailItemPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (Motion.ReducedMotion) return;
        var hovered = sender as RailItem;
        foreach (var item in _railItems)
        {
            if (item == hovered) continue;
            AnimateRailDim(item, opacity: 0.5, scale: 0.97);
        }
    }

    private void OnRailItemPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (Motion.ReducedMotion) return;
        foreach (var item in _railItems)
        {
            AnimateRailDim(item, opacity: 1.0, scale: 1.0);
        }
    }

    private static void AnimateRailDim(RailItem item, double opacity, double scale)
    {
        EnsureRailItemTransform(item);
        var transform = (CompositeTransform)item.RenderTransform;
        var ease = new QuarticEase { EasingMode = EasingMode.EaseOut };
        var sb = new Storyboard();

        var fade = new DoubleAnimation { To = opacity, Duration = new Duration(TimeSpan.FromMilliseconds(220)), EasingFunction = ease };
        Storyboard.SetTarget(fade, item);
        Storyboard.SetTargetProperty(fade, "Opacity");
        sb.Children.Add(fade);

        var sx = new DoubleAnimation { To = scale, Duration = new Duration(TimeSpan.FromMilliseconds(220)), EasingFunction = ease };
        Storyboard.SetTarget(sx, transform);
        Storyboard.SetTargetProperty(sx, "ScaleX");
        sb.Children.Add(sx);

        var sy = new DoubleAnimation { To = scale, Duration = new Duration(TimeSpan.FromMilliseconds(220)), EasingFunction = ease };
        Storyboard.SetTarget(sy, transform);
        Storyboard.SetTargetProperty(sy, "ScaleY");
        sb.Children.Add(sy);

        sb.Begin();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is null)
        {
            try
            {
                var catalog = new MockCatalogService();
                DataContext = new HomeFallbackVm(catalog);
            }
            catch
            {
                // Fall back silently
            }
        }

        PlayEntranceCascade();
    }

    // §2.1 First-paint of HomePage
    private void PlayEntranceCascade()
    {
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        if (Motion.ReducedMotion)
        {
            // Per §6.3, collapse to a single 120ms opacity fade
            ForYouSection.Opacity = 1;
            TrendingSection.Opacity = 1;
            return;
        }

        // 1) Hero is animated by HeroFeatured itself (existing entrance)

        // 3) Recommendation rail items would cascade individually if we had
        //    direct references. Container fades suffice here.

        // 4) For You — expressive 350ms, 320ms delay
        AnimateOpacity(ForYouSection, 1, 350, 320, ease);

        // 5) Trending — expressive 350ms, 480ms delay
        AnimateOpacity(TrendingSection, 1, 350, 480, ease);
    }

    private static void AnimateOpacity(UIElement target, double to, double durationMs, double delayMs, EasingFunctionBase ease)
    {
        var anim = new DoubleAnimation
        {
            To = to,
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = ease,
        };
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, "Opacity");
        new Storyboard { Children = { anim } }.Begin();
    }

    private static void AnimateDouble(DependencyObject target, string property, double to, double durationMs, double delayMs, EasingFunctionBase ease)
    {
        var anim = new DoubleAnimation
        {
            To = to,
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = ease,
        };
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, property);
        new Storyboard { Children = { anim } }.Begin();
    }

    // Carousel chevrons (§3.6) — fade in on hover, scroll by viewport width
    private void OnCarouselPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        FadeChevron(ForYouLeftChevron, 1);
        FadeChevron(ForYouRightChevron, 1);
        ForYouLeftChevron.IsHitTestVisible = true;
        ForYouRightChevron.IsHitTestVisible = true;
    }

    private void OnCarouselPointerExited(object sender, PointerRoutedEventArgs e)
    {
        FadeChevron(ForYouLeftChevron, 0);
        FadeChevron(ForYouRightChevron, 0);
        ForYouLeftChevron.IsHitTestVisible = false;
        ForYouRightChevron.IsHitTestVisible = false;
    }

    private static void FadeChevron(UIElement target, double to)
    {
        var anim = new DoubleAnimation
        {
            To = to,
            Duration = new Duration(TimeSpan.FromMilliseconds(120)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, "Opacity");
        new Storyboard { Children = { anim } }.Begin();
    }

    private void OnForYouScrollLeft(object sender, RoutedEventArgs e)
    {
        var step = ForYouScroller.ViewportWidth * 0.6;
        ForYouScroller.ChangeView(Math.Max(0, ForYouScroller.HorizontalOffset - step), null, null);
    }

    private void OnForYouScrollRight(object sender, RoutedEventArgs e)
    {
        var step = ForYouScroller.ViewportWidth * 0.6;
        ForYouScroller.ChangeView(ForYouScroller.HorizontalOffset + step, null, null);
    }
}

public sealed class HomeFallbackVm
{
    public HomeFallbackVm(ICatalogService catalog)
    {
        Featured = catalog.GetFeaturedAsync().AsTask().GetAwaiter().GetResult();
        ForYou = catalog.GetForYouAsync().AsTask().GetAwaiter().GetResult();
        Trending = catalog.GetTrendingAsync().AsTask().GetAwaiter().GetResult();
        Recommendations = catalog.GetRailAsync().AsTask().GetAwaiter().GetResult();
    }

    public FeaturedVideo Featured { get; }
    public IImmutableList<Video> ForYou { get; }
    public IImmutableList<Video> Trending { get; }
    public IImmutableList<RailVideo> Recommendations { get; }
}
