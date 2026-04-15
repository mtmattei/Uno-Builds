using System.Reflection;

namespace Orbital.Services;

public class RuntimeStudioService : IStudioService
{
    public ValueTask<StudioStatus> GetStatusAsync(CancellationToken ct)
    {
        var studioVersion = GetHotDesignVersion() ?? "N/A";
        var userName = Environment.UserName;
        var machineName = Environment.MachineName;

        // License info isn't queryable without the Studio API
        // Show real user identity, indicate when license is unknown
        return ValueTask.FromResult(new StudioStatus(
            studioVersion,
            studioVersion != "N/A" ? "Active" : "Not detected",
            userName,
            $"{userName}@{machineName}",
            DateOnly.MinValue,
            studioVersion != "N/A"));
    }

    public ValueTask<ImmutableList<StudioFeature>> GetFeaturesAsync(CancellationToken ct)
    {
        var features = new List<StudioFeature>();

        // Hot Reload — always available in debug
        var isDebug = false;
#if DEBUG
        isDebug = true;
#endif
        features.Add(new StudioFeature("Hot Reload", "Live XAML and C# updates", isDebug, "Free"));

        // Hot Design — check if the assembly is loaded
        var hotDesignLoaded = GetHotDesignVersion() is not null;
        features.Add(new StudioFeature("Hot Design", "Visual editor integration", hotDesignLoaded, "Professional"));

        // XAML Inspector — available via Uno dev tools
        features.Add(new StudioFeature("XAML Inspector", "Runtime visual tree browser", isDebug, "Free"));

        // Detect other capabilities from loaded assemblies
        var hasNavigation = IsAssemblyLoaded("Uno.Extensions.Navigation");
        features.Add(new StudioFeature("Navigation Extensions", "Region-based page navigation", hasNavigation, "Free"));

        var hasReactive = IsAssemblyLoaded("Uno.Extensions.Reactive");
        features.Add(new StudioFeature("MVUX / Reactive", "Feeds, States, and reactive data", hasReactive, "Free"));

        var hasToolkit = IsAssemblyLoaded("Uno.Toolkit.UI");
        features.Add(new StudioFeature("Toolkit UI", "AutoLayout, Card, Chip, and more", hasToolkit, "Free"));

        return ValueTask.FromResult(features.ToImmutableList());
    }

    public ValueTask<ImmutableList<McpConnector>> GetConnectorsAsync(CancellationToken ct)
    {
        // MCP connector discovery is limited without the MCP protocol
        // Detect what we can from environment
        var connectors = new List<McpConnector>();

        // Check for claude config that might list MCP servers
        var claudeConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude", "settings.json");

        if (File.Exists(claudeConfig))
        {
            connectors.Add(new McpConnector("Claude Code", "local", true, 0, "Detected"));
        }

        // The Uno Platform MCP is available if we're running in a Claude Code session
        var unoMcpAvailable = Environment.GetEnvironmentVariable("MCP_SERVER_UNO") is not null
                           || File.Exists(claudeConfig); // If Claude is configured, Uno MCP is likely available
        if (unoMcpAvailable)
        {
            connectors.Add(new McpConnector("Uno Platform", "mcp.platform.uno", true, 0, "Available"));
        }

        return ValueTask.FromResult(connectors.ToImmutableList());
    }

    private static string? GetHotDesignVersion()
    {
        var asm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.Contains("HotDesign", StringComparison.OrdinalIgnoreCase) == true
                              || a.GetName().Name?.Contains("Uno.UI.RemoteControl", StringComparison.OrdinalIgnoreCase) == true);

        if (asm is null)
            return null;

        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (info is not null)
        {
            var plus = info.IndexOf('+');
            return plus >= 0 ? info[..plus] : info;
        }

        return asm.GetName().Version?.ToString(3);
    }

    private static bool IsAssemblyLoaded(string name) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.GetName().Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
}
