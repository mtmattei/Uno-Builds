namespace ClaudeDash.Models;

public class HookInfo
{
    public string Matcher { get; set; } = string.Empty;
    public string HookType { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    // New properties for terminal-style hooks page
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string TriggerType { get; set; } = string.Empty;
}
