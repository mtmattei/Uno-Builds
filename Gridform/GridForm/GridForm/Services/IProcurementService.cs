namespace GridForm.Services;

public interface IProcurementService
{
	ValueTask<ImmutableList<PurchaseOrder>> GetOrders(CancellationToken ct = default);
	ValueTask<PurchaseOrder?> GetOrder(string id, CancellationToken ct = default);
	ValueTask Approve(string id, CancellationToken ct = default);
	ValueTask Reject(string id, CancellationToken ct = default);
	ValueTask Escalate(string id, CancellationToken ct = default);
}
