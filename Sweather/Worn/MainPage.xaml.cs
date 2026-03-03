using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;
using Worn.Services;

namespace Worn;

public sealed partial class MainPage : Page
{
    private readonly IReverseGeocodingService _geocoding;
    private CancellationTokenSource? _suggestionCts;
    private bool _locationTagFlipped;
    private bool _outfitMemoExpanded = true;
    private Storyboard? _locationFlipStoryboard;
    private Storyboard? _outfitCollapseSb;
    private List<Controls.ClotheslineCard>? _clotheslineCards;

    private static readonly string[] _collapsedHints = new[]
    {
        "your outfit is patiently folded in here...",
        "shhh... your clothes are napping",
        "a whole look, tucked away neatly",
        "tap to peek inside the wardrobe",
        "styled and stacked, ready when you are",
        "your fit is in here, trust us",
        "folded with care, unfold to wear",
    };
    private static readonly Random _hintRng = new();

    private static readonly ConditionalWeakTable<TranslateTransform, StoryboardHolder> _liftStoryboards = new();

    public MainPage()
    {
        this.InitializeComponent();

        var services = ((App)Application.Current).Host!.Services;
        _geocoding = services.GetRequiredService<IReverseGeocodingService>();
        DataContext = new MainViewModel(
            services.GetRequiredService<IGeoLocationService>(),
            services.GetRequiredService<IWeatherService>(),
            _geocoding,
            services.GetRequiredService<IWeatherMappingEngine>()
        );
    }

    // ── Location tag flip ──────────────────────────────

    private async void OnLocationTagTapped(object sender, TappedRoutedEventArgs e)
    {
        if (_locationTagFlipped) return;
        _locationTagFlipped = true;

        // Prepare back face for animation
        LocationTagBack.Opacity = 0;
        LocationTagBack.IsHitTestVisible = false;
        LocationBackScale.ScaleX = 0;
        LocationTagBack.Visibility = Visibility.Visible;

        AnimateLocationTagFlip(toBack: true);

        await Task.Delay(180);
        LocationSearchBox.Focus(FocusState.Programmatic);
    }

    private void OnLocationTagClose(object sender, TappedRoutedEventArgs e)
    {
        if (!_locationTagFlipped) return;
        _locationTagFlipped = false;
        LocationSearchBox.Text = "";
        SuggestionsList.Visibility = Visibility.Collapsed;
        SuggestionsList.ItemsSource = null;
        AnimateLocationTagFlip(toBack: false);
    }

