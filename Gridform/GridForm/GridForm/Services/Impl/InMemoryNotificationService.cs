namespace GridForm.Services.Impl;

public class InMemoryNotificationService : INotificationService
{
	private readonly List<Notification> _notifications;

	public InMemoryNotificationService()
	{
		var now = DateTimeOffset.Now;
		_notifications =
		[
			new Notification("n1", "PO-7420 Flagged", "AI detected quarterly ceiling exceeded for Sandvik Coromant.", NotificationType.Warning, now.AddHours(-4)),
			new Notification("n2", "PO-7421 Pending", "New purchase order from Kennametal awaiting your review.", NotificationType.Info, now.AddHours(-3)),
			new Notification("n3", "PO-7417 Approved", "Final approval granted by VP Ops. Order sent to vendor.", NotificationType.Success, now.AddHours(-18)),
			new Notification("n4", "SLA Warning", "PO-7420 SLA deadline approaching — 1d 4h remaining.", NotificationType.Warning, now.AddHours(-2)),
			new Notification("n5", "Delivery Confirmed", "Walter Tools shipment for PO-7416 dispatched.", NotificationType.Success, now.AddDays(-1)),
		];
	}

	public ValueTask<ImmutableList<Notification>> GetNotifications(CancellationToken ct)
		=> ValueTask.FromResult(_notifications.ToImmutableList());

	public ValueTask MarkRead(string id, CancellationToken ct)
	{
		var idx = _notifications.FindIndex(n => n.Id == id);
		if (idx >= 0)
			_notifications[idx] = _notifications[idx] with { IsRead = true };
		return ValueTask.CompletedTask;
	}

	public ValueTask MarkAllRead(CancellationToken ct)
	{
		for (var i = 0; i < _notifications.Count; i++)
			_notifications[i] = _notifications[i] with { IsRead = true };
		return ValueTask.CompletedTask;
	}
}
