namespace ClaudeDash.Models;

public class RepoInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public DateTime LastActivity { get; set; }
    public string LastBranch { get; set; } = string.Empty;

    // Enriched fields for terminal dashboard
    public string TechStack { get; set; } = string.Empty;
    public bool IsDirty { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public int DepCount { get; set; }
    public double[] LanguageSegments { get; set; } = [];
    public string[] LanguageColorHexes { get; set; } = [];
}
