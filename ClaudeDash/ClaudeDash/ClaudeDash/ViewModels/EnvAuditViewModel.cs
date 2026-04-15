using System.Collections.Immutable;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record EnvAuditModel
{
    private readonly IClaudeConfigService _configService;
    private readonly ILogger<EnvAuditModel> _logger;

    public EnvAuditModel(
        IClaudeConfigService configService,
        ILogger<EnvAuditModel> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public IState<string> ErrorMessage => State.Value(this, () => string.Empty);

    public IListFeed<EnvCheckResult> Results => ListFeed.Async(async ct =>
    {
        try
        {
            var results = await _configService.RunEnvAuditAsync();
            return results.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to run environment audit");
            await ErrorMessage.Set(ex.Message, ct);
            return ImmutableList<EnvCheckResult>.Empty;
        }
    });

    public IFeed<int> PassedCount => Feed.Async(async ct =>
    {
        var results = await Results;
        return results?.Count(r => r.Status == "ok") ?? 0;
    });

    public IFeed<int> FailedCount => Feed.Async(async ct =>
    {
        var results = await Results;
        return results?.Count(r => r.Status != "ok") ?? 0;
    });
}
