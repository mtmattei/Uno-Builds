using System.Text.RegularExpressions;

namespace Orbital.Services;

public partial class RuntimeSkillsService : ISkillsService
{
    private List<SkillInfo>? _skills;

    public ValueTask<ImmutableList<SkillInfo>> GetSkillsAsync(CancellationToken ct)
    {
        _skills ??= ScanSkillDirectories();
        return ValueTask.FromResult(_skills.ToImmutableList());
    }

    public ValueTask ToggleSkillAsync(string skillId, bool active, CancellationToken ct)
    {
        _skills ??= ScanSkillDirectories();
        var index = _skills.FindIndex(s => s.Id == skillId);
        if (index >= 0)
            _skills[index] = _skills[index] with { IsActive = active };
        return ValueTask.CompletedTask;
    }

    private static List<SkillInfo> ScanSkillDirectories()
    {
        var skills = new List<SkillInfo>();
        var skillsRoot = GetSkillsRoot();

        if (skillsRoot is null || !Directory.Exists(skillsRoot))
            return skills;

        var dirs = Directory.GetDirectories(skillsRoot);
        var counter = 0;

        foreach (var dir in dirs.OrderBy(d => d))
        {
            var skillFile = Path.Combine(dir, "SKILL.md");
            if (!File.Exists(skillFile))
                continue;

            counter++;
            var content = File.ReadAllText(skillFile);
            var (name, description, category) = ParseFrontmatter(content);
            var dirName = Path.GetFileName(dir);

            skills.Add(new SkillInfo(
                Id: $"sk-{counter:D3}",
                Name: name ?? dirName,
                Description: description ?? "",
                Category: InferCategory(dirName),
                IsActive: true,
                Invocations: 0,
                Accuracy: 0.0,
                Path: dir));
        }

        return skills;
    }

    private static (string? Name, string? Description, string? Category) ParseFrontmatter(string content)
    {
        var match = FrontmatterRegex().Match(content);
        if (!match.Success)
            return (null, null, null);

        var yaml = match.Groups[1].Value;

        var name = ExtractYamlValue(yaml, "name");
        var description = ExtractYamlValue(yaml, "description");
        var category = ExtractYamlValue(yaml, "category");

        return (name, description, category);
    }

    private static string? ExtractYamlValue(string yaml, string key)
    {
        var pattern = $@"^{key}:\s*(.+)$";
        var match = Regex.Match(yaml, pattern, RegexOptions.Multiline);
        if (!match.Success)
            return null;

        var value = match.Groups[1].Value.Trim();
        // Strip surrounding quotes
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
            value = value[1..^1];

        return value;
    }

    private static string InferCategory(string dirName)
    {
        if (dirName.StartsWith("mvux-")) return "mvux";
        if (dirName.StartsWith("uno-material-")) return "styling";
        if (dirName.StartsWith("uno-navigation-")) return "navigation";
        if (dirName.StartsWith("uno-toolkit-")) return "toolkit";
        if (dirName.StartsWith("uno-app-")) return "testing";
        if (dirName.StartsWith("winui-")) return "core";
        if (dirName.StartsWith("uno-csharp-")) return "core";
        if (dirName.StartsWith("uno-extensions-")) return "core";
        if (dirName.StartsWith("uno-migration-")) return "core";
        if (dirName.StartsWith("uno-platform-")) return "core";
        if (dirName.StartsWith("uno-wasm-")) return "other";
        return "other";
    }

    private static string? GetSkillsRoot()
    {
        // Look for ~/.claude/skills
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var claudeSkills = Path.Combine(userProfile, ".claude", "skills");
        if (Directory.Exists(claudeSkills))
            return claudeSkills;

        return null;
    }

    [GeneratedRegex(@"^---\s*\n(.*?)\n---", RegexOptions.Singleline)]
    private static partial Regex FrontmatterRegex();
}
