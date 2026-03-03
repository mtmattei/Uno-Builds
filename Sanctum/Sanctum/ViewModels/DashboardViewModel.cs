using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sanctum.Models;
using Sanctum.Services;
using System.Collections.ObjectModel;

namespace Sanctum.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAppStateService _appState;
    private readonly IMockDataService _mockData;

    public DashboardViewModel(IAppStateService appState, IMockDataService mockData)
    {
        _appState = appState;
        _mockData = mockData;
        _appState.StateChanged += OnStateChanged;
        LoadData();
    }

    [ObservableProperty]
    private AppMode _currentMode = AppMode.Explore;

    [ObservableProperty]
    private string _smartSynthesis = string.Empty;

    [ObservableProperty]
    private int _attentionReclaimed;

    [ObservableProperty]
    private bool _showContextToast;

    public ObservableCollection<SourceControlItem> SourceControls { get; } = [];

    private void LoadData()
    {
        SmartSynthesis = _mockData.GenerateSmartSynthesis(_appState.CurrentAppMode);
        AttentionReclaimed = _appState.AttentionReclaimedMinutes;

        foreach (var item in _mockData.GetSourceControlItems())
        {
            SourceControls.Add(item);
        }
    }

    private void OnStateChanged()
    {
        CurrentMode = _appState.CurrentAppMode;
        SmartSynthesis = _mockData.GenerateSmartSynthesis(CurrentMode);
        AttentionReclaimed = _appState.AttentionReclaimedMinutes;
    }

    [RelayCommand]
    private void SetMode(string mode)
    {
        var appMode = mode switch
        {
            "focus" => AppMode.Focus,
            "recover" => AppMode.Recover,
            _ => AppMode.Explore
        };
        _appState.SetAppMode(appMode);
    }

    [RelayCommand]
    private void CycleSourceStatus(SourceControlItem item)
    {
        item.Status = item.Status switch
        {
            SourceStatus.Allowed => SourceStatus.Batched,
            SourceStatus.Batched => SourceStatus.Muted,
            _ => SourceStatus.Allowed
        };
        OnPropertyChanged(nameof(SourceControls));
    }

    [RelayCommand]
    private void ShowContextSuggestion()
    {
        ShowContextToast = true;
    }

    [RelayCommand]
    private void DismissContextToast()
    {
        ShowContextToast = false;
    }

    [RelayCommand]
    private void SwitchToRecover()
    {
        _appState.SetAppMode(AppMode.Recover);
        ShowContextToast = false;
    }
}
