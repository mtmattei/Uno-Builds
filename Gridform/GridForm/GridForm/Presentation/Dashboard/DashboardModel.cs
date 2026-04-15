namespace GridForm.Presentation.Dashboard;

public partial record DashboardModel(
	IProcurementService Procurement,
	IActivityService Activity)
{
	public IListFeed<PurchaseOrder> PipelineOrders =>
		ListFeed.Async(Procurement.GetOrders);

	public IListFeed<ActivityEvent> ActivityEvents =>
		ListFeed.Async(Activity.GetActivity);

	public IFeed<int> PendingCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.Pending));

	public IFeed<int> ReviewCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.InReview));

	public IFeed<int> ApprovedCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.Approved));

	public IFeed<int> FlaggedCount =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count(o => o.Status == OrderStatus.Flagged));

	public IFeed<string> TotalValue =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Sum(o => o.Amount).ToString("C0"));

	public IFeed<int> TotalOrders =>
		Feed.Async(async ct => (await Procurement.GetOrders(ct)).Count);
}
