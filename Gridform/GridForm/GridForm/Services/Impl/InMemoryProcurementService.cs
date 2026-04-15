namespace GridForm.Services.Impl;

public class InMemoryProcurementService : IProcurementService
{
	private readonly List<PurchaseOrder> _orders;

	public InMemoryProcurementService()
	{
		var now = DateTimeOffset.Now;
		_orders =
		[
			new PurchaseOrder(
				Id: "PO-7421",
				VendorName: "Kennametal Inc.",
				VendorLocation: "Pittsburgh, PA",
				Category: "Cutting Tools",
				RequestedBy: "Mike Torres",
				RequestedByRole: "Manufacturing",
				Amount: 48_200m,
				Status: OrderStatus.Pending,
				Risk: RiskLevel.Low,
				CreatedAt: now.AddHours(-3),
				SlaDeadline: now.AddDays(2).AddHours(4),
				AiNote: null,
				AiNoteType: null,
				OnTimePercent: 97,
				QualityGrade: "A",
				YtdOrders: 23,
				ContractStatus: "Active",
				PaymentTerms: "Net 30",
				ShipDate: now.AddDays(12),
				Items: [
					new LineItem("KMT-2040", "CoroMill 390 shoulder milling cutter", 4, 3_200m),
					new LineItem("KMT-2041", "CoroMill 490 face milling cutter", 6, 2_800m),
					new LineItem("KMT-3010", "Carbide insert pack (100pc)", 10, 1_540m),
				],
				ApprovalChain: [
					new ApprovalStep("Mike Torres", "Requester", ApprovalStatus.Done, now.AddHours(-3)),
					new ApprovalStep("You", "Manager Review", ApprovalStatus.Current),
					new ApprovalStep("Finance", "Budget Check", ApprovalStatus.Waiting),
				],
				History: [
					new AuditEntry(now.AddHours(-3), "PO created by Mike Torres"),
					new AuditEntry(now.AddHours(-2.5), "Auto-routed to manager review"),
				]),

			new PurchaseOrder(
				Id: "PO-7420",
				VendorName: "Sandvik Coromant",
				VendorLocation: "Fair Lawn, NJ",
				Category: "Milling Tools",
				RequestedBy: "Lisa Park",
				RequestedByRole: "Production",
				Amount: 67_800m,
				Status: OrderStatus.Flagged,
				Risk: RiskLevel.High,
				CreatedAt: now.AddHours(-5),
				SlaDeadline: now.AddDays(1).AddHours(4),
				AiNote: "This PO + PO-7417 = $122K from Sandvik this quarter vs $110K ceiling. Recommend splitting across vendors or requesting ceiling increase.",
				AiNoteType: "warning",
				OnTimePercent: 94,
				QualityGrade: "A-",
				YtdOrders: 18,
				ContractStatus: "Active",
				PaymentTerms: "Net 45",
				ShipDate: now.AddDays(8),
				Items: [
					new LineItem("SVC-5501", "CoroDrill 860 drill bit set", 12, 1_850m),
					new LineItem("SVC-5502", "CoroMill Plura end mill", 8, 3_400m),
					new LineItem("SVC-5503", "T-Max P turning insert kit", 20, 680m),
				],
				ApprovalChain: [
					new ApprovalStep("Lisa Park", "Requester", ApprovalStatus.Done, now.AddHours(-5)),
					new ApprovalStep("You", "Manager Review", ApprovalStatus.Current),
					new ApprovalStep("Finance", "Budget Check", ApprovalStatus.Waiting),
					new ApprovalStep("VP Ops", "Final (>$25K)", ApprovalStatus.Waiting),
				],
				History: [
					new AuditEntry(now.AddHours(-5), "PO created by Lisa Park"),
					new AuditEntry(now.AddHours(-4.8), "AI flagged: quarterly ceiling exceeded"),
					new AuditEntry(now.AddHours(-4.5), "Auto-routed to manager review"),
				]),

			new PurchaseOrder(
				Id: "PO-7419",
				VendorName: "Iscar Ltd.",
				VendorLocation: "Arlington, TX",
				Category: "Turning Tools",
				RequestedBy: "Chen Wei",
				RequestedByRole: "Engineering",
				Amount: 31_500m,
				Status: OrderStatus.InReview,
				Risk: RiskLevel.Medium,
				CreatedAt: now.AddHours(-8),
				SlaDeadline: now.AddDays(3),
				AiNote: "New vendor relationship. First order — verify payment terms and delivery commitments.",
				AiNoteType: "info",
				OnTimePercent: 89,
				QualityGrade: "B+",
				YtdOrders: 3,
				ContractStatus: "Provisional",
				PaymentTerms: "Net 30",
				ShipDate: now.AddDays(15),
				Items: [
					new LineItem("ISC-1010", "TANGGRIP parting blade", 20, 425m),
					new LineItem("ISC-1011", "HELIMILL indexable cutter", 6, 3_750m),
				],
				ApprovalChain: [
					new ApprovalStep("Chen Wei", "Requester", ApprovalStatus.Done, now.AddHours(-8)),
					new ApprovalStep("Sarah Kim", "Lead Engineer", ApprovalStatus.Done, now.AddHours(-6)),
					new ApprovalStep("Finance", "Budget Check", ApprovalStatus.Current),
				],
				History: [
					new AuditEntry(now.AddHours(-8), "PO created by Chen Wei"),
					new AuditEntry(now.AddHours(-6), "Approved by Sarah Kim (Lead Engineer)"),
					new AuditEntry(now.AddHours(-5), "Forwarded to Finance for budget review"),
				]),

			new PurchaseOrder(
				Id: "PO-7418",
				VendorName: "Mitsubishi Materials",
				VendorLocation: "Schaumburg, IL",
				Category: "Drilling Tools",
				RequestedBy: "Tom Bradley",
				RequestedByRole: "Maintenance",
				Amount: 22_100m,
				Status: OrderStatus.InReview,
				Risk: RiskLevel.Low,
				CreatedAt: now.AddHours(-12),
				SlaDeadline: now.AddDays(4),
				AiNote: null,
				AiNoteType: null,
				OnTimePercent: 96,
				QualityGrade: "A",
				YtdOrders: 11,
				ContractStatus: "Active",
				PaymentTerms: "Net 30",
				ShipDate: now.AddDays(10),
				Items: [
					new LineItem("MIT-3301", "MVX drill body 25mm", 3, 4_200m),
					new LineItem("MIT-3302", "Carbide drill insert SOMX", 30, 320m),
				],
				ApprovalChain: [
					new ApprovalStep("Tom Bradley", "Requester", ApprovalStatus.Done, now.AddHours(-12)),
					new ApprovalStep("Dept Head", "Manager Review", ApprovalStatus.Done, now.AddHours(-10)),
					new ApprovalStep("Finance", "Budget Check", ApprovalStatus.Current),
				],
				History: [
					new AuditEntry(now.AddHours(-12), "PO created by Tom Bradley"),
					new AuditEntry(now.AddHours(-10), "Approved by department head"),
				]),

			new PurchaseOrder(
				Id: "PO-7417",
				VendorName: "Sandvik Coromant",
				VendorLocation: "Fair Lawn, NJ",
				Category: "Boring Tools",
				RequestedBy: "Lisa Park",
				RequestedByRole: "Production",
				Amount: 54_200m,
				Status: OrderStatus.Approved,
				Risk: RiskLevel.Low,
				CreatedAt: now.AddDays(-2),
				SlaDeadline: null,
				AiNote: null,
				AiNoteType: null,
				OnTimePercent: 94,
				QualityGrade: "A-",
				YtdOrders: 18,
				ContractStatus: "Active",
				PaymentTerms: "Net 45",
				ShipDate: now.AddDays(5),
				Items: [
					new LineItem("SVC-7701", "CoroBore 825 fine boring tool", 2, 12_600m),
					new LineItem("SVC-7702", "Silent Tools damped adapter", 4, 7_250m),
				],
				ApprovalChain: [
					new ApprovalStep("Lisa Park", "Requester", ApprovalStatus.Done, now.AddDays(-2)),
					new ApprovalStep("James Chen", "Manager Review", ApprovalStatus.Done, now.AddDays(-1.5)),
					new ApprovalStep("Finance", "Budget Check", ApprovalStatus.Done, now.AddDays(-1)),
					new ApprovalStep("VP Ops", "Final (>$25K)", ApprovalStatus.Done, now.AddHours(-18)),
				],
				History: [
					new AuditEntry(now.AddDays(-2), "PO created by Lisa Park"),
					new AuditEntry(now.AddDays(-1.5), "Approved by James Chen"),
					new AuditEntry(now.AddDays(-1), "Budget approved by Finance"),
					new AuditEntry(now.AddHours(-18), "Final approval by VP Ops"),
				]),

			new PurchaseOrder(
				Id: "PO-7416",
				VendorName: "Walter Tools",
				VendorLocation: "Waukesha, WI",
				Category: "Threading Tools",
				RequestedBy: "Ana Rivera",
				RequestedByRole: "Production",
				Amount: 18_900m,
				Status: OrderStatus.Approved,
				Risk: RiskLevel.Low,
				CreatedAt: now.AddDays(-3),
				SlaDeadline: null,
				AiNote: null,
				AiNoteType: null,
				OnTimePercent: 92,
				QualityGrade: "B+",
				YtdOrders: 8,
				ContractStatus: "Active",
				PaymentTerms: "Net 30",
				ShipDate: now.AddDays(3),
				Items: [
					new LineItem("WLT-4401", "Prototyp thread mill TC115", 10, 890m),
					new LineItem("WLT-4402", "Walter Titex X-treme drill", 8, 1_125m),
				],
				ApprovalChain: [
					new ApprovalStep("Ana Rivera", "Requester", ApprovalStatus.Done, now.AddDays(-3)),
					new ApprovalStep("James Chen", "Manager Review", ApprovalStatus.Done, now.AddDays(-2.5)),
					new ApprovalStep("Finance", "Budget Check", ApprovalStatus.Done, now.AddDays(-2)),
				],
				History: [
					new AuditEntry(now.AddDays(-3), "PO created by Ana Rivera"),
					new AuditEntry(now.AddDays(-2.5), "Approved by James Chen"),
					new AuditEntry(now.AddDays(-2), "Budget approved by Finance"),
				]),

			new PurchaseOrder(
				Id: "PO-7415",
				VendorName: "Seco Tools",
				VendorLocation: "Troy, MI",
				Category: "Milling Inserts",
				RequestedBy: "David Kim",
				RequestedByRole: "Manufacturing",
				Amount: 12_400m,
				Status: OrderStatus.Pending,
				Risk: RiskLevel.Low,
				CreatedAt: now.AddHours(-1),
				SlaDeadline: now.AddDays(3),
				AiNote: null,
				AiNoteType: null,
				OnTimePercent: 91,
				QualityGrade: "B+",
				YtdOrders: 6,
				ContractStatus: "Active",
				PaymentTerms: "Net 30",
				ShipDate: now.AddDays(14),
				Items: [
					new LineItem("SEC-2201", "Square shoulder insert XOMX", 50, 148m),
					new LineItem("SEC-2202", "Face mill insert SEEX", 30, 128m),
				],
				ApprovalChain: [
					new ApprovalStep("David Kim", "Requester", ApprovalStatus.Done, now.AddHours(-1)),
					new ApprovalStep("You", "Manager Review", ApprovalStatus.Current),
				],
				History: [
					new AuditEntry(now.AddHours(-1), "PO created by David Kim"),
					new AuditEntry(now.AddMinutes(-50), "Auto-routed to manager review"),
				]),
		];
	}

	public ValueTask<ImmutableList<PurchaseOrder>> GetOrders(CancellationToken ct)
		=> ValueTask.FromResult(_orders.ToImmutableList());

	public ValueTask<PurchaseOrder?> GetOrder(string id, CancellationToken ct)
		=> ValueTask.FromResult(_orders.FirstOrDefault(o => o.Id == id));

	public ValueTask Approve(string id, CancellationToken ct)
	{
		var idx = _orders.FindIndex(o => o.Id == id);
		if (idx >= 0)
			_orders[idx] = _orders[idx] with { Status = OrderStatus.Approved };
		return ValueTask.CompletedTask;
	}

	public ValueTask Reject(string id, CancellationToken ct)
	{
		var idx = _orders.FindIndex(o => o.Id == id);
		if (idx >= 0)
			_orders[idx] = _orders[idx] with { Status = OrderStatus.Flagged };
		return ValueTask.CompletedTask;
	}

	public ValueTask Escalate(string id, CancellationToken ct)
		=> ValueTask.CompletedTask;
}
