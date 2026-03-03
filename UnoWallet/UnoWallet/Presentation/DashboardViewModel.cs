using UnoWallet.Models;

namespace UnoWallet.Presentation;

public partial class DashboardViewModel : ObservableObject
{
    public DashboardViewModel()
    {
        // Initialize with mock data
        TotalBalanceFormatted = MockData.TotalBalance.ToString("N2");
        NextPaymentFormatted = MockData.NextPayment.ToString("N2");
        CompletedPaymentsFormatted = MockData.CompletedPayments.ToString("N2");
        TodaySpentFormatted = $"${MockData.TodaySpent:N2}";
        DailyLimitFormatted = $"${MockData.DailyLimit:N2}";
        NotificationCount = MockData.NotificationCount;
        TransactionGroups = MockData.GroupedTransactions;

        // Calculate progress bar width (percentage of daily limit spent)
        var spentPercentage = (double)(MockData.TodaySpent / MockData.DailyLimit);
        // Minimum width to show the indicator, scale to reasonable max
        SpentBarWidth = Math.Max(20, spentPercentage * 300);
    }

    public string TotalBalanceFormatted { get; }
    public string NextPaymentFormatted { get; }
    public string CompletedPaymentsFormatted { get; }
    public string TodaySpentFormatted { get; }
    public string DailyLimitFormatted { get; }
    public int NotificationCount { get; }
    public double SpentBarWidth { get; }
    public IReadOnlyList<TransactionGroup> TransactionGroups { get; }
}
