namespace Unoblueprint.Models;

public class PluginInfo
{
    public string Name { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int ActiveInstallations { get; set; } = 0;
    public bool IsInstalled { get; set; } = false;
}
