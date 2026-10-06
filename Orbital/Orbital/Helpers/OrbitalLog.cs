using System.Diagnostics;

namespace Orbital.Helpers;

/// <summary>
/// Lightweight diagnostic logger for boundary failures. Routes to Debug output and the .NET
/// trace listeners so swallowed exceptions surface in the IDE and during diagnostic builds
/// without crashing the app. Use at file/process/network boundaries that genuinely tolerate failure.
/// </summary>
internal static class OrbitalLog
{
    public static void Warn(Exception ex, string context)
    {
        Debug.WriteLine($"[Orbital] {context}: {ex.GetType().Name}: {ex.Message}");
    }

    public static void Warn(string context, string detail)
    {
        Debug.WriteLine($"[Orbital] {context}: {detail}");
    }
}
