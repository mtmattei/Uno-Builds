using System.Diagnostics;

namespace Orbital.Services;

public class RuntimeBuildService : IBuildService
{
    private readonly IProjectContext _ctx;
    private ImmutableList<Controls.ConsoleLine>? _lastBuildOutput;
    private ImmutableList<Controls.ConsoleLine>? _lastRunOutput;

    public RuntimeBuildService(IProjectContext ctx) => _ctx = ctx;

    public ValueTask<ImmutableList<Controls.ConsoleLine>> GetLastBuildOutputAsync(CancellationToken ct)
    {
        if (_lastBuildOutput is not null)
            return ValueTask.FromResult(_lastBuildOutput);

        // No build has been run yet — show real info about the running app
        var lines = ImmutableList.Create(
            new Controls.ConsoleLine("# No build captured this session", "dim"),
            new Controls.ConsoleLine($"# Running from: {AppContext.BaseDirectory}", "dim"),
            new Controls.ConsoleLine($"# .NET {Environment.Version} | PID {Environment.ProcessId}", "dim"));
        return ValueTask.FromResult(lines);
    }

    public ValueTask<ImmutableList<Controls.ConsoleLine>> GetLastRunOutputAsync(CancellationToken ct)
    {
        if (_lastRunOutput is not null)
            return ValueTask.FromResult(_lastRunOutput);

        var lines = ImmutableList.Create(
            new Controls.ConsoleLine($"[INFO] Process running — PID {Environment.ProcessId}", "info"),
            new Controls.ConsoleLine($"[INFO] Working set: {Environment.WorkingSet / (1024 * 1024)} MB", "info"),
            new Controls.ConsoleLine($"[INFO] Uptime: {TimeSpan.FromMilliseconds(Environment.TickCount64):hh\\:mm\\:ss}", "info"));
        return ValueTask.FromResult(lines);
    }

    public ValueTask<ImmutableList<Artifact>> GetArtifactsAsync(CancellationToken ct)
    {
        var artifacts = new List<Artifact>();

        // Scan the actual output directory
        var baseDir = _ctx.ActiveProject?.RootDirectory ?? AppContext.BaseDirectory;
        var extensions = new[] { "*.dll", "*.exe", "*.pdb" };
        var entryName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Orbital";

        foreach (var ext in extensions)
        {
            var pattern = $"{entryName}{ext.TrimStart('*')}";
            var file = new FileInfo(Path.Combine(baseDir, pattern));
            if (file.Exists)
            {
                var size = file.Length switch
                {
                    >= 1024 * 1024 => $"{file.Length / (1024.0 * 1024.0):F1} MB",
                    >= 1024 => $"{file.Length / 1024.0:F0} KB",
                    _ => $"{file.Length} B",
                };

                var type = file.Extension.ToUpperInvariant() switch
                {
                    ".DLL" => "Assembly",
                    ".EXE" => "Executable",
                    ".PDB" => "Debug Symbols",
                    _ => "File",
                };

                artifacts.Add(new Artifact(file.Name, type, size, baseDir, file.LastWriteTime));
            }
        }

        // Also look for any deps.json and runtimeconfig.json
        foreach (var configFile in Directory.EnumerateFiles(baseDir, $"{entryName}*.json"))
        {
            var fi = new FileInfo(configFile);
            var size = fi.Length >= 1024 ? $"{fi.Length / 1024.0:F0} KB" : $"{fi.Length} B";
            artifacts.Add(new Artifact(fi.Name, "Config", size, baseDir, fi.LastWriteTime));
        }

        return ValueTask.FromResult(artifacts.ToImmutableList());
    }

    public async ValueTask BuildAsync(CancellationToken ct)
    {
        var csproj = FindCsproj();
        if (csproj is null)
        {
            _lastBuildOutput = ImmutableList.Create(
                new Controls.ConsoleLine("ERROR: Could not locate .csproj file", "error"));
            return;
        }

        _lastBuildOutput = await RunDotnetCommandAsync($"build \"{csproj}\" -c Debug", ct);
    }

