using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace MsnMessenger.Helpers;

public static class MicroAnimations
{
    public static async Task AnimatePress(UIElement element)
    {
        var transform = GetOrCreateScaleTransform(element);
        AnimateScale(transform, 0.95, TimeSpan.FromMilliseconds(80));
        await Task.Delay(80);
        AnimateScale(transform, 1.0, TimeSpan.FromMilliseconds(120));
    }

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
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };

        var slideAnim = new DoubleAnimation
        {
            From = 20,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };

        Storyboard.SetTarget(fadeAnim, element);
        Storyboard.SetTargetProperty(fadeAnim, "Opacity");
        Storyboard.SetTarget(slideAnim, translateTransform);
        Storyboard.SetTargetProperty(slideAnim, "Y");

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeAnim);
        storyboard.Children.Add(slideAnim);
        storyboard.Begin();
    }

    public static Storyboard AnimatePulse(UIElement element, double maxScale = 1.1)
    {
        var transform = GetOrCreateScaleTransform(element);

        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
        storyboard.Children.Add(BuildPulseAxis(transform, "ScaleX", maxScale));
        storyboard.Children.Add(BuildPulseAxis(transform, "ScaleY", maxScale));
        storyboard.Begin();
        return storyboard;
    }

    private static DoubleAnimationUsingKeyFrames BuildPulseAxis(ScaleTransform transform, string property, double maxScale)
    {
        var ease = new SineEase { EasingMode = EasingMode.EaseInOut };
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 1.0, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(800), Value = maxScale, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(1600), Value = 1.0, EasingFunction = ease });
        Storyboard.SetTarget(anim, transform);
        Storyboard.SetTargetProperty(anim, property);
        return anim;
    }

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
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 },
        };

        var scaleXAnim = new DoubleAnimation
        {
            From = 0.5,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 },
        };

        var scaleYAnim = new DoubleAnimation
        {
            From = 0.5,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 },
        };

        Storyboard.SetTarget(fadeAnim, element);
        Storyboard.SetTargetProperty(fadeAnim, "Opacity");
        Storyboard.SetTarget(scaleXAnim, transform);
        Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");
        Storyboard.SetTarget(scaleYAnim, transform);
        Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(fadeAnim);
        storyboard.Children.Add(scaleXAnim);
        storyboard.Children.Add(scaleYAnim);
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
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(scaleXAnim, transform);
        Storyboard.SetTargetProperty(scaleXAnim, "ScaleX");

        var scaleYAnim = new DoubleAnimation
        {
            To = targetScale,
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(scaleYAnim, transform);
        Storyboard.SetTargetProperty(scaleYAnim, "ScaleY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(scaleXAnim);
        storyboard.Children.Add(scaleYAnim);
        storyboard.Begin();
    }
}
