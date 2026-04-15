namespace ClaudeDash.Models;

public class GitStatusInfo
{
    public bool IsDirty { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public string CurrentBranch { get; set; } = string.Empty;
    public int UntrackedCount { get; set; }
}
