using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MsnMessenger.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private bool _notificationsEnabled = true;

    [ObservableProperty]
    private bool _nudgeVibrationEnabled = true;

    [ObservableProperty]
    private bool _messageSoundsEnabled = true;

    [ObservableProperty]
    private bool _showNowPlaying = true;

    public string AppVersion => "2026.1.0";

    public event EventHandler? ThemeChanged;

    [RelayCommand]
    private void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
