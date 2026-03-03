using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FormaEspresso.Models;

namespace FormaEspresso.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static int _orderCounter = 246;
    private DispatcherTimer? _brewingTimer;
    private int _brewingElapsed;

    [ObservableProperty]
    private AppStage _currentStage = AppStage.Menu;

    [ObservableProperty]
    private EspressoItem? _selectedItem;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private int? _orderNumber;

    [ObservableProperty]
    private double _brewingProgress;

    [ObservableProperty]
    private string _brewingStatus = "EXTRACTING";

    public EspressoItem[] MenuItems => MenuData.Items;

    public decimal TotalPrice => SelectedItem?.Price * Quantity ?? 0;
    public string FormattedTotal => $"${TotalPrice:F2}";
    public int TotalTimeSeconds => (SelectedItem?.TimeSeconds ?? 0) * Quantity;
    public string FormattedTotalTime => $"Est. {TotalTimeSeconds} seconds";
    public string OrderSummary => $"{Quantity}x {SelectedItem?.Name}";

    public bool HasSelection => SelectedItem is not null;
    public bool IsMenuStage => CurrentStage == AppStage.Menu;
    public bool IsCustomizeStage => CurrentStage == AppStage.Customize;
    public bool IsBrewingStage => CurrentStage == AppStage.Brewing;
    public bool IsReadyStage => CurrentStage == AppStage.Ready;

    partial void OnSelectedItemChanged(EspressoItem? value)
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(TotalPrice));
        OnPropertyChanged(nameof(FormattedTotal));
        OnPropertyChanged(nameof(TotalTimeSeconds));
        OnPropertyChanged(nameof(FormattedTotalTime));
        OnPropertyChanged(nameof(OrderSummary));
    }

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(TotalPrice));
        OnPropertyChanged(nameof(FormattedTotal));
        OnPropertyChanged(nameof(TotalTimeSeconds));
        OnPropertyChanged(nameof(FormattedTotalTime));
        OnPropertyChanged(nameof(OrderSummary));
    }

    partial void OnCurrentStageChanged(AppStage value)
    {
        OnPropertyChanged(nameof(IsMenuStage));
        OnPropertyChanged(nameof(IsCustomizeStage));
        OnPropertyChanged(nameof(IsBrewingStage));
        OnPropertyChanged(nameof(IsReadyStage));
    }

    [RelayCommand]
    private void SelectItem(EspressoItem item)
    {
        SelectedItem = item;
    }

    [RelayCommand]
    private void SetQuantity(int qty)
    {
        Quantity = qty;
    }

    [RelayCommand]
    private void ContinueToCustomize()
    {
        if (SelectedItem is not null)
        {
            CurrentStage = AppStage.Customize;
        }
    }

    [RelayCommand]
    private void BackToMenu()
    {
        CurrentStage = AppStage.Menu;
    }

    [RelayCommand]
    private void PlaceOrder()
    {
        OrderNumber = ++_orderCounter;
        BrewingProgress = 0;
        BrewingStatus = "EXTRACTING";
        _brewingElapsed = 0;
        CurrentStage = AppStage.Brewing;
        StartBrewing();
    }

    [RelayCommand]
    private void NewOrder()
    {
        SelectedItem = null;
        Quantity = 1;
        OrderNumber = null;
        BrewingProgress = 0;
        CurrentStage = AppStage.Menu;
    }

    [RelayCommand]
    private void GoHome()
    {
        NewOrder();
    }

    private void StartBrewing()
    {
        _brewingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _brewingTimer.Tick += OnBrewingTick;
        _brewingTimer.Start();
    }

    private void OnBrewingTick(object? sender, object e)
    {
        _brewingElapsed += 100;
        var totalDuration = 5000;
        BrewingProgress = Math.Min(100, (_brewingElapsed / (double)totalDuration) * 100);

        if (BrewingProgress >= 100)
        {
            _brewingTimer?.Stop();
            _brewingTimer = null;
            BrewingStatus = "COMPLETE";
            CurrentStage = AppStage.Ready;
        }
    }
}
