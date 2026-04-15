namespace ClaudeDash.Models;

public class DepUpdateInfo
{
    public string PackageName { get; set; } = string.Empty;
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public string UpdateType { get; set; } = string.Empty; // "major", "minor", "patch"
    public string RepoName { get; set; } = string.Empty;
}
