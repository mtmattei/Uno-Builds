namespace ClaudeDash.Models;

public enum ToolchainStatus { Pass, Warn, Fail, Unknown }

public record ToolchainItem(
    string Category = "",
    string Name = "",
    string Version = "",
    ToolchainStatus Status = ToolchainStatus.Unknown,
    string Detail = "",
    string FixCommand = "",
    string FixLabel = "");
