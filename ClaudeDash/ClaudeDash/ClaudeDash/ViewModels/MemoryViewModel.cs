using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record MemoryModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<MemoryModel> _logger;

    public MemoryModel(
        IClaudeConfigService configService,
        ILogger<MemoryModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListFeed<MemoryFile> MemoryFiles => ListFeed.Async(async ct =>
    {
        try
        {
            var files = await _configService.GetMemoryFilesAsync();
            return files.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load memory files");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<MemoryFile>.Empty;
        }
    });

    public IFeed<int> TotalFiles => Feed.Async(async ct =>
    {
        var files = await MemoryFiles;
        return files?.Count ?? 0;
    });

    public IFeed<int> ProjectCount => Feed.Async(async ct =>
    {
        var files = await MemoryFiles;
        return files?.Select(f => f.ProjectContext).Distinct().Count() ?? 0;
    });
}
