using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sanctum.Models;
using Sanctum.Services;

namespace Sanctum.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAppStateService _appState;
    private readonly INavigationService _navigation;

    public MainViewModel(IAppStateService appState, INavigationService navigation)
    {
        _appState = appState;
        _navigation = navigation;
        _appState.StateChanged += OnStateChanged;
    }

    [ObservableProperty]
    private bool _isOnboarding = true;

    [ObservableProperty]
    private string _selectedNavItem = "command";

    [ObservableProperty]
    private AppMode _currentMode = AppMode.Explore;

    private void OnStateChanged()
    {
        IsOnboarding = _appState.CurrentViewMode == ViewMode.Onboarding;
        CurrentMode = _appState.CurrentAppMode;
    }

    [RelayCommand]
    private void NavigateTo(string destination)
    {
        SelectedNavItem = destination;
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
    private void CompleteOnboarding()
    {
        _appState.SetViewMode(ViewMode.Dashboard);
        IsOnboarding = false;
    }
}
