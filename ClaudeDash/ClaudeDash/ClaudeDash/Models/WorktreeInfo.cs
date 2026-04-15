namespace ClaudeDash.Models;

public class WorktreeInfo
{
    public string Path { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string HeadShort { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public string RepoPath { get; set; } = string.Empty;
}
