namespace ClaudeDash.Models.Remediation;

public class RemediationScanResult
{
    public List<RemediationItem> Items { get; set; } = [];
    public int InfoCount => Items.Count(i => i.Severity == RemediationSeverity.Info);
    public int WarningCount => Items.Count(i => i.Severity == RemediationSeverity.Warning);
    public int ErrorCount => Items.Count(i => i.Severity == RemediationSeverity.Error);
    public int FixableCount => Items.Count(i => i.IsFixable);
    public TimeSpan ScanDuration { get; set; }
}
