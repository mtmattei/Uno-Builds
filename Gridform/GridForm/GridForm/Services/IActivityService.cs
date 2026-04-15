namespace GridForm.Services;

public interface IActivityService
{
	ValueTask<ImmutableList<ActivityEvent>> GetActivity(CancellationToken ct = default);
}
