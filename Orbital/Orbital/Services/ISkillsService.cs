namespace Orbital.Services;

public interface ISkillsService
{
    ValueTask<ImmutableList<SkillInfo>> GetSkillsAsync(CancellationToken ct);
    ValueTask ToggleSkillAsync(string skillId, bool active, CancellationToken ct);
}
