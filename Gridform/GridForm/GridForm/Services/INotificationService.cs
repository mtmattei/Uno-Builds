namespace GridForm.Services;

public interface INotificationService
{
	ValueTask<ImmutableList<Notification>> GetNotifications(CancellationToken ct = default);
	ValueTask MarkRead(string id, CancellationToken ct = default);
	ValueTask MarkAllRead(CancellationToken ct = default);
}
