using Uno.Extensions.Navigation;

namespace GridForm.Presentation.Orders;

public partial record OrdersModel(IProcurementService Procurement, INavigator Navigator)
{
	public IListFeed<PurchaseOrder> Orders =>
		ListFeed.Async(Procurement.GetOrders);

	public IState<ImmutableList<string>> SelectedIds =>
		State<ImmutableList<string>>.Value(this, () => ImmutableList<string>.Empty);

	public IFeed<int> SelectedCount =>
		Feed.Async(async ct =>
		{
			var ids = await SelectedIds.Value(ct);
			return ids?.Count ?? 0;
		});

	public IFeed<int> PendingCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.Pending));

	public IFeed<int> ReviewCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.InReview));

	public IFeed<int> ApprovedCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.Approved));

	public IFeed<int> FlaggedCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.Flagged));

	public async ValueTask NavigateToDetail(PurchaseOrder po)
	{
		await Navigator.NavigateRouteAsync(this, "OrderDetail", data: po);
	}

	public async ValueTask ToggleSelection(string poId)
	{
		await SelectedIds.UpdateAsync(ids =>
		{
			ids ??= ImmutableList<string>.Empty;
			return ids.Contains(poId) ? ids.Remove(poId) : ids.Add(poId);
		});
	}

	public async ValueTask BulkApprove()
	{
		var ids = await SelectedIds.Value(CancellationToken.None);
		if (ids is { Count: > 0 })
		{
			foreach (var id in ids)
				await Procurement.Approve(id);
			await SelectedIds.UpdateAsync(_ => ImmutableList<string>.Empty);
		}
	}

	public async ValueTask ClearSelection()
	{
		await SelectedIds.UpdateAsync(_ => ImmutableList<string>.Empty);
	}
}
