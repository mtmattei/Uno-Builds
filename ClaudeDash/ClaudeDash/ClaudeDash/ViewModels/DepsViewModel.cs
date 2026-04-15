using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record DepsModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<DepsModel> _logger;

    public DepsModel(
        IClaudeConfigService configService,
        ILogger<DepsModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListFeed<DependencyInfo> Dependencies => ListFeed.Async(async ct =>
    {
        try
        {
            var deps = await _configService.GetDependenciesAsync();
            return deps.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load dependencies");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<DependencyInfo>.Empty;
        }
    });

    public IFeed<int> TotalPackages => Feed.Async(async ct =>
    {
        var deps = await Dependencies;
        return deps?.Select(d => d.PackageName).Distinct().Count() ?? 0;
    });

    public IFeed<int> ProjectCount => Feed.Async(async ct =>
    {
        var deps = await Dependencies;
        return deps?.Select(d => d.ProjectName).Distinct().Count() ?? 0;
    });
}
