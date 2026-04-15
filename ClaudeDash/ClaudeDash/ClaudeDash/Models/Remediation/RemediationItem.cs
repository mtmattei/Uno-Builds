namespace ClaudeDash.Models.Remediation;

public class RemediationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public RemediationCategory Category { get; set; }
    public RemediationSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FixLabel { get; set; } = "Fix";
    public string? TargetPath { get; set; }

    /// <summary>
    /// Whether this issue can be auto-fixed by the service.
    /// </summary>
    public bool IsFixable { get; set; }

    /// <summary>
    /// Estimated disk space freed or impact description.
    /// </summary>
    public string? Impact { get; set; }
}
