namespace VoxelWarehouse.Models;

public enum POStatus { Pending, Review, Approved, Flagged }
public enum RiskLevel { Low, Med, High }

public partial record PurchaseOrder(
    string Id,
    string VendorName,
    string VendorRegion,
    decimal Amount,
    string BudgetCode,
    POStatus Status,
    RiskLevel Risk,
    string? AiAlertType,
    string? AiAlertText,
    string SubmittedAgo,
    string Approver,
    string Detail,
    string? AiBrief);

public record ActivityEntry(string TimeAgo, string Message, string Type);
