using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Orbital.Services;

public class RuntimeDiagnosticsService : IDiagnosticsService
{
    private readonly IProjectContext _ctx;

    public RuntimeDiagnosticsService(IProjectContext ctx) => _ctx = ctx;

    public async ValueTask<ImmutableList<DiagnosticsCheck>> GetChecksAsync(CancellationToken ct)
    {
        // Run all shell commands in parallel to avoid sequential process startup latency
        var dotnetTask = RunCommandAsync("dotnet", "--version", ct);
        var javaTask = RunCommandAsync("java", "--version", ct);
        Task<string?>? xcodeTask = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? RunCommandAsync("xcodebuild", "-version", ct) : null;
        Task<string?>? gtkTask = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? RunCommandAsync("pkg-config", "--modversion gtk+-3.0", ct) : null;

        // Await all launched tasks at once
        var tasks = new List<Task> { dotnetTask, javaTask };
        if (xcodeTask is not null) tasks.Add(xcodeTask);
        if (gtkTask is not null) tasks.Add(gtkTask);
        await Task.WhenAll(tasks);

        var checks = new List<DiagnosticsCheck>();

        // .NET SDK
        var dotnetVersion = dotnetTask.Result;
        checks.Add(dotnetVersion is not null
            ? new DiagnosticsCheck(".NET SDK", $"{dotnetVersion.Trim()} installed", HealthStatus.Ok)
            : new DiagnosticsCheck(".NET SDK", "Not found", HealthStatus.Error));

        // Uno.Sdk (from global.json — no process, just file read)
        var unoSdkVersion = ReadUnoSdkFromGlobalJson();
        checks.Add(unoSdkVersion is not null
            ? new DiagnosticsCheck("Uno.Sdk", $"{unoSdkVersion} installed", HealthStatus.Ok)
            : new DiagnosticsCheck("Uno.Sdk", "Not found in global.json", HealthStatus.Warn));

        // Android SDK
        var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME")
                       ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        if (androidHome is not null && Directory.Exists(androidHome))
        {
            var buildToolsDir = Path.Combine(androidHome, "build-tools");
            var latestBuildTools = Directory.Exists(buildToolsDir)
                ? Directory.GetDirectories(buildToolsDir).OrderDescending().FirstOrDefault()
                : null;
            var btName = latestBuildTools is not null ? Path.GetFileName(latestBuildTools) : "unknown";
            checks.Add(new DiagnosticsCheck("Android SDK", $"Build-tools {btName}", HealthStatus.Ok));
        }
        else
        {
            checks.Add(new DiagnosticsCheck("Android SDK", "ANDROID_HOME not set", HealthStatus.Warn));
        }

        // Xcode (macOS only)
        if (xcodeTask is not null)
        {
            var xcodeVersion = xcodeTask.Result;
            checks.Add(xcodeVersion is not null
                ? new DiagnosticsCheck("Xcode", xcodeVersion.Split('\n').FirstOrDefault()?.Trim() ?? "Installed", HealthStatus.Ok)
                : new DiagnosticsCheck("Xcode", "Not installed", HealthStatus.Warn));
        }
        else
        {
            checks.Add(new DiagnosticsCheck("Xcode", "N/A (not macOS)", HealthStatus.Idle));
        }

        // Java / OpenJDK
        var javaVersion = javaTask.Result;
        if (javaVersion is not null)
        {
            var firstLine = javaVersion.Split('\n').FirstOrDefault()?.Trim() ?? "Installed";
            checks.Add(new DiagnosticsCheck("OpenJDK", firstLine, HealthStatus.Ok));
        }
        else
        {
            checks.Add(new DiagnosticsCheck("OpenJDK", "Not found", HealthStatus.Warn));
        }

        // GTK (Linux only)
        if (gtkTask is not null)
        {
            var gtkVersion = gtkTask.Result;
            checks.Add(gtkVersion is not null
                ? new DiagnosticsCheck("GTK", $"{gtkVersion.Trim()} installed", HealthStatus.Ok)
                : new DiagnosticsCheck("GTK", "Not found", HealthStatus.Warn));
        }

        return checks.ToImmutableList();
    }

