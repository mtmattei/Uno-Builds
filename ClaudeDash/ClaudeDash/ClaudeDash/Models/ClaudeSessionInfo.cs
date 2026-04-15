namespace ClaudeDash.Models;

public class ClaudeSessionInfo
{
    public string SessionId { get; set; } = string.Empty;
    public string ShortId { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string FirstUserMessage { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
    public string Model { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public string GitBranch { get; set; } = string.Empty;
    public string ClaudeVersion { get; set; } = string.Empty;

    // New properties for terminal-style sessions page
    public int TokenCount { get; set; }
    public string Duration { get; set; } = string.Empty;
    public string Status { get; set; } = "completed"; // "running", "completed", "error"
    public string Subtitle { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public double CostAmount { get; set; }
}
