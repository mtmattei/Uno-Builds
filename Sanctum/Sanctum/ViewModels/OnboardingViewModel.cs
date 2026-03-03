using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sanctum.Models;
using Sanctum.Services;
using System.Collections.ObjectModel;

namespace Sanctum.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly IAppStateService _appState;
    private readonly IMockDataService _mockData;

    public OnboardingViewModel(IAppStateService appState, IMockDataService mockData)
    {
        _appState = appState;
        _mockData = mockData;
        LoadOptions();
    }

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isFocusModeEnabled;

    [ObservableProperty]
    private string _manifesto = string.Empty;

    [ObservableProperty]
    private bool _canProceed;

    public ObservableCollection<GoalOption> Goals { get; } = [];
    public ObservableCollection<SourceOption> Sources { get; } = [];
    public ObservableCollection<string> Rules { get; } = [];

    private void LoadOptions()
    {
        foreach (var goal in _mockData.GetGoalOptions())
        {
            Goals.Add(goal);
        }
        foreach (var source in _mockData.GetSourceOptions())
        {
            Sources.Add(source);
        }
    }

    [RelayCommand]
    private void ToggleGoal(GoalOption goal)
    {
        goal.IsSelected = !goal.IsSelected;
        UpdateCanProceed();
        OnPropertyChanged(nameof(Goals));
    }

    [RelayCommand]
    private void ToggleSource(SourceOption source)
    {
        source.IsSelected = !source.IsSelected;
        OnPropertyChanged(nameof(Sources));
    }

    private void UpdateCanProceed()
    {
        CanProceed = Goals.Any(g => g.IsSelected);
    }

    [RelayCommand]
    private async Task NextStepAsync()
    {
        if (CurrentStep == 1)
        {
            CurrentStep = 2;
            _appState.SetOnboardingStep(2);
        }
        else if (CurrentStep == 2)
        {
            IsLoading = true;

            // Simulate AI generation delay
            await Task.Delay(1500);

            var preferences = new UserPreferences
            {
                Goals = Goals.Where(g => g.IsSelected).Select(g => g.Id).ToList(),
                Sources = Sources.Where(s => s.IsSelected).Select(s => s.Id).ToList()
            };

            var plan = _mockData.GenerateSanityPlan(preferences);
            _appState.SetUserPreferences(preferences);
            _appState.SetSanityPlan(plan);

            Manifesto = plan.Manifesto;
            Rules.Clear();
            foreach (var rule in plan.Rules)
            {
                Rules.Add(rule);
            }

            IsLoading = false;
            CurrentStep = 3;
            _appState.SetOnboardingStep(3);
        }
    }

    [RelayCommand]
    private void EnableFocusMode()
    {
        IsFocusModeEnabled = true;
    }

    [RelayCommand]
    private void EnterSanctum()
    {
        _appState.SetViewMode(ViewMode.Dashboard);
    }
}
