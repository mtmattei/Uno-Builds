namespace Orbital.Helpers;

/// <summary>
/// Shared utilities for accessing the DI host and finding project paths.
/// </summary>
public static class HostHelper
{
    /// <summary>
    /// Polls for the App.Host to be assigned (up to 2 seconds).
    /// HomePage loads during NavigateAsync before Host is set.
    /// </summary>
    public static async ValueTask<IHost?> WaitForHostAsync()
    {
        var app = (App)Application.Current;
        for (var i = 0; i < 20 && app.Host is null; i++)
            await Task.Delay(100);
        return app.Host;
    }

    /// <summary>
    /// Gets the Host, returning null if not yet assigned (for pages loaded after navigation).
    /// </summary>
    public static IHost? GetHost() => ((App)Application.Current).Host;

    /// <summary>
    /// Walks up from AppContext.BaseDirectory to find the nearest .csproj or .sln.
    /// </summary>
    public static string? FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.csproj").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>
    /// Finds the nearest .csproj file from a root directory (walking up).
    /// </summary>
    public static string? FindCsproj(string? rootDir = null)
    {
        var dir = new DirectoryInfo(rootDir ?? AppContext.BaseDirectory);
        while (dir is not null)
        {
            var files = dir.GetFiles("*.csproj");
            if (files.Length > 0)
                return files[0].FullName;
            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>
    /// Reads TargetFrameworks from a .csproj file, returns shortened display names.
    /// </summary>
    public static (string[] ShortNames, string[] FullTfms) ReadTargetFrameworks(string csprojPath)
    {
        try
        {
            var content = File.ReadAllText(csprojPath);
            var match = System.Text.RegularExpressions.Regex.Match(
                content, @"<TargetFrameworks?>(.*?)</TargetFrameworks?>", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (match.Success)
            {
                var raw = match.Groups[1].Value.Replace("\n", "").Replace("\r", "").Trim();
                var tfms = raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var shortNames = tfms.Select(t =>
                    t.Replace("net10.0-", "").Replace("net9.0-", "").Replace("net8.0-", ""))
                    .ToArray();
                return (shortNames, tfms);
            }
        }
        catch { }
        return (["desktop"], ["net10.0-desktop"]);
    }
}