    public ValueTask<ImmutableList<DependencyInfo>> GetDependenciesAsync(CancellationToken ct)
    {
        var deps = new List<DependencyInfo>();

        // Scan loaded assemblies for key packages
        var targets = new (string DisplayName, string AssemblyPrefix, Type? Anchor)[]
        {
            ("Uno.WinUI", "Uno.UI", typeof(Microsoft.UI.Xaml.Application)),
            ("Uno.Extensions.Navigation", "Uno.Extensions.Navigation", null),
            ("Uno.Extensions.Reactive", "Uno.Extensions.Reactive", null),
            ("Uno.Toolkit.UI", "Uno.Toolkit.UI", null),
            ("Uno.Toolkit.UI.Material", "Uno.Toolkit.UI.Material", null),
            ("Microsoft.Extensions.Hosting", "Microsoft.Extensions.Hosting", typeof(IHost)),
            ("Uno.Resizetizer", "Uno.Resizetizer", null),
            ("SkiaSharp", "SkiaSharp", null),
        };

        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var (displayName, prefix, anchor) in targets)
        {
            string? version = null;

            if (anchor is not null)
            {
                version = GetAssemblyInfoVersion(anchor.Assembly);
            }
            else
            {
                var asm = loadedAssemblies.FirstOrDefault(a =>
                    a.GetName().Name?.Equals(prefix, StringComparison.OrdinalIgnoreCase) == true);
                if (asm is not null)
                    version = GetAssemblyInfoVersion(asm);
            }

            if (version is not null)
            {
                // We don't have NuGet latest version without an API call, so show current = latest for now
                deps.Add(new DependencyInfo(displayName, version, version, HealthStatus.Ok));
            }
        }

        return ValueTask.FromResult(deps.ToImmutableList());
    }

    public ValueTask<ImmutableList<PlatformTarget>> GetPlatformTargetsAsync(CancellationToken ct)
    {
        var targets = new List<PlatformTarget>();

        // Read TargetFrameworks from csproj if we can find it
        var tfms = ReadTargetFrameworksFromCsproj();

        // Desktop (Skia)
        var hasDesktop = tfms.Any(t => t.Contains("desktop", StringComparison.OrdinalIgnoreCase));
        targets.Add(new PlatformTarget("Desktop (Skia)", $".NET {Environment.Version.ToString(2)}", hasDesktop || tfms.Count == 0, HealthStatus.Ok));

        // WebAssembly
        var hasWasm = tfms.Any(t => t.Contains("browserwasm", StringComparison.OrdinalIgnoreCase));
        targets.Add(new PlatformTarget("WebAssembly", $".NET {Environment.Version.ToString(2)}", hasWasm,
            hasWasm ? HealthStatus.Ok : HealthStatus.Idle));

        // Android
        var hasAndroid = tfms.Any(t => t.Contains("android", StringComparison.OrdinalIgnoreCase));
        var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME") ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        var androidInstalled = hasAndroid && androidHome is not null && Directory.Exists(androidHome);
        targets.Add(new PlatformTarget("Android", "API 35", androidInstalled,
            hasAndroid ? (androidInstalled ? HealthStatus.Ok : HealthStatus.Warn) : HealthStatus.Idle));

        // iOS
        var hasIos = tfms.Any(t => t.Contains("ios", StringComparison.OrdinalIgnoreCase));
        var iosInstalled = hasIos && RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        targets.Add(new PlatformTarget("iOS", "Xcode 16", iosInstalled,
            hasIos ? (iosInstalled ? HealthStatus.Ok : HealthStatus.Warn) : HealthStatus.Idle));

        // macOS Catalyst
        var hasMac = tfms.Any(t => t.Contains("maccatalyst", StringComparison.OrdinalIgnoreCase));
        var macInstalled = hasMac && RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        targets.Add(new PlatformTarget("macOS (Catalyst)", "Xcode 16", macInstalled,
            hasMac ? (macInstalled ? HealthStatus.Ok : HealthStatus.Warn) : HealthStatus.Idle));

        return ValueTask.FromResult(targets.ToImmutableList());
    }

    public ValueTask<ImmutableList<RuntimeTool>> GetRuntimeToolsAsync(CancellationToken ct)
    {
        var tools = new List<RuntimeTool>
        {
            new("Hot Reload", "Live XAML and C# updates",
#if DEBUG
                "Active", HealthStatus.Ok, "emerald"),
#else
                "Disabled", HealthStatus.Idle, "emerald"),
#endif
            new("XAML Inspector", "Runtime visual tree browser", "Ready", HealthStatus.Ok, "blue"),
            new("Screenshot", "Capture app screenshots via MCP", "Ready", HealthStatus.Ok, "violet"),
            new(".NET SDK", "Build and runtime tools", Environment.Version.ToString(), HealthStatus.Ok, "blue"),
            new("Hot Design",
#if DEBUG
                "Visual designer integration", "Active", HealthStatus.Ok, "emerald"),
#else
                "Visual designer integration", "Disabled", HealthStatus.Idle, "emerald"),
