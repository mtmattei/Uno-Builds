using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClaudeDash.ViewModels;

public partial record DevicesModel
{
    private readonly ILogger<DevicesModel> _logger;

    public DevicesModel(ILogger<DevicesModel> logger)
    {
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListFeed<DeviceInfoItem> Items => ListFeed.Async(async ct =>
    {
        try
        {
            var items = new List<DeviceInfoItem>();

            // OS
            items.Add(new DeviceInfoItem("OS", RuntimeInformation.OSDescription, "ok"));
            items.Add(new DeviceInfoItem("Architecture", RuntimeInformation.OSArchitecture.ToString(), "ok"));
            items.Add(new DeviceInfoItem("Platform", RuntimeInformation.RuntimeIdentifier, "ok"));

            // Runtime
            items.Add(new DeviceInfoItem(".NET Runtime", RuntimeInformation.FrameworkDescription, "ok"));
            items.Add(new DeviceInfoItem("Process Arch", RuntimeInformation.ProcessArchitecture.ToString(), "ok"));

            // Machine
            items.Add(new DeviceInfoItem("Machine Name", Environment.MachineName, "ok"));
            items.Add(new DeviceInfoItem("User Name", Environment.UserName, "ok"));
            items.Add(new DeviceInfoItem("Processors", Environment.ProcessorCount.ToString(), "ok"));

            // Memory
            var proc = Process.GetCurrentProcess();
            var workingSetMb = proc.WorkingSet64 / (1024.0 * 1024.0);
            items.Add(new DeviceInfoItem("App Memory", $"{workingSetMb:F1} MB", workingSetMb > 500 ? "warn" : "ok"));

            // Uptime
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            items.Add(new DeviceInfoItem("System Uptime", $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m", "ok"));

            // Drives
            try
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
                {
                    var totalGb = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    var freeGb = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    var usedPct = ((totalGb - freeGb) / totalGb) * 100;
                    var status = usedPct > 90 ? "error" : usedPct > 75 ? "warn" : "ok";
                    items.Add(new DeviceInfoItem($"Disk {drive.Name.TrimEnd('\\')}", $"{freeGb:F1} GB free / {totalGb:F1} GB ({usedPct:F0}% used)", status));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enumerate drives");
            }

            // Environment variables of interest
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";
            var pathCount = path.Split(System.IO.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Length;
            items.Add(new DeviceInfoItem("PATH Entries", pathCount.ToString(), pathCount > 100 ? "warn" : "ok"));

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            items.Add(new DeviceInfoItem("Home Dir", home, "ok"));

            return items.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load device info");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<DeviceInfoItem>.Empty;
        }
    });
}
