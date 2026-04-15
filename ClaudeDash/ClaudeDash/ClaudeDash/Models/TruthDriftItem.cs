namespace ClaudeDash.Models;

public enum DriftStatus { Aligned, Drifted, Error }

public record TruthDriftItem(
    string Title = "",
    string Description = "",
    DriftStatus Status = DriftStatus.Aligned,
    string ActionLabel = "",
    string ActionCommand = "",
    string NavigationTarget = "",
    string ActionNavigationTarget = "");
