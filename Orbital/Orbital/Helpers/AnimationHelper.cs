using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Orbital.Helpers;

/// <summary>
/// Shared animation utilities for Orbital page entrance choreography and persistent effects.
/// </summary>
public static class AnimationHelper
{
    /// <summary>
    /// Fade-up entrance: opacity 0→1 + translateY 8→0, 350ms CubicEaseOut, with stagger delay.
    /// The element must have a TranslateTransform set as RenderTransform, OR this will create one.
    /// </summary>
    public static void FadeUp(UIElement element, int delayMs)
    {
        element.Opacity = 0;

        var translate = element.RenderTransform as TranslateTransform;
        if (translate == null)
        {
            translate = new TranslateTransform();
            element.RenderTransform = translate;
        }
        translate.Y = 8;

        var sb = new Storyboard();

        var opacityAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(350)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(opacityAnim, element);
        Storyboard.SetTargetProperty(opacityAnim, "Opacity");

        var translateAnim = new DoubleAnimation
        {
            From = 8,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(350)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(translateAnim, translate);
        Storyboard.SetTargetProperty(translateAnim, "Y");

        sb.Children.Add(opacityAnim);
        sb.Children.Add(translateAnim);
        sb.Begin();
    }

    /// <summary>
    /// Timer-driven border breathe. Uno Skia cannot animate brush properties via Storyboard,
    /// so we manually interpolate the border color alpha on a timer.
    /// Returns a Storyboard (empty) for API compat — call .Stop() on it to stop the timer.
    /// </summary>
    public static Storyboard StartBorderBreathe(Border border)
    {
        const byte r = 0x2D, g = 0x2D, b = 0x30;
        const byte minAlpha = 0x1A; // ~10%
        const byte maxAlpha = 0x60; // ~38%
        const double cycleDuration = 3.0; // seconds per full cycle
        const int fps = 20; // 20fps is sufficient for a slow breathe

        // Reuse a single brush — mutate Color instead of allocating a new brush each tick
        var brush = new SolidColorBrush(Windows.UI.Color.FromArgb(minAlpha, r, g, b));
        border.BorderBrush = brush;

        var startTime = DateTime.UtcNow;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / fps) };
        timer.Tick += (_, _) =>
        {
            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            var t = (Math.Sin(elapsed * 2 * Math.PI / cycleDuration - Math.PI / 2) + 1) / 2;
            var alpha = (byte)(minAlpha + (maxAlpha - minAlpha) * t);
            brush.Color = Windows.UI.Color.FromArgb(alpha, r, g, b);
        };
        timer.Start();

        var sb = new Storyboard();
        sb.Completed += (_, _) => timer.Stop();
        border.Tag = timer;
        return sb;
    }

    public static void StopBorderBreathe(Border border)
    {
        if (border.Tag is DispatcherTimer timer)
        {
            timer.Stop();
            border.Tag = null;
        }
    }
}
