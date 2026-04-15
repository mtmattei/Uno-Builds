namespace ClaudeDash.Services;

public interface IRemediationService
{
    Task<RemediationScanResult> ScanAsync();
    Task<FixResult> ApplyFixAsync(RemediationItem item);
    Task<FixResult> DismissAsync(RemediationItem item);
}
