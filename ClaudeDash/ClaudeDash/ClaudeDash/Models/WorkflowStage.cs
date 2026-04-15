namespace ClaudeDash.Models;

public enum StageStatus { Completed, Active, Blocked, Pending }

public record WorkflowStage(
    string Name = "",
    string IconGlyph = "",
    StageStatus Status = StageStatus.Pending,
    string Description = "");
