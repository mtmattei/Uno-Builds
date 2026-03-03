using System.Collections.ObjectModel;

namespace AdTokensIDE.Models;

public class SolutionItem
{
    public string Name { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "\uE8B7";
    public bool IsFolder { get; set; }
    public bool IsExpanded { get; set; }
    public ObservableCollection<SolutionItem> Children { get; set; } = new();
}
