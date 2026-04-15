using Uno.Extensions.Navigation;

namespace GridForm.Presentation.Orders;

public partial record OrderDetailModel(PurchaseOrder Order, IProcurementService Procurement, INavigator Navigator)
{
	public IState<string> ActiveTab => State<string>.Value(this, () => "overview");

	public async ValueTask Approve()
	{
		await Procurement.Approve(Order.Id);
		await Navigator.NavigateBackAsync(this);
	}

	public async ValueTask Reject()
	{
		await Procurement.Reject(Order.Id);
		await Navigator.NavigateBackAsync(this);
	}

	public async ValueTask Escalate()
	{
		await Procurement.Escalate(Order.Id);
	}

	public async ValueTask GoBack()
	{
		await Navigator.NavigateBackAsync(this);
	}
}
