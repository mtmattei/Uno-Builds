using Microsoft.UI.Xaml.Media.Animation;

namespace UnoToolkit.Bench;

// Shared easing instances — sharing is safe because EasingFunctionBase has no
// per-animation state. Avoids `new CubicEase { ... }` on every storyboard build.
internal static class Easings
{
    public static readonly EasingFunctionBase MorphInOut = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 4 };
    public static readonly EasingFunctionBase BackOut    = new BackEase        { EasingMode = EasingMode.EaseOut,   Amplitude = 0.4 };
    public static readonly EasingFunctionBase Decel      = new CubicEase       { EasingMode = EasingMode.EaseOut };
    public static readonly EasingFunctionBase EaseIn     = new CubicEase       { EasingMode = EasingMode.EaseIn };
    public static readonly EasingFunctionBase EaseInOut  = new CubicEase       { EasingMode = EasingMode.EaseInOut };
    public static readonly EasingFunctionBase QuadIn     = new QuadraticEase   { EasingMode = EasingMode.EaseIn };
}
