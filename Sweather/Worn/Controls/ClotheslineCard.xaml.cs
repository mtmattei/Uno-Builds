using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;
using Worn.Models;

namespace Worn.Controls;

public sealed partial class ClotheslineCard : UserControl
{
    private Storyboard? _swaySb;
    private Storyboard? _pulseSb;
    private Storyboard? _hoverSb;

    private static readonly Dictionary<string, SolidColorBrush> _brushCache = new();

    public static readonly DependencyProperty MomentProperty =
        DependencyProperty.Register(
            nameof(Moment),
            typeof(HourlyMoment),
            typeof(ClotheslineCard),
            new PropertyMetadata(null, OnMomentChanged));

    public HourlyMoment? Moment
    {
        get => (HourlyMoment?)GetValue(MomentProperty);
        set => SetValue(MomentProperty, value);
    }

    public ClotheslineCard()
    {
        this.InitializeComponent();
    }

    private static void OnMomentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ClotheslineCard card && e.NewValue is HourlyMoment moment)
        {
            card.ApplyMoment(moment);
        }
    }

    private void ApplyMoment(HourlyMoment m)
    {
        // Sway rotation
        SwayRotation.Angle = m.SwayAngle;

        // Transition annotation
        if (m.HasTransition && !string.IsNullOrEmpty(m.TransitionLabel))
        {
            TransitionText.Text = m.TransitionLabel;
            TransitionText.Visibility = Visibility.Visible;
        }
        else
        {
            TransitionText.Visibility = Visibility.Collapsed;
        }

        // Bubble background color (swatch at 20% opacity)
        BubbleBackground.Fill = GetCachedBrush(m.SwatchColor);

        // Emoji
        EmojiText.Text = m.Emoji;

        // Time
        TimeText.Text = m.DisplayTime;

        // Past dimming
        if (m.IsPast && !m.IsNow)
        {
            CardRoot.Opacity = 0.4;
        }
        else
        {
            CardRoot.Opacity = 1.0;
        }

        // Current-hour treatments
        if (m.IsNow)
        {
            CardScale.ScaleX = 1.08;
            CardScale.ScaleY = 1.08;
            AccentRing.Visibility = Visibility.Visible;
            PulsingDot.Visibility = Visibility.Visible;
            TierLabel.Text = m.TierId.ToString();
            TierLabel.Visibility = Visibility.Visible;
            TimeText.FontWeight = Microsoft.UI.Text.FontWeights.Bold;

            StartPulseAnimation();
            AutoScrollToCenter();
        }
        else
        {
            _pulseSb?.Stop();
            _pulseSb = null;

            CardScale.ScaleX = 1.0;
            CardScale.ScaleY = 1.0;
            AccentRing.Visibility = Visibility.Collapsed;
            PulsingDot.Visibility = Visibility.Collapsed;
            TierLabel.Visibility = Visibility.Collapsed;
            TimeText.FontWeight = Microsoft.UI.Text.FontWeights.Normal;
        }
    }

    private void StartPulseAnimation()
    {
        _pulseSb?.Stop();

        var pulseDuration = TimeSpan.FromMilliseconds(1600);
        var halfDuration = TimeSpan.FromMilliseconds(800);

        var anim = new DoubleAnimationUsingKeyFrames
        {
            RepeatBehavior = RepeatBehavior.Forever
        };
        anim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = 1.0 });
        anim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(halfDuration), Value = 0.2 });
        anim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(pulseDuration), Value = 1.0 });

        _pulseSb = new Storyboard();
        Storyboard.SetTarget(anim, PulsingDot);
        Storyboard.SetTargetProperty(anim, "Opacity");
        _pulseSb.Children.Add(anim);
        _pulseSb.Begin();
    }

    private void AutoScrollToCenter()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            this.StartBringIntoView(new BringIntoViewOptions
            {
                HorizontalAlignmentRatio = 0.5,
                AnimationDesired = true
            });
        });
    }

    /// <summary>
    /// Applies a dampened pendulum sway that winds down over ~1.2s.
    /// </summary>
    public void ApplyScrollSway(double swayDelta)
    {
        var rest = Moment?.SwayAngle ?? 0;
        var peak = Math.Clamp(rest + swayDelta, -20.0, 20.0);
        var offset = peak - rest;

        // Set base angle to rest BEFORE stopping so Stop() reverts to rest, not a stale value
        SwayRotation.Angle = rest;
        _swaySb?.Stop();
        _swaySb = null;

        // Dampened oscillation: peak -> overshoot back -> smaller forward -> settle
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = peak });
        anim.KeyFrames.Add(new SplineDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300)), Value = rest - offset * 0.45,
          KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.4, 0), ControlPoint2 = new Windows.Foundation.Point(0.6, 1) } });
        anim.KeyFrames.Add(new SplineDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(600)), Value = rest + offset * 0.2,
          KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.4, 0), ControlPoint2 = new Windows.Foundation.Point(0.6, 1) } });
        anim.KeyFrames.Add(new SplineDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(850)), Value = rest - offset * 0.08,
          KeySpline = new KeySpline { ControlPoint1 = new Windows.Foundation.Point(0.4, 0), ControlPoint2 = new Windows.Foundation.Point(0.6, 1) } });
        anim.KeyFrames.Add(new LinearDoubleKeyFrame
        { KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1100)), Value = rest });

        var sb = new Storyboard();
        _swaySb = sb;
        Storyboard.SetTarget(anim, SwayRotation);
        Storyboard.SetTargetProperty(anim, "Angle");
        sb.Children.Add(anim);

        sb.Completed += (_, _) =>
        {
            // Lock base angle to rest so future Stop() calls won't leave it dangling
            SwayRotation.Angle = rest;
            sb.Stop();
            if (_swaySb == sb) _swaySb = null;
        };

        sb.Begin();
    }

    private void OnCardPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverLift(toY: -4);
    }

    private void OnCardPointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimateHoverLift(toY: 0);
    }

    private void AnimateHoverLift(double toY)
    {
        _hoverSb?.Stop();

        var anim = new DoubleAnimation
        {
            To = toY,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        _hoverSb = new Storyboard();
        Storyboard.SetTarget(anim, HoverLift);
        Storyboard.SetTargetProperty(anim, "Y");
        _hoverSb.Children.Add(anim);

        _hoverSb.Completed += (_, _) => _hoverSb?.Stop();
        _hoverSb.Begin();
    }

    private static SolidColorBrush GetCachedBrush(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length < 6)
            return new SolidColorBrush(Colors.Gray);

        if (_brushCache.TryGetValue(hex, out var cached))
            return cached;

        var clean = hex.TrimStart('#');
        if (clean.Length == 6)
        {
            var r = Convert.ToByte(clean[..2], 16);
            var g = Convert.ToByte(clean[2..4], 16);
            var b = Convert.ToByte(clean[4..6], 16);
            var brush = new SolidColorBrush(Color.FromArgb(255, r, g, b));
            _brushCache[hex] = brush;
            return brush;
        }

        return new SolidColorBrush(Colors.Gray);
    }
}
