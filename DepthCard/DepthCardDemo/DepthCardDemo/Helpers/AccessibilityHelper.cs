using Windows.UI.ViewManagement;

namespace DepthCardDemo.Helpers;

/// <summary>
/// Helper class for detecting accessibility settings and preferences.
/// </summary>
public static class AccessibilityHelper
{
    private static bool? _animationsEnabled;

    /// <summary>
    /// Determines whether animations are enabled in the system settings.
    /// Respects the user's "prefers-reduced-motion" preference.
    /// </summary>
    /// <returns>True if animations are enabled; false if the user prefers reduced motion.</returns>
    public static bool AreAnimationsEnabled()
    {
        if (_animationsEnabled.HasValue)
            return _animationsEnabled.Value;

        try
        {
            var uiSettings = new UISettings();
            _animationsEnabled = uiSettings.AnimationsEnabled;
            return _animationsEnabled.Value;
        }
        catch
        {
            // Fallback for platforms that don't support UISettings
            // Default to enabled to avoid breaking functionality
            _animationsEnabled = true;
            return true;
        }
    }

    /// <summary>
    /// Resets the cached animation preference, forcing a re-check on next call.
    /// Call this if you expect the user's system settings to have changed.
    /// </summary>
    public static void RefreshAnimationPreference()
    {
        _animationsEnabled = null;
    }
}
