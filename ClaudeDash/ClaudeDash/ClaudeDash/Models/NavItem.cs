namespace ClaudeDash.Models;

public partial record NavItem(string Key, string Label, string Icon, string Group, string Description = "")
{
    public string Tooltip => Description;
}
