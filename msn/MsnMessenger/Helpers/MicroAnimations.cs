using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace MsnMessenger.Helpers;

public static class MicroAnimations
{
    /// <summary>
    /// Animate scale up on hover
    /// </summary>
    public static void AnimateHoverIn(UIElement element, double scale = 1.02)
    {
        var transform = GetOrCreateScaleTransform(element);
        AnimateScale(transform, scale, TimeSpan.FromMilliseconds(150));
    }

    /// <summary>
    /// Animate scale back to normal
    /// </summary>
    public static void AnimateHoverOut(UIElement element)
    {
        var transform = GetOrCreateScaleTransform(element);
        AnimateScale(transform, 1.0, TimeSpan.FromMilliseconds(150));
    }

    /// <summary>
    /// Animate press down effect
    /// </summary>
    public static async Task AnimatePress(UIElement element)
    {
        var transform = GetOrCreateScaleTransform(element);
        AnimateScale(transform, 0.95, TimeSpan.FromMilliseconds(80));
        await Task.Delay(80);
        AnimateScale(transform, 1.0, TimeSpan.FromMilliseconds(120));
    }

    /// <summary>
    /// Animate entrance with fade and slide up
    /// </summary>
    public static void AnimateEntrance(UIElement element, int delayMs = 0)
    {
        element.Opacity = 0;
        var translateTransform = new TranslateTransform { Y = 20 };
        element.RenderTransform = translateTransform;

        var fadeAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var slideAnim = new DoubleAnimation
        {
            From = 20,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeAnim);
        storyboard.Children.Add(slideAnim);

        Storyboard.SetTarget(fadeAnim, element);
        Storyboard.SetTargetProperty(fadeAnim, "Opacity");
        Storyboard.SetTarget(slideAnim, translateTransform);
        Storyboard.SetTargetProperty(slideAnim, "Y");

        storyboard.Begin();
    }

    /// <summary>
    /// Animate a subtle pulse effect
    /// </summary>
    public static void AnimatePulse(UIElement element, double maxScale = 1.1)
    {
        var transform = GetOrCreateScaleTransform(element);
        AnimatePulseLoop(transform, maxScale);
    }

    /// <summary>
    /// Animate a breathing/glow effect on opacity
    /// </summary>
    public static void AnimateBreathing(UIElement element, double minOpacity = 0.7, double maxOpacity = 1.0)
    {
        AnimateBreathingLoop(element, minOpacity, maxOpacity);
    }

    /// <summary>
    /// Shake animation for nudge effect
    /// </summary>
    public static async Task AnimateShake(UIElement element)
    {
        var translateTransform = new TranslateTransform();
        element.RenderTransform = translateTransform;

        var offsets = new[] { 0, -12, 12, -12, 12, -8, 8, -4, 4, 0 };

        foreach (var offset in offsets)
        {
            translateTransform.X = offset;
            await Task.Delay(50);
        }

        translateTransform.X = 0;
    }

    /// <summary>
    /// Pop in animation
    /// </summary>
    public static void AnimatePopIn(UIElement element, int delayMs = 0)
    {
        var transform = GetOrCreateScaleTransform(element);
        element.Opacity = 0;
        transform.ScaleX = 0.5;
        transform.ScaleY = 0.5;

        var fadeAnim = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        var scaleXAnim = new DoubleAnimation
        {
            From = 0.5,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        var scaleYAnim = new DoubleAnimation
        {
            From = 0.5,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeAnim);
        storyboard.Children.Add(scaleXAnim);
        storyboard.Children.Add(scaleYAnim);

        Storyboard.SetTarget(fadeAnim, element);
        Storyboard.SetTargetProperty(fadeAnim, "Opacity");
        Storyboard.SetTarget(scaleXAnim, transform);
        Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");
        Storyboard.SetTarget(scaleYAnim, transform);
        Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");

        storyboard.Begin();
    }

    private static ScaleTransform GetOrCreateScaleTransform(UIElement element)
    {
        if (element.RenderTransform is ScaleTransform existing)
            return existing;

        var transform = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
        element.RenderTransform = transform;
        element.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        return transform;
    }

    private static void AnimateScale(ScaleTransform transform, double targetScale, TimeSpan duration)
    {
        var scaleXAnim = new DoubleAnimation
        {
            To = targetScale,
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleYAnim = new DoubleAnimation
        {
            To = targetScale,
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(scaleXAnim);
        storyboard.Children.Add(scaleYAnim);

        Storyboard.SetTarget(scaleXAnim, transform);
        Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");
        Storyboard.SetTarget(scaleYAnim, transform);
        Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");

        storyboard.Begin();
    }

    private static void AnimatePulseLoop(ScaleTransform transform, double maxScale)
    {
        var scaleUpX = new DoubleAnimation
        {
            To = maxScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(800)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleUpY = new DoubleAnimation
        {
            To = maxScale,
            Duration = new Duration(TimeSpan.FromMilliseconds(800)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(scaleUpX);
        storyboard.Children.Add(scaleUpY);

        Storyboard.SetTarget(scaleUpX, transform);
        Storyboard.SetTargetProperty(scaleUpX, "ScaleX");
        Storyboard.SetTarget(scaleUpY, transform);
        Storyboard.SetTargetProperty(scaleUpY, "ScaleY");

        storyboard.Completed += (s, e) =>
        {
            var nextScale = transform.ScaleX >= maxScale ? 1.0 : maxScale;
            AnimatePulseLoop(transform, nextScale == 1.0 ? maxScale : nextScale);
        };

        storyboard.Begin();
    }

    private static void AnimateBreathingLoop(UIElement element, double minOpacity, double maxOpacity)
    {
        var currentOpacity = element.Opacity;
        var targetOpacity = currentOpacity <= minOpacity ? maxOpacity : minOpacity;

        var opacityAnim = new DoubleAnimation
        {
            To = targetOpacity,
            Duration = new Duration(TimeSpan.FromMilliseconds(1500)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(opacityAnim);

        Storyboard.SetTarget(opacityAnim, element);
        Storyboard.SetTargetProperty(opacityAnim, "Opacity");

        storyboard.Completed += (s, e) =>
        {
            AnimateBreathingLoop(element, minOpacity, maxOpacity);
        };

        storyboard.Begin();
    }
}
