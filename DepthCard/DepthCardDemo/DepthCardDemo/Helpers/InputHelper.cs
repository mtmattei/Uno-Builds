using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;

namespace DepthCardDemo.Helpers;

/// <summary>
/// Helper class for detecting input device types and characteristics.
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Determines whether the pointer event originates from a touch device.
    /// </summary>
    /// <param name="e">The pointer event arguments to check.</param>
    /// <returns>True if the pointer is from a touch device; false otherwise.</returns>
    public static bool IsTouchDevice(PointerRoutedEventArgs e)
    {
        return e.Pointer.PointerDeviceType == PointerDeviceType.Touch;
    }

    /// <summary>
    /// Determines whether the pointer event originates from a pen/stylus device.
    /// </summary>
    /// <param name="e">The pointer event arguments to check.</param>
    /// <returns>True if the pointer is from a pen device; false otherwise.</returns>
    public static bool IsPenDevice(PointerRoutedEventArgs e)
    {
        return e.Pointer.PointerDeviceType == PointerDeviceType.Pen;
    }

    /// <summary>
    /// Determines whether the pointer event originates from a mouse device.
    /// </summary>
    /// <param name="e">The pointer event arguments to check.</param>
    /// <returns>True if the pointer is from a mouse device; false otherwise.</returns>
    public static bool IsMouseDevice(PointerRoutedEventArgs e)
    {
        return e.Pointer.PointerDeviceType == PointerDeviceType.Mouse;
    }

    /// <summary>
    /// Gets the appropriate intensity multiplier for the input device type.
    /// Touch and pen devices typically need reduced intensity for better UX.
    /// </summary>
    /// <param name="e">The pointer event arguments to check.</param>
    /// <returns>A multiplier value between 0 and 1.</returns>
    public static double GetIntensityMultiplier(PointerRoutedEventArgs e)
    {
        return e.Pointer.PointerDeviceType switch
        {
            PointerDeviceType.Touch => 0.7,  // Reduced intensity for touch
            PointerDeviceType.Pen => 0.8,    // Slightly reduced for pen
            PointerDeviceType.Mouse => 1.0,  // Full intensity for mouse
            _ => 1.0
        };
    }
}
