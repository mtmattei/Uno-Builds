using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record SkillsModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<SkillsModel> _logger;

    public SkillsModel(
        IClaudeConfigService configService,
        ILogger<SkillsModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    private IFeed<ImmutableList<SkillInfo>> AllSkills => Feed.Async(async ct =>
    {
        try
        {
            var skills = await _configService.GetSkillsAsync();
            var allSkills = EnrichSkills(skills);
            return allSkills.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load skills");
            await ErrorMessage.Set($"Failed to load skills: {ex.Message}", ct);
            return ImmutableList<SkillInfo>.Empty;
        }
    });

    public IListFeed<SkillInfo> CoreSkills => ListFeed.Async(async ct =>
    {
        var all = await AllSkills;
        return all?.Where(s => s.Category != "user" && s.Category != "example").ToImmutableList() ?? ImmutableList<SkillInfo>.Empty;
    });

    public IListFeed<SkillInfo> UserSkills => ListFeed.Async(async ct =>
    {
        var all = await AllSkills;
        return all?.Where(s => s.Category == "user").ToImmutableList() ?? ImmutableList<SkillInfo>.Empty;
    });

    public IListFeed<SkillInfo> ExampleSkills => ListFeed.Async(async ct =>
    {
        var all = await AllSkills;
        return all?.Where(s => s.Category == "example").ToImmutableList() ?? ImmutableList<SkillInfo>.Empty;
    });

    public IFeed<int> TotalSkills => Feed.Async(async ct =>
    {
        var all = await AllSkills;
        return all?.Count ?? 0;
    });

    public IFeed<int> ActiveCount => Feed.Async(async ct =>
    {
        var all = await AllSkills;
        return all?.Count(s => s.IsActive) ?? 0;
    });

    public IFeed<int> UserDefinedCount => Feed.Async(async ct =>
    {
        var all = await AllSkills;
        return all?.Count(s => s.Category == "user") ?? 0;
    });

    public IFeed<int> TotalInvocations => Feed.Async(async ct =>
    {
        var all = await AllSkills;
        return all?.Sum(s => s.Invocations) ?? 0;
    });

    public IFeed<double> AvgAccuracy => Feed.Async(async ct =>
    {
        var all = await AllSkills;
        return all is { Count: > 0 }
            ? Math.Round(all.Average(s => s.Accuracy), 1)
            : 0.0;
    });

    private static List<SkillInfo> EnrichSkills(List<SkillInfo> existing)
    {
        var mockSkills = new List<SkillInfo>
        {
            // Core skills
            new()
            {
                Name = "docx", Category = "core",
                Path = "/mnt/skills/public/docx/SKILL.md",
                Description = "Create, read, edit, and manipulate Word documents with full formatting support.",
                IsActive = true, Invocations = 89, Accuracy = 96.1, AvgReadTime = "1.2s"
            },
            new()
            {
                Name = "pptx", Category = "core",
                Path = "/mnt/skills/public/pptx/SKILL.md",
                Description = "Generate and modify PowerPoint presentations with slides, layouts, and themes.",
                IsActive = true, Invocations = 45, Accuracy = 93.8, AvgReadTime = "1.8s"
            },
            new()
            {
                Name = "xlsx", Category = "core",
                Path = "/mnt/skills/public/xlsx/SKILL.md",
                Description = "Read, write, and transform Excel spreadsheets with formulas and charts.",
                IsActive = true, Invocations = 67, Accuracy = 97.2, AvgReadTime = "1.4s"
            },
            new()
            {
                Name = "pdf", Category = "core",
                Path = "/mnt/skills/public/pdf/SKILL.md",
                Description = "Parse, extract, and generate PDF documents with text and image support.",
                IsActive = true, Invocations = 52, Accuracy = 91.5, AvgReadTime = "2.1s"
            },
            new()
            {
                Name = "frontend-design", Category = "core",
                Path = "/mnt/skills/public/frontend-design/SKILL.md",
                Description = "Generate responsive UI components with modern CSS and accessibility standards.",
                IsActive = true, Invocations = 34, Accuracy = 94.7, AvgReadTime = "2.8s"
            },
            new()
            {
                Name = "product-self-knowledge", Category = "core",
                Path = "/mnt/skills/public/product-self-knowledge/SKILL.md",
                Description = "Query and reason about product documentation, specs, and internal knowledge bases.",
                IsActive = false, Invocations = 21, Accuracy = 88.3, AvgReadTime = "3.4s"
            },
            new()
            {
                Name = "csv-analysis", Category = "core",
                Path = "/mnt/skills/public/csv-analysis/SKILL.md",
                Description = "Parse, filter, aggregate, and visualize CSV data with statistical operations.",
                IsActive = true, Invocations = 18, Accuracy = 95.0, AvgReadTime = "0.9s"
            },

            // User skills
            new()
            {
                Name = "uno-platform-components", Category = "user",
                Path = "/mnt/skills/user/uno-platform-components/SKILL.md",
                Description = "Scaffold and configure Uno Platform UI components with XAML and C# code-behind.",
                IsActive = true, Invocations = 28, Accuracy = 92.4, AvgReadTime = "2.3s"
            },
            new()
            {
                Name = "blog-post-writer", Category = "user",
                Path = "/mnt/skills/user/blog-post-writer/SKILL.md",
                Description = "Draft, edit, and format blog posts with SEO metadata and image placement.",
                IsActive = true, Invocations = 15, Accuracy = 87.9, AvgReadTime = "1.6s"
            },

            // Example skills
            new()
            {
                Name = "api-integration", Category = "example",
                Path = "/mnt/skills/examples/api-integration/SKILL.md",
                Description = "Generate REST/GraphQL client code with authentication and error handling patterns.",
                IsActive = false, Invocations = 12, Accuracy = 90.1, AvgReadTime = "1.9s"
            },
            new()
            {
                Name = "data-pipeline", Category = "example",
                Path = "/mnt/skills/examples/data-pipeline/SKILL.md",
                Description = "Build ETL workflows with validation, transformation, and output staging steps.",
                IsActive = false, Invocations = 14, Accuracy = 88.6, AvgReadTime = "2.7s"
            }
        };

        var result = new List<SkillInfo>();
        var addedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var existingSkill in existing)
        {
            var mock = mockSkills.FirstOrDefault(m =>
                m.Name.Equals(existingSkill.Name, StringComparison.OrdinalIgnoreCase));

            if (mock != null)
            {
                existingSkill.Description = mock.Description;
                existingSkill.IsActive = mock.IsActive;
                existingSkill.Invocations = mock.Invocations;
                existingSkill.Accuracy = mock.Accuracy;
                existingSkill.AvgReadTime = mock.AvgReadTime;
                existingSkill.Category = mock.Category;
            }
            else
            {
                existingSkill.IsActive = true;
                existingSkill.Invocations = 5;
                existingSkill.Accuracy = 90.0;
                existingSkill.AvgReadTime = "1.5s";
                existingSkill.Category = "core";
            }

            result.Add(existingSkill);
            addedNames.Add(existingSkill.Name);
        }

        foreach (var mock in mockSkills)
        {
            if (!addedNames.Contains(mock.Name))
            {
                result.Add(mock);
            }
        }

        return result;
    }
}
