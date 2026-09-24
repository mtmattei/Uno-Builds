namespace FieldCheck.Models;

public enum InspectionCondition
{
    Good,
    Attention,
    Critical,
}

public sealed record Inspection(
    string Id,
    string AssetId,
    string AssetName,
    DateTime Date,
    InspectionCondition Condition,
    bool OperatingNormally,
    double TemperatureC,
    string Inspector,
    string? Notes,
    string? IssueDescription = null,
    string? AttachmentFileName = null)
{
    public override string ToString() => $"{AssetName}, {Id}, {Condition}, {Inspector}";
}

public sealed record InspectionDraft(
    string AssetId,
    InspectionCondition Condition,
    bool OperatingNormally,
    double TemperatureC,
    string? Notes,
    string? IssueDescription);

/// <summary>A file chosen through the platform picker.</summary>
public sealed record PickedFile(string FileName, Func<Task<Stream>> OpenReadAsync);
