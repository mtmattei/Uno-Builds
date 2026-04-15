using System.Diagnostics;
#if !__WASM__
using System.Management;
#endif

namespace ClaudeDash.Services;

public class AgentLauncherService : IAgentLauncherService
{
    public Task<List<RunningAgent>> GetRunningAgentsAsync()
    {
        var agents = new List<RunningAgent>();

        try
        {
            var processes = Process.GetProcessesByName("claude");
            foreach (var proc in processes)
            {
                try
                {
                    agents.Add(new RunningAgent
                    {
                        ProcessId = proc.Id,
                        StartedAt = proc.StartTime.ToUniversalTime(),
                        Uptime = DateTime.UtcNow - proc.StartTime.ToUniversalTime(),
                        CommandLine = TryGetCommandLine(proc.Id),
                        WorkingDirectory = TryGetWorkingDirectory(proc.Id)
                    });
                }
                catch
                {
                    // Skip processes we can't access
                }
            }
        }
        catch
        {
            // Graceful degradation
        }

        return Task.FromResult(agents);
    }

    public Task<int> LaunchAgentAsync(string projectPath, string? prompt = null, string? model = null)
    {
        var args = "";
        if (!string.IsNullOrEmpty(prompt))
            args += $" -p \"{prompt}\"";
        if (!string.IsNullOrEmpty(model))
            args += $" --model {model}";

        var psi = new ProcessStartInfo
        {
            FileName = "claude",
            Arguments = args.Trim(),
            WorkingDirectory = projectPath,
            UseShellExecute = true
        };

        var proc = Process.Start(psi);
        return Task.FromResult(proc?.Id ?? -1);
    }

    public Task StopAgentAsync(int processId)
    {
        try
        {
            var proc = Process.GetProcessById(processId);
            proc.Kill(entireProcessTree: true);
        }
        catch
        {
            // Process may already be gone
        }

        return Task.CompletedTask;
    }

    private static string TryGetCommandLine(int processId)
    {
#if !__WASM__
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");
            foreach (var obj in searcher.Get())
            {
                return obj["CommandLine"]?.ToString() ?? "";
            }
        }
        catch { }
#endif
        return "";
    }

    private static string TryGetWorkingDirectory(int processId)
    {
        // WMI doesn't reliably provide working directory
        // Fall back to empty - could be enhanced with /proc on Linux
        return "";
    }
}