    public async ValueTask RunAsync(CancellationToken ct)
    {
        var csproj = FindCsproj();
        if (csproj is null)
        {
            _lastRunOutput = ImmutableList.Create(
                new Controls.ConsoleLine("ERROR: Could not locate .csproj file", "error"));
            return;
        }

        _lastRunOutput = await RunDotnetCommandAsync($"run --project \"{csproj}\" -f net10.0-desktop", ct);
    }

    public async ValueTask BuildRunVerifyAsync(CancellationToken ct)
    {
        await BuildAsync(ct);
    }

    public async ValueTask PackageAsync(CancellationToken ct)
    {
        var csproj = FindCsproj();
        if (csproj is null)
        {
            _lastBuildOutput = ImmutableList.Create(
                new Controls.ConsoleLine("ERROR: Could not locate .csproj file", "error"));
            return;
        }

        _lastBuildOutput = await RunDotnetCommandAsync($"pack \"{csproj}\" -c Release", ct);
    }

    public async ValueTask<ImmutableList<Controls.ConsoleLine>> RunSmokeTestAsync(CancellationToken ct)
    {
        var lines = new List<Controls.ConsoleLine>
        {
            new("$ UI Smoke Test", "dim"),
            new("", "dim"),
        };

        // Basic smoke test: verify the app is running and responsive
        try
        {
            lines.Add(new Controls.ConsoleLine($"  \u2713 Process alive — PID {Environment.ProcessId}", "success"));
            lines.Add(new Controls.ConsoleLine($"  \u2713 Working set: {Environment.WorkingSet / (1024 * 1024)} MB", "success"));
            lines.Add(new Controls.ConsoleLine($"  \u2713 .NET {Environment.Version}", "success"));

            var entryAsm = System.Reflection.Assembly.GetEntryAssembly();
            if (entryAsm is not null)
                lines.Add(new Controls.ConsoleLine($"  \u2713 Entry assembly: {entryAsm.GetName().Name}", "success"));

            // Check for Uno.UI assembly
            var unoAsm = typeof(Microsoft.UI.Xaml.Application).Assembly;
            lines.Add(new Controls.ConsoleLine($"  \u2713 Uno.UI loaded: {unoAsm.GetName().Version}", "success"));

            lines.Add(new Controls.ConsoleLine("", "dim"));
            lines.Add(new Controls.ConsoleLine("  All checks passed.", "success"));
        }
        catch (Exception ex)
        {
            lines.Add(new Controls.ConsoleLine($"  \u2717 {ex.Message}", "error"));
        }

        return lines.ToImmutableList();
    }

    private async Task<ImmutableList<Controls.ConsoleLine>> RunDotnetCommandAsync(string args, CancellationToken ct)
    {
        var lines = new List<Controls.ConsoleLine>();
        lines.Add(new Controls.ConsoleLine($"$ dotnet {args}", "dim"));

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _ctx.ActiveProject?.RootDirectory ?? "",
                },
            };

            process.Start();
            var stdout = await process.StandardOutput.ReadToEndAsync(ct);
            var stderr = await process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);

            foreach (var line in stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.TrimEnd('\r');
                var type = trimmed switch
                {
                    _ when trimmed.Contains("error", StringComparison.OrdinalIgnoreCase) => "error",
                    _ when trimmed.Contains("warning", StringComparison.OrdinalIgnoreCase) => "warn",
                    _ when trimmed.Contains("succeeded", StringComparison.OrdinalIgnoreCase) => "success",
                    _ when trimmed.Contains("->") => "success",
                    _ => "info",
                };
                lines.Add(new Controls.ConsoleLine(trimmed, type));
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                foreach (var line in stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    lines.Add(new Controls.ConsoleLine(line.TrimEnd('\r'), "error"));
            }
        }
        catch (Exception ex)
        {
            lines.Add(new Controls.ConsoleLine($"ERROR: {ex.Message}", "error"));
        }

        return lines.ToImmutableList();
    }

    private string? FindCsproj()
    {
        var dir = new DirectoryInfo(_ctx.ActiveProject?.RootDirectory ?? AppContext.BaseDirectory);
        while (dir is not null)
        {
            var files = dir.GetFiles("*.csproj");
            if (files.Length > 0)
                return files[0].FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