    private void AnimateLocationTagFlip(bool toBack)
    {
        _locationFlipStoryboard?.Stop();

        var halfDuration = TimeSpan.FromMilliseconds(150);
        var fullDuration = TimeSpan.FromMilliseconds(300);
        var storyboard = new Storyboard();
        _locationFlipStoryboard = storyboard;

        // Shrink outgoing face
        var shrinkAnim = new DoubleAnimationUsingKeyFrames();
        shrinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1 });
        shrinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 0 });
        shrinkAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(fullDuration), Value = 0 });
        Storyboard.SetTarget(shrinkAnim, toBack ? LocationFrontScale : LocationBackScale);
        Storyboard.SetTargetProperty(shrinkAnim, "ScaleX");
        storyboard.Children.Add(shrinkAnim);

        // Grow incoming face
        var growAnim = new DoubleAnimationUsingKeyFrames();
        growAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 });
        growAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 0 });
        growAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(fullDuration), Value = 1 });
        Storyboard.SetTarget(growAnim, toBack ? LocationBackScale : LocationFrontScale);
        Storyboard.SetTargetProperty(growAnim, "ScaleX");
        storyboard.Children.Add(growAnim);

        // Discrete opacity swap at midpoint
        var hideAnim = new DoubleAnimationUsingKeyFrames();
        hideAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1 });
        hideAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 0 });
        Storyboard.SetTarget(hideAnim, toBack ? LocationPillFront : LocationTagBack);
        Storyboard.SetTargetProperty(hideAnim, "Opacity");
        storyboard.Children.Add(hideAnim);

        var showAnim = new DoubleAnimationUsingKeyFrames();
        showAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 0 });
        showAnim.KeyFrames.Add(new DiscreteDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 1 });
        Storyboard.SetTarget(showAnim, toBack ? LocationTagBack : LocationPillFront);
        Storyboard.SetTargetProperty(showAnim, "Opacity");
        storyboard.Children.Add(showAnim);

        LocationPillFront.IsHitTestVisible = !toBack;
        LocationTagBack.IsHitTestVisible = toBack;

        if (!toBack)
        {
            storyboard.Completed += (_, _) =>
            {
                storyboard.Stop();
                LocationTagBack.Visibility = Visibility.Collapsed;
            };
        }

        storyboard.Begin();
    }

    // ── Location search ──────────────────────────────────

    private void OnLocationSearchKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && sender is TextBox tb)
        {
            var city = tb.Text.Trim();
            if (!string.IsNullOrEmpty(city) && DataContext is MainViewModel vm)
            {
                vm.CityOverride = city;
            }

            SuggestionsList.Visibility = Visibility.Collapsed;
            _locationTagFlipped = false;
            AnimateLocationTagFlip(toBack: false);
            e.Handled = true;
        }
    }

    private void OnLocationSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var query = tb.Text.Trim();

        _suggestionCts?.Cancel();
        _suggestionCts?.Dispose();

        if (query.Length < 2)
        {
            SuggestionsList.Visibility = Visibility.Collapsed;
            SuggestionsList.ItemsSource = null;
            return;
        }

        _suggestionCts = new CancellationTokenSource();
        _ = SearchSuggestionsAsync(query, _suggestionCts.Token);
    }

    private async Task SearchSuggestionsAsync(string query, CancellationToken ct)
    {
        await Task.Delay(150, ct);
        if (ct.IsCancellationRequested) return;

        var suggestions = await _geocoding.SearchCitySuggestionsAsync(query, ct);
        if (ct.IsCancellationRequested) return;

        DispatcherQueue.TryEnqueue(() =>
        {
            if (suggestions.Count > 0)
            {
                SuggestionsList.ItemsSource = suggestions;
                SuggestionsList.Visibility = Visibility.Visible;
            }
            else
            {
                SuggestionsList.Visibility = Visibility.Collapsed;
            }
        });
    }

    private void OnSuggestionTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is CitySuggestion suggestion)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.CityOverride = suggestion.DisplayName;
            }

            LocationSearchBox.Text = suggestion.DisplayName;
            SuggestionsList.Visibility = Visibility.Collapsed;

            AnimateStampBounce(fe);
            _ = FlipBackAfterDelay();
        }
    }

    private async Task FlipBackAfterDelay()
    {
        await Task.Delay(250);
        _locationTagFlipped = false;
        AnimateLocationTagFlip(toBack: false);
    }

    private void OnResetLocationTapped(object sender, TappedRoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.CityOverride = "";
        }

        LocationSearchBox.Text = "";
        SuggestionsList.Visibility = Visibility.Collapsed;
        _locationTagFlipped = false;
        AnimateLocationTagFlip(toBack: false);
    }

    private static void AnimateStampBounce(FrameworkElement target)
    {
        // Reuse existing ScaleTransform if present, otherwise create one
        if (target.RenderTransform is not ScaleTransform scale)
        {
            scale = new ScaleTransform();
            target.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            target.RenderTransform = scale;
        }

        var sb = new Storyboard();
        var mid = TimeSpan.FromMilliseconds(100);
        var end = TimeSpan.FromMilliseconds(200);

        var xAnim = new DoubleAnimationUsingKeyFrames();
        xAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1.0 });
        xAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(mid), Value = 1.08 });
        xAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(end), Value = 1.0 });
        Storyboard.SetTarget(xAnim, scale);
        Storyboard.SetTargetProperty(xAnim, "ScaleX");
        sb.Children.Add(xAnim);

        var yAnim = new DoubleAnimationUsingKeyFrames();
        yAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1.0 });
        yAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(mid), Value = 1.08 });
        yAnim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(end), Value = 1.0 });
        Storyboard.SetTarget(yAnim, scale);
        Storyboard.SetTargetProperty(yAnim, "ScaleY");
        sb.Children.Add(yAnim);

        sb.Begin();
    }

    // ── Outfit memo collapse ─────────────────────────────

    private void OnOutfitMemoHeaderTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not Border header) return;
        if (VisualTreeHelper.GetParent(header) is not Panel parent) return;

        _outfitMemoExpanded = !_outfitMemoExpanded;

        // Find siblings: container Border (with ScaleTransform) and hint TextBlock
        Border? container = null;
        TextBlock? hint = null;
        bool pastHeader = false;

        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child == header) { pastHeader = true; continue; }
            if (!pastHeader) continue;

            if (child is Border b && b.RenderTransform is ScaleTransform)
                container = b;
            else if (child is TextBlock tb)
                hint = tb;
        }

        var chevron = FindDescendant<FontIcon>(header);
        if (container == null || chevron == null) return;

        AnimateOutfitCollapse(container, chevron, hint, _outfitMemoExpanded);
    }

    private void AnimateOutfitCollapse(
        Border container, FontIcon chevron, TextBlock? hint, bool expand)
    {
        _outfitCollapseSb?.Stop();

        if (expand)
        {
            container.Visibility = Visibility.Visible;
        }
        else if (hint != null)
        {
            // Pick a random fun message when collapsing
            hint.Text = _collapsedHints[_hintRng.Next(_collapsedHints.Length)];
        }

        var sb = new Storyboard();
        _outfitCollapseSb = sb;
        var duration = TimeSpan.FromMilliseconds(300);
        var ease = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

        // Container ScaleY
        if (container.RenderTransform is ScaleTransform containerScale)
        {
            var scaleAnim = new DoubleAnimation
            {
                From = expand ? 0 : 1,
                To = expand ? 1 : 0,
                Duration = new Duration(duration),
                EasingFunction = ease
            };
            Storyboard.SetTarget(scaleAnim, containerScale);
            Storyboard.SetTargetProperty(scaleAnim, "ScaleY");
            sb.Children.Add(scaleAnim);
        }

        // Container Opacity
        var opacityAnim = new DoubleAnimation
        {
            From = expand ? 0 : 1,
            To = expand ? 1 : 0,
            Duration = new Duration(duration),
            EasingFunction = ease
        };
        Storyboard.SetTarget(opacityAnim, container);
        Storyboard.SetTargetProperty(opacityAnim, "Opacity");
        sb.Children.Add(opacityAnim);

        // Chevron rotation
        if (chevron.RenderTransform is RotateTransform chevronRotate)
        {
            var rotateAnim = new DoubleAnimation
            {
                From = expand ? 180 : 0,
                To = expand ? 0 : 180,
                Duration = new Duration(duration),
                EasingFunction = ease
            };
            Storyboard.SetTarget(rotateAnim, chevronRotate);
            Storyboard.SetTargetProperty(rotateAnim, "Angle");
            sb.Children.Add(rotateAnim);
        }

        // Hint opacity
        if (hint != null)
        {
            var hintAnim = new DoubleAnimation
            {
                From = expand ? 0.8 : 0,
                To = expand ? 0 : 0.8,
                Duration = new Duration(duration),
                EasingFunction = ease
            };
            Storyboard.SetTarget(hintAnim, hint);
            Storyboard.SetTargetProperty(hintAnim, "Opacity");
            sb.Children.Add(hintAnim);
        }

        if (!expand)
        {
            sb.Completed += (_, _) =>
            {
                // Persist final values before Stop() resets them
                container.Visibility = Visibility.Collapsed;
                if (hint != null) hint.Opacity = 0.8;
                if (chevron.RenderTransform is RotateTransform cr) cr.Angle = 180;
                sb.Stop();
            };
        }
        else
        {
            sb.Completed += (_, _) =>
            {
                if (hint != null) hint.Opacity = 0;
                if (chevron.RenderTransform is RotateTransform cr) cr.Angle = 0;
                sb.Stop();
            };
        }

        sb.Begin();
    }

    private static T? FindDescendant<T>(DependencyObject parent) where T : class
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T found) return found;
            var result = FindDescendant<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    // ── Horizontal scroll wheel redirect ──────────────

    private void OnHorizontalScrollWheel(object sender, PointerRoutedEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            var props = e.GetCurrentPoint(sv).Properties;
            var delta = props.MouseWheelDelta;
            if (delta != 0)
            {
                sv.ChangeView(sv.HorizontalOffset - delta, null, null, false);
                e.Handled = true;
            }
        }
    }

    // ── Clothesline scroll sway ────────────────────────

    private double _previousClotheslineOffset;

    private void OnClotheslineViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        if (sender is not ScrollViewer sv) return;

        var delta = sv.HorizontalOffset - _previousClotheslineOffset;
        _previousClotheslineOffset = sv.HorizontalOffset;

        if (Math.Abs(delta) < 1) return;

        var swayDelta = Math.Clamp(-delta * 0.6, -14.0, 14.0);

        // Build card cache on first scroll, invalidate when children change
        if (_clotheslineCards is null)
        {
            _clotheslineCards = new List<Controls.ClotheslineCard>();
            CollectCards(sv, _clotheslineCards);
        }

        foreach (var card in _clotheslineCards)
        {
            card.ApplyScrollSway(swayDelta);
        }
    }

    private static void CollectCards(DependencyObject parent, List<Controls.ClotheslineCard> cards)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is Controls.ClotheslineCard card)
                cards.Add(card);
            else
                CollectCards(child, cards);
        }
    }

    // ── Outfit item hover lift ─────────────────────────

    private void OnOutfitItemEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b && b.RenderTransform is TranslateTransform tt)
        {
            AnimateLift(tt, toY: -6);
            b.Background = (Brush)Application.Current.Resources["WornBlushBrush"];
            b.Opacity = 0.92;
        }
    }

    private void OnOutfitItemExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b && b.RenderTransform is TranslateTransform tt)
        {
            AnimateLift(tt, toY: 0);
            b.Background = null;
            b.Opacity = 1.0;
        }
    }

    // ── Card hover lift (hourly + daily) ─────────────

    private void OnCardHoverEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b && b.RenderTransform is TranslateTransform tt)
        {
            AnimateLift(tt, toY: -4);
        }
    }

    private void OnCardHoverExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border b && b.RenderTransform is TranslateTransform tt)
        {
            AnimateLift(tt, toY: 0);
        }
    }

    private static void AnimateLift(TranslateTransform target, double toY)
    {
        // Stop any existing lift animation on this transform
        if (_liftStoryboards.TryGetValue(target, out var holder))
        {
            holder.Storyboard?.Stop();
        }

        var anim = new DoubleAnimation
        {
            To = toY,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var sb = new Storyboard();
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, "Y");
        sb.Children.Add(anim);

        sb.Completed += (_, _) => sb.Stop();

        _liftStoryboards.AddOrUpdate(target, new StoryboardHolder { Storyboard = sb });
        sb.Begin();
    }

    private sealed class StoryboardHolder
    {
        public Storyboard? Storyboard { get; set; }
    }
}
