namespace ClaudeDash.Models;

public class SkillInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsSymlink { get; set; }
    public string ResolvedTarget { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Invocations { get; set; }
    public double Accuracy { get; set; }
    public string AvgReadTime { get; set; } = string.Empty;
    public string Category { get; set; } = "core"; // "core", "user", "example"
}
