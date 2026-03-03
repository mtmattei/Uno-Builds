using UnoWallet.Models;

namespace UnoWallet.Presentation;

public partial class CardViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isAmountVisible = true;

    [ObservableProperty]
    private bool _isCardFrozen = false;

    public CardViewModel()
    {
        CardType = CardMockData.CardType;
        CardNumber = CardMockData.CardNumber;
        CardLimitFormatted = CardMockData.CardLimit.ToString("N2");
        LimitLabel = CardMockData.LimitLabel;
        UpcomingPayments = CardMockData.UpcomingPayments;
        NotificationCount = MockData.NotificationCount;
    }

    public string CardType { get; }
    public string CardNumber { get; }
    public string CardLimitFormatted { get; }
    public string LimitLabel { get; }
    public int NotificationCount { get; }
    public IReadOnlyList<Installment> UpcomingPayments { get; }

    // Computed property for displayed amount (visible or hidden)
    public string DisplayedAmount => IsAmountVisible ? $"${CardLimitFormatted}" : "$••••••••";

    // Eye icon glyph based on visibility state
    public string VisibilityIconGlyph => IsAmountVisible ? "\uE7B3" : "\uED1A"; // Eye vs EyeOff

    [RelayCommand]
    private void ToggleAmountVisibility()
    {
        IsAmountVisible = !IsAmountVisible;
        OnPropertyChanged(nameof(DisplayedAmount));
        OnPropertyChanged(nameof(VisibilityIconGlyph));
    }

    [RelayCommand]
    private void ToggleFreezeCard()
    {
        IsCardFrozen = !IsCardFrozen;
    }
}
