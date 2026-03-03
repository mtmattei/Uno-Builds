using Sanctum.Models;

namespace Sanctum.Services;

/// <summary>
/// Manages global application state
/// </summary>
public class AppStateService : IAppStateService
{
    public event Action? StateChanged;

    public ViewMode CurrentViewMode { get; private set; } = ViewMode.Onboarding;
    public AppMode CurrentAppMode { get; private set; } = AppMode.Explore;
    public int OnboardingStep { get; private set; } = 1;
    public bool IsFocusSessionActive { get; private set; }
    public UserPreferences UserPreferences { get; private set; } = new();
    public SanityPlan? SanityPlan { get; private set; }
    public int AttentionReclaimedMinutes { get; private set; } = 127;

    public void SetViewMode(ViewMode mode)
    {
        CurrentViewMode = mode;
        NotifyStateChanged();
    }

    public void SetAppMode(AppMode mode)
    {
        CurrentAppMode = mode;
        NotifyStateChanged();
    }

    public void SetOnboardingStep(int step)
    {
        OnboardingStep = Math.Clamp(step, 1, 3);
        NotifyStateChanged();
    }

    public void SetFocusSessionActive(bool active)
    {
        IsFocusSessionActive = active;
        NotifyStateChanged();
    }

    public void SetUserPreferences(UserPreferences preferences)
    {
        UserPreferences = preferences;
        NotifyStateChanged();
    }

    public void SetSanityPlan(SanityPlan plan)
    {
        SanityPlan = plan;
        NotifyStateChanged();
    }

    public void AddAttentionReclaimed(int minutes)
    {
        AttentionReclaimedMinutes += minutes;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}

public interface IAppStateService
{
    event Action? StateChanged;
    ViewMode CurrentViewMode { get; }
    AppMode CurrentAppMode { get; }
    int OnboardingStep { get; }
    bool IsFocusSessionActive { get; }
    UserPreferences UserPreferences { get; }
    SanityPlan? SanityPlan { get; }
    int AttentionReclaimedMinutes { get; }

    void SetViewMode(ViewMode mode);
    void SetAppMode(AppMode mode);
    void SetOnboardingStep(int step);
    void SetFocusSessionActive(bool active);
    void SetUserPreferences(UserPreferences preferences);
    void SetSanityPlan(SanityPlan plan);
    void AddAttentionReclaimed(int minutes);
}
