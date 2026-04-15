namespace ClaudeDash.Models;

public record EvidenceItem(
    string Label = "",
    int Value = 0,
    ToolchainStatus Status = ToolchainStatus.Pass,
    string TimeAgo = "",
    string NavigationTarget = "");
