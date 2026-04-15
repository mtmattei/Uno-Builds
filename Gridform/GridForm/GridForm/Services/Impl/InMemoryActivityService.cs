namespace GridForm.Services.Impl;

public class InMemoryActivityService : IActivityService
{
	public ValueTask<ImmutableList<ActivityEvent>> GetActivity(CancellationToken ct)
	{
		var now = DateTimeOffset.Now;
		var events = ImmutableList.Create(
			new ActivityEvent(now.AddMinutes(-8), "Shipment #4481 received — Sandvik, 4 pallets at Bay 2", ActivityEventType.Delivery),
			new ActivityEvent(now.AddMinutes(-23), "PO-7418 auto-approved — Haas, within policy", ActivityEventType.Approval),
			new ActivityEvent(now.AddHours(-1), "AI flagged PO-7420 — vendor ceiling exceeded +$12K", ActivityEventType.System),
			new ActivityEvent(now.AddHours(-2), "Zone B3 rebalanced — 3 pallets → staging", ActivityEventType.Delivery),
			new ActivityEvent(now.AddHours(-4), "Walter Tools onboarded — initial risk: B", ActivityEventType.Submission),
			new ActivityEvent(now.AddHours(-6), "Q2 budget alert — 87% committed", ActivityEventType.Escalation),
			new ActivityEvent(now.AddHours(-8), "PO-7415 shipped — Haas consumables, tracking #HAS-44182", ActivityEventType.Approval)
		);
		return ValueTask.FromResult(events);
	}
}
