namespace GridForm.Models;

public record LineItem(
	string Sku,
	string Description,
	int Quantity,
	decimal UnitPrice)
{
	public decimal Total => Quantity * UnitPrice;
}
