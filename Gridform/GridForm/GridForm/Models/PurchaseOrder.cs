namespace GridForm.Models;

public partial record PurchaseOrder(
	string Id,
	string VendorName,
	string VendorLocation,
	string Category,
	string RequestedBy,
	string RequestedByRole,
	decimal Amount,
	OrderStatus Status,
	RiskLevel Risk,
	DateTimeOffset CreatedAt,
	DateTimeOffset? SlaDeadline,
	string? AiNote,
	string? AiNoteType,
	double OnTimePercent,
	string QualityGrade,
	int YtdOrders,
	string ContractStatus,
	string PaymentTerms,
	DateTimeOffset? ShipDate,
	ImmutableList<LineItem> Items,
	ImmutableList<ApprovalStep> ApprovalChain,
	ImmutableList<AuditEntry> History);

public enum OrderStatus
{
	Pending,
	InReview,
	Approved,
	Flagged
}

public enum RiskLevel
{
	Low,
	Medium,
	High
}
