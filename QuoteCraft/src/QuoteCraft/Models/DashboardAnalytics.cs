namespace QuoteCraft.Models;

public partial record DashboardAnalytics(
    decimal TotalQuotedThisMonth,
    int QuotesSentThisMonth,
    double AcceptanceRate);
