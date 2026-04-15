using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Orbital.Services;

public class RuntimeEnvironmentService : IEnvironmentService
{
    private readonly IProjectContext _ctx;
    private int? _cachedWorkloadCount;

    public RuntimeEnvironmentService(IProjectContext ctx) => _ctx = ctx;

    public async ValueTask<EnvironmentStatus> GetStatusAsync(CancellationToken ct)
    {
        var unoVersion = GetUnoSdkVersion();
        var dotnetVersion = Environment.Version.ToString();
        var workloadCount = await GetWorkloadCountAsync(ct);
        var renderer = GetRendererName();

        return new EnvironmentStatus(unoVersion, dotnetVersion, workloadCount, renderer);
    }

    public ValueTask<VersionInfo> GetVersionInfoAsync(CancellationToken ct)
    {
        var unoVersion = GetUnoSdkVersion();
        var dotnetVersion = Environment.Version.ToString();

        return ValueTask.FromResult(new VersionInfo(
            GetAppVersion(),
            unoVersion,
            dotnetVersion,
            "Claude Sonnet 4", // TODO: pull from config/MCP when available
            3)); // TODO: pull from MCP service
    }

    public ValueTask<ProjectMeta> GetCurrentProjectAsync(CancellationToken ct)
    {
        var active = _ctx.ActiveProject;
        if (active is not null)
        {
            return ValueTask.FromResult(new ProjectMeta(
                active.Name,
                active.RootDirectory,
                active.Branch ?? "main",
                GetTargetFramework(),
                TimeAgo(active.LastOpened),
                active.Status));
        }

        var asm = Assembly.GetEntryAssembly();
        var name = asm?.GetName().Name ?? "Unknown";
        var basePath = AppContext.BaseDirectory;
        var tfm = GetTargetFramework();

        return ValueTask.FromResult(new ProjectMeta(
            name,
            basePath,
            "main",
            tfm,
            "just now",
            HealthStatus.Ok));
    }

    public async ValueTask<ImmutableList<RecentProject>> GetRecentProjectsAsync(CancellationToken ct)
    {
        var recents = await _ctx.GetRecentProjectsAsync(ct);
        if (recents.Count > 0)
        {
            return recents.Select(p => new RecentProject(
                p.Name,
                p.RootDirectory,
                p.Branch ?? "main",
                TimeAgo(p.LastOpened),
                p.Status)).ToImmutableList();
        }

        var asm = Assembly.GetEntryAssembly();
        var name = asm?.GetName().Name ?? "Unknown";
        return ImmutableList.Create(
            new RecentProject(name, AppContext.BaseDirectory, "main", "just now", HealthStatus.Ok));
    }

    public async ValueTask<ProjectCreateResult> CreateProjectAsync(string name, string outputPath, CancellationToken ct)
    {
        var fullPath = Path.Combine(outputPath, name);
        try
        {
            Directory.CreateDirectory(outputPath);

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"new unoapp -n \"{name}\" -o \"{fullPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            var stdout = await process.StandardOutput.ReadToEndAsync(ct);
            var stderr = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            var output = string.IsNullOrWhiteSpace(stderr)
                ? stdout.Trim()
                : $"{stdout.Trim()}\n{stderr.Trim()}";

            return new ProjectCreateResult(process.ExitCode == 0, fullPath, output);
        }
        catch (Exception ex)
        {
            return new ProjectCreateResult(false, fullPath, ex.Message);
        }
    }

    private static string TimeAgo(DateTime dt)
    {
        var elapsed = DateTime.Now - dt;
        return elapsed.TotalMinutes < 1 ? "just now"
            : elapsed.TotalMinutes < 60 ? $"{(int)elapsed.TotalMinutes}m ago"
            : elapsed.TotalHours < 24 ? $"{(int)elapsed.TotalHours}hr ago"
            : $"{(int)elapsed.TotalDays}d ago";
    }

    private static string GetUnoSdkVersion()
    {
        // Get the version from the Uno.UI assembly (ships as the Uno.WinUI NuGet)
        var unoAssembly = typeof(Microsoft.UI.Xaml.Application).Assembly;
        var infoVersion = unoAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (infoVersion is not null)
        {
            // Strip any +commit hash suffix (e.g. "5.4.100+abc123" -> "5.4.100")
            var plusIndex = infoVersion.IndexOf('+');
            return plusIndex >= 0 ? infoVersion[..plusIndex] : infoVersion;
        }

        return unoAssembly.GetName().Version?.ToString(3) ?? "unknown";
    }

    private static string GetAppVersion()
    {
        var asm = Assembly.GetEntryAssembly();
        var infoVersion = asm?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (infoVersion is not null)
        {
            var plusIndex = infoVersion.IndexOf('+');
            return plusIndex >= 0 ? infoVersion[..plusIndex] : infoVersion;
        }

        return asm?.GetName().Version?.ToString() ?? "0.0.0";
    }

    private static string GetRendererName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Skia/WPF";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Skia/X11";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "Skia/macOS";
        return "Skia/Desktop";
    }

    private static string GetTargetFramework()
    {
        var tfm = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?
            .FrameworkName;

        // e.g. ".NETCoreApp,Version=v10.0" -> "net10.0-desktop"
        if (tfm is not null && tfm.Contains("Version=v"))
        {
            var versionStart = tfm.IndexOf("Version=v") + 9;
            var version = tfm[versionStart..];
            return $"net{version}-desktop";
        }

        return "net10.0-desktop";
    }

    private async ValueTask<int> GetWorkloadCountAsync(CancellationToken ct)
    {
        if (_cachedWorkloadCount.HasValue)
            return _cachedWorkloadCount.Value;

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "workload list",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            // Use Task.WhenAny with a delay to guarantee we don't hang
            var readTask = process.StandardOutput.ReadToEndAsync(ct).AsTask();
            var completed = await Task.WhenAny(readTask, Task.Delay(5000, ct));
            if (completed != readTask)
            {
                try { process.Kill(); } catch { }
                _cachedWorkloadCount = 0;
                return 0;
            }

            var output = await readTask;

            var lines = output.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var count = 0;
            var pastHeader = false;
            foreach (var line in lines)
            {
                if (line.StartsWith("---") || line.StartsWith("==="))
                {
                    pastHeader = true;
                    continue;
                }
                if (pastHeader && line.Length > 0 && !line.StartsWith("Use ") && !line.StartsWith("Installed"))
                    count++;
            }

            _cachedWorkloadCount = count;
            return count;
        }
        catch
        {
            _cachedWorkloadCount = 0;
            return 0;
        }
    }
}
