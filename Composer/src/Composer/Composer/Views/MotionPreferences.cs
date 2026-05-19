namespace Composer.Views;

/// <summary>
/// Reads the OS-level reduced-motion preference once and caches it for the
/// session. Animation sites consult <see cref="AnimationsEnabled"/> before
/// running storyboards — when false, they short-circuit to the animation's
/// final state so users with vestibular sensitivity / motion-reduce
/// preferences don't see the spin / fade / cross-fade transitions.
///
/// Defaults to <c>true</c> if the underlying <c>Windows.UI.ViewManagement.UISettings</c>
/// API isn't available on the current target — better to play animations
/// than to break the visual flow because of a missing API.
/// </summary>
internal static class MotionPreferences
{
    private static bool? _animationsEnabled;

    public static bool AnimationsEnabled
    {
        get
        {
            if (_animationsEnabled.HasValue) return _animationsEnabled.Value;
            try
            {
                var settings = new Windows.UI.ViewManagement.UISettings();
                _animationsEnabled = settings.AnimationsEnabled;
            }
            catch
            {
                _animationsEnabled = true;
            }
            return _animationsEnabled.Value;
        }
    }
}