#endif
            new("Performance Profiler", "Frame timing analysis", "Idle", HealthStatus.Idle, "amber"),
        };

        return ValueTask.FromResult(tools.ToImmutableList());
    }

    // --- Helpers ---

    private static async Task<string?> RunCommandAsync(string command, string args, CancellationToken ct)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            // Enforce 10-second timeout to prevent hanging on broken tools
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            var token = timeout.Token;

            var output = await process.StandardOutput.ReadToEndAsync(token);
            var error = await process.StandardError.ReadToEndAsync(token);
            await process.WaitForExitAsync(token);

            // java --version writes to stderr
            return !string.IsNullOrWhiteSpace(output) ? output
                 : !string.IsNullOrWhiteSpace(error) ? error
                 : null;
        }
        catch
        {
            return null;
        }
    }

    private string? ReadUnoSdkFromGlobalJson()
    {
        try
        {
            // Walk up from the active project root (or app base) to find global.json
            var dir = new DirectoryInfo(_ctx.ActiveProject?.RootDirectory ?? AppContext.BaseDirectory);
            while (dir is not null)
            {
                var globalJsonPath = Path.Combine(dir.FullName, "global.json");
                if (File.Exists(globalJsonPath))
                {
                    var json = JsonDocument.Parse(File.ReadAllText(globalJsonPath));
                    if (json.RootElement.TryGetProperty("msbuild-sdks", out var sdks) &&
                        sdks.TryGetProperty("Uno.Sdk", out var version))
                    {
                        return version.GetString();
                    }
                }
                dir = dir.Parent;
            }
        }
        catch
        {
            // Fall through
        }

        return null;
    }

    private List<string> ReadTargetFrameworksFromCsproj()
    {
        try
        {
            var dir = new DirectoryInfo(_ctx.ActiveProject?.RootDirectory ?? AppContext.BaseDirectory);
            while (dir is not null)
            {
                var csprojFiles = dir.GetFiles("*.csproj");
                if (csprojFiles.Length > 0)
                {
                    var content = File.ReadAllText(csprojFiles[0].FullName);
                    // Simple parse: look for <TargetFrameworks> or <TargetFramework>
                    var match = System.Text.RegularExpressions.Regex.Match(content,
                        @"<TargetFrameworks?>(.*?)</TargetFrameworks?>",
                        System.Text.RegularExpressions.RegexOptions.Singleline);
                    if (match.Success)
                    {
                        return match.Groups[1].Value
                            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .ToList();
                    }
                }
                dir = dir.Parent;
            }
        }
        catch
        {
            // Fall through
        }

        return [];
    }

    private static string? GetAssemblyInfoVersion(Assembly asm)
    {
        var infoVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (infoVersion is not null)
        {
            var plusIndex = infoVersion.IndexOf('+');
            return plusIndex >= 0 ? infoVersion[..plusIndex] : infoVersion;
        }

        return asm.GetName().Version?.ToString(3);
    }
}
