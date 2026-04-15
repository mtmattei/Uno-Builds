namespace ClaudeDash.Models;

public class RalphLoopConfig
{
    public string ProjectName { get; set; } = "";
    public string PrdContent { get; set; } = "";
    public List<string> TargetPlatforms { get; set; } = new();
    public string Theme { get; set; } = "Material";
    public string MarkupStyle { get; set; } = "XAML";
    public string MvvmPattern { get; set; } = "MVVM";

    // 7-stage pipeline fields
    public string IdeaInput { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string GeneratedPrd { get; set; } = "";
    public string DesignSpec { get; set; } = "";
    public string ScaffoldOutput { get; set; } = "";
    public string BuildOutput { get; set; } = "";
    public string TestResults { get; set; } = "";
    public string ShipManifest { get; set; } = "";
}
