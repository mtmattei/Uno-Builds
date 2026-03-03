using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListHold.Models;

namespace ListHold.ViewModels;

public partial class ListItemViewModel : ObservableObject
{
    private const double HoldDurationMs = 800.0;
    private const double LockThreshold = 0.95;
    private const int UpdateIntervalMs = 16;

    private readonly DispatcherTimer _holdTimer;
    private DateTime _holdStartTime;

    [ObservableProperty]
    private ListItemModel _item;

    [ObservableProperty]
    private double _holdProgress;

    [ObservableProperty]
    private HoldState _state = HoldState.Idle;

    [ObservableProperty]
    private int _revealStage;

    [ObservableProperty]
    private bool _showStage1;

    [ObservableProperty]
    private bool _showStage2;

    [ObservableProperty]
    private bool _showStage3;

    [ObservableProperty]
    private bool _isHolding;

    [ObservableProperty]
    private double _containerScale = 1.0;

    public ListItemViewModel(ListItemModel item)
    {
        _item = item;
        _holdTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(UpdateIntervalMs)
        };
        _holdTimer.Tick += OnHoldTimerTick;
    }

    public void StartHold()
    {
        if (State == HoldState.Locked)
            return;

        State = HoldState.Holding;
        IsHolding = true;
        ContainerScale = 0.995;
        _holdStartTime = DateTime.Now;
        _holdTimer.Start();
    }

    public void EndHold()
    {
        if (State != HoldState.Holding)
            return;

        _holdTimer.Stop();
        IsHolding = false;
        ContainerScale = 1.0;

        if (HoldProgress < LockThreshold)
        {
            ResetProgress();
        }
    }

    private void OnHoldTimerTick(object? sender, object e)
    {
        var elapsed = (DateTime.Now - _holdStartTime).TotalMilliseconds;
        HoldProgress = Math.Min(elapsed / HoldDurationMs, 1.0);

        UpdateRevealStage();

        if (HoldProgress >= LockThreshold)
        {
            Lock();
        }
    }

    private void UpdateRevealStage()
    {
        var newStage = HoldProgress switch
        {
            < 0.30 => 0,
            < 0.60 => 1,
            < 0.90 => 2,
            _ => 3
        };

        if (newStage != RevealStage)
        {
            RevealStage = newStage;
            ShowStage1 = newStage >= 1;
            ShowStage2 = newStage >= 2;
            ShowStage3 = newStage >= 3;
        }
    }

    private void Lock()
    {
        _holdTimer.Stop();
        State = HoldState.Locked;
        IsHolding = false;
        ContainerScale = 1.0;
        HoldProgress = 1.0;
        RevealStage = 3;
        ShowStage1 = true;
        ShowStage2 = true;
        ShowStage3 = true;
    }

    private void ResetProgress()
    {
        State = HoldState.Idle;
        HoldProgress = 0;
        RevealStage = 0;
        ShowStage1 = false;
        ShowStage2 = false;
        ShowStage3 = false;
    }

    [RelayCommand]
    private void Collapse()
    {
        if (State == HoldState.Locked)
        {
            ResetProgress();
        }
    }
}
