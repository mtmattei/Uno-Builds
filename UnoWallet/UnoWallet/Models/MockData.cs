namespace UnoWallet.Models;

public static class MockData
{
    public static decimal TotalBalance => 248967.83m;
    public static decimal NextPayment => 43093.00m;
    public static decimal CompletedPayments => 274825.01m;
    public static decimal TodaySpent => 614.93m;
    public static decimal DailyLimit => 43093.00m;
    public static int NotificationCount => 4;

    public static IReadOnlyList<Transaction> Transactions { get; } =
    [
        new("Amazon.com", "ms-appx:///Assets/Images/amazon.png", 89.71m, new DateTime(2025, 11, 18, 9, 17, 0)),
        new("Temu.com", "ms-appx:///Assets/Images/temu.png", 30.45m, new DateTime(2025, 11, 18, 8, 49, 0)),
        new("Amazon.com", "ms-appx:///Assets/Images/amazon.png", 261.92m, new DateTime(2025, 11, 17, 8, 33, 0)),
    ];

    public static IReadOnlyList<TransactionGroup> GroupedTransactions { get; } =
    [
        new("Today",
        [
            new("Amazon.com", "ms-appx:///Assets/Images/amazon.png", 89.71m, new DateTime(2025, 11, 18, 9, 17, 0)),
            new("Temu.com", "ms-appx:///Assets/Images/temu.png", 30.45m, new DateTime(2025, 11, 18, 8, 49, 0)),
        ]),
        new("Yesterday",
        [
            new("Amazon.com", "ms-appx:///Assets/Images/amazon.png", 261.92m, new DateTime(2025, 11, 17, 8, 33, 0)),
        ]),
    ];
}

public record Transaction(
    string MerchantName,
    string LogoSource,
    decimal Amount,
    DateTime Timestamp
)
{
    public string FormattedAmount => $"${Amount:N2}";
    public string FormattedDate => Timestamp.ToString("MMM d, yyyy");
    public string FormattedTime => Timestamp.ToString("h:mm tt");
}

public record TransactionGroup(
    string DateLabel,
    IReadOnlyList<Transaction> Transactions
);

// Analytics Page Data
public static class AnalyticsMockData
{
    public static decimal TotalSpending => 248967.83m;
    public static decimal OnProgress => 61523.00m;
    public static decimal Overdue => 4825.43m;
    public static decimal TotalInstallments => 89271.92m;

    // Chart data points for November 2025
    public static IReadOnlyList<ChartDataPoint> ChartData { get; } =
    [
        new(new DateTime(2025, 11, 1), 1200.00m),
        new(new DateTime(2025, 11, 5), 1850.00m),
        new(new DateTime(2025, 11, 10), 2100.00m),
        new(new DateTime(2025, 11, 15), 2800.00m),
        new(new DateTime(2025, 11, 20), 3200.00m),
        new(new DateTime(2025, 11, 25), 4274.00m),
        new(new DateTime(2025, 11, 30), 3900.00m),
    ];

    public static IReadOnlyList<Installment> FourInstallments { get; } =
    [
        new("PS5", "Amazon.com", "ms-appx:///Assets/Images/ps5.png", 836.94m, 18, 1, 4),
        new("Nikon Camera", "Amazon.com", "ms-appx:///Assets/Images/nikon.png", 563.04m, 18, 1, 4),
        new("Gaming Laptop", "Amazon.com", "ms-appx:///Assets/Images/laptop.png", 1746.94m, 18, 3, 4),
    ];

    public static IReadOnlyList<Installment> SixInstallments { get; } =
    [
        new("MacBook Pro", "Apple.com", "ms-appx:///Assets/Images/laptop.png", 2499.00m, 22, 2, 6),
        new("Sony TV", "BestBuy.com", "ms-appx:///Assets/Images/ps5.png", 1299.00m, 15, 4, 6),
    ];
}

public record ChartDataPoint(DateTime Date, decimal Amount)
{
    public string FormattedAmount => $"${Amount:N2}";
    public string FormattedDate => Date.ToString("MMM d, yyyy");
}

public record Installment(
    string ProductName,
    string MerchantName,
    string ImageSource,
    decimal Price,
    int DueDate,
    int CurrentInstallment,
    int TotalInstallments
)
{
    public string FormattedPrice => $"${Price:N2}";
    public string DueDateText => $"Due date {DueDate}";
    public string ProgressText => $"{CurrentInstallment} of {TotalInstallments} Installment";
}

// Card Page Data
public static class CardMockData
{
    public static string CardType => "Master Card";
    public static string CardNumber => "828749-2847-03";
    public static decimal CardLimit => 43093.00m;
    public static string LimitLabel => "Limit Card";

    public static IReadOnlyList<Installment> UpcomingPayments { get; } =
    [
        new("PS5", "Amazon.com", "ms-appx:///Assets/Images/ps5.png", 836.94m, 18, 1, 4),
        new("Nikon Camera", "Amazon.com", "ms-appx:///Assets/Images/nikon.png", 563.04m, 18, 3, 4),
        new("Gaming Laptop", "Amazon.com", "ms-appx:///Assets/Images/laptop.png", 1746.94m, 18, 2, 4),
    ];
}
