using Sanctum.Services;
using Sanctum.ViewModels;

namespace Sanctum.Pages;

public sealed partial class FocusSessionPage : UserControl
{
    public FocusSessionViewModel ViewModel { get; }

    public FocusSessionPage()
    {
        var appState = App.Services!.GetRequiredService<IAppStateService>();
        ViewModel = new FocusSessionViewModel(appState);

        this.InitializeComponent();
    }

    public string GetPauseIcon(bool isPaused)
    {
        return isPaused ? "\uE768" : "\uE769"; // Play : Pause
    }
}
