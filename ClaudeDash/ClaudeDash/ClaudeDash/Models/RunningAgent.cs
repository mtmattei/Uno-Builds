namespace ClaudeDash.Models;

public class RunningAgent
{
    public int ProcessId { get; set; }
    public string CommandLine { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public TimeSpan Uptime { get; set; }
    public DateTime StartedAt { get; set; }
}
