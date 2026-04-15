namespace VoxelWarehouse.Services;

public static class SeedDataService
{
    // Cached — seed data is immutable, no need to reallocate on every call
    private static readonly IReadOnlyList<PurchaseOrder> CachedOrders =
    [
        new PurchaseOrder(
            Id: "PO-2421",
            VendorName: "Kennametal Inc.",
            VendorRegion: "Latrobe, PA",
            Amount: 18_450.00m,
            BudgetCode: "Q2-TOOL-04",
            Status: POStatus.Pending,
            Risk: RiskLevel.Med,
            AiAlertType: "warn",
            AiAlertText: "+$12K vs Q1 avg",
            SubmittedAgo: "2h ago",
            Approver: "M. Torres",
            Detail: "CNMG 120408 carbide inserts (x200), DNMG 150608 turning inserts (x150), KC5010 grade",
            AiBrief: "Spend is 42% above Q1 quarterly average for carbide tooling. Recommend splitting across two budget periods or negotiating volume discount with Kennametal rep."
        ),
        new PurchaseOrder(
            Id: "PO-2422",
            VendorName: "Sandvik Coromant",
            VendorRegion: "Fair Lawn, NJ",
            Amount: 34_200.00m,
            BudgetCode: "Q2-MILL-01",
            Status: POStatus.Review,
            Risk: RiskLevel.High,
            AiAlertType: "alert",
            AiAlertText: "New vendor contract",
            SubmittedAgo: "4h ago",
            Approver: "D. Chen",
            Detail: "CoroMill 390 shoulder milling cutters (x8), R390-11T308M inserts (x400), CoroMill 316 exchangeable head system",
            AiBrief: "First order under new Sandvik framework agreement. Contract terms not yet validated by legal. High-value PO requires VP approval per policy. Lead time 6-8 weeks."
        ),
        new PurchaseOrder(
            Id: "PO-2423",
            VendorName: "Mitutoyo America",
            VendorRegion: "Aurora, IL",
            Amount: 8_750.00m,
            BudgetCode: "Q2-INSP-02",
            Status: POStatus.Approved,
            Risk: RiskLevel.Low,
            AiAlertType: null,
            AiAlertText: null,
            SubmittedAgo: "1d ago",
            Approver: "S. Park",
            Detail: "Absolute Digimatic calipers 500-196-30 (x12), Gauge block set Grade 0 (x2), SPC cable kit",
            AiBrief: "Routine replacement order. Pricing matches last 3 quarters. All items in stock at distributor."
        ),
        new PurchaseOrder(
            Id: "PO-2424",
            VendorName: "Haas Automation",
            VendorRegion: "Oxnard, CA",
            Amount: 52_800.00m,
            BudgetCode: "Q2-MAINT-03",
            Status: POStatus.Flagged,
            Risk: RiskLevel.High,
            AiAlertType: "alert",
            AiAlertText: "Budget ceiling exceeded",
            SubmittedAgo: "6h ago",
            Approver: "R. Nakamura",
            Detail: "VF-2SS spindle bearing assembly (x1), servo motor Y-axis (x2), 40-taper tool changer arm repair kit",
            AiBrief: "Critical machine down — VF-2SS spindle failure. Q2-MAINT budget has $31K remaining vs $52.8K request. Emergency escalation recommended. Alternative: refurbished spindle from MRO at $28K."
        ),
        new PurchaseOrder(
            Id: "PO-2425",
            VendorName: "MSC Industrial",
            VendorRegion: "Melville, NY",
            Amount: 4_320.00m,
            BudgetCode: "Q2-SAFE-01",
            Status: POStatus.Approved,
            Risk: RiskLevel.Low,
            AiAlertType: "info",
            AiAlertText: "Auto-reorder",
            SubmittedAgo: "3d ago",
            Approver: "S. Park",
            Detail: "Coolant concentrate Trim Sol (55gal x4), nitrile gloves (case x10), safety glasses Z87+ (x50)",
            AiBrief: "Triggered by inventory auto-reorder. Consumption rate normal. No action needed."
        ),
        new PurchaseOrder(
            Id: "PO-2426",
            VendorName: "Renishaw Inc.",
            VendorRegion: "Hoffman Estates, IL",
            Amount: 22_100.00m,
            BudgetCode: "Q2-TOOL-05",
            Status: POStatus.Pending,
            Risk: RiskLevel.Med,
            AiAlertType: "warn",
            AiAlertText: "Lead time 10 wks",
            SubmittedAgo: "5h ago",
            Approver: "M. Torres",
            Detail: "OMP400 high-accuracy touch probe (x2), TS27R tool setter (x1), probe stylus kit M2 (x6)",
            AiBrief: "Long lead time items — 10 week delivery. If approved today, arrival aligns with Q3 cell commissioning. Delay risks pushing install to Q4."
        ),
        new PurchaseOrder(
            Id: "PO-2427",
            VendorName: "Kennametal Inc.",
            VendorRegion: "Latrobe, PA",
            Amount: 6_890.00m,
            BudgetCode: "Q2-TOOL-04",
            Status: POStatus.Review,
            Risk: RiskLevel.Low,
            AiAlertType: "info",
            AiAlertText: "Duplicate vendor",
            SubmittedAgo: "8h ago",
            Approver: "D. Chen",
            Detail: "SECO Minimaster Plus end mills (x20), solid carbide drills 8mm (x30), boring bar A16R-SCLCR09",
            AiBrief: "Second Kennametal PO this week (see PO-2421). Consider consolidating for shipping savings. Combined order qualifies for 5% volume tier."
        ),
        new PurchaseOrder(
            Id: "PO-2428",
            VendorName: "Mitutoyo America",
            VendorRegion: "Aurora, IL",
            Amount: 41_500.00m,
            BudgetCode: "Q2-INSP-03",
            Status: POStatus.Pending,
            Risk: RiskLevel.High,
            AiAlertType: "alert",
            AiAlertText: "CapEx threshold",
            SubmittedAgo: "1h ago",
            Approver: "R. Nakamura",
            Detail: "Crysta-Apex S CMM 574 (x1), MCOSMOS software license, installation & calibration service",
            AiBrief: "Capital expenditure — exceeds $25K CapEx threshold. Requires CFO sign-off per purchasing policy. ROI analysis shows 14-month payback based on current inspection backlog."
        ),
    ];

    private static readonly IReadOnlyList<ActivityEntry> CachedFeed =
    [
        new ActivityEntry("12m ago", "PO-2428 submitted — Mitutoyo CMM ($41.5K)", "update"),
        new ActivityEntry("1h ago", "PO-2421 flagged — spend variance +42%", "warning"),
        new ActivityEntry("2h ago", "PO-2423 approved by S. Park", "approval"),
        new ActivityEntry("4h ago", "PO-2422 moved to Review — new vendor", "update"),
        new ActivityEntry("5h ago", "PO-2426 submitted — Renishaw probes", "update"),
        new ActivityEntry("6h ago", "PO-2424 flagged — budget ceiling breach", "alert"),
        new ActivityEntry("8h ago", "PO-2427 duplicate vendor detected", "warning"),
        new ActivityEntry("1d ago", "PO-2425 auto-approved — MSC reorder", "approval"),
        new ActivityEntry("1d ago", "Warehouse floor utilization crossed 65%", "warning"),
        new ActivityEntry("2d ago", "Q2 budget allocations finalized", "update"),
    ];

    public static IReadOnlyList<PurchaseOrder> GetPurchaseOrders() => CachedOrders;
    public static IReadOnlyList<ActivityEntry> GetActivityFeed() => CachedFeed;
}
