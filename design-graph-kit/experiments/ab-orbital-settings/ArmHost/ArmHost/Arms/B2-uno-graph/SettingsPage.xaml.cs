// SettingsPage.xaml.cs — "Orbital" Settings screen (Design Graph arm B2).
//
// Implements the behavior the Design Graph declares and nothing more:
//   - state.settings.entering : staggered FadeUp of the four cards on Loaded
//                               (0 / 100 / 200 / 300 ms; header does not animate).
//   - state.profile.saved     : Save button content flips to "Saved!" for 1500 ms.
//   - component.action-button.clear-recents -> control.actions.cleared-dialog
//                               (ContentDialog "Cleared" / "Recent projects list
//                               has been cleared." / "OK").
//   - component.action-button.open-data-folder : creates and opens
//                               LocalApplicationData/Orbital in the OS shell.
//   - component.action-button.open-docs : launches the Uno docs URL in the browser.
//   - component.page-header    : SearchBorder.Tapped raises the static
//                               SearchRequested event; the graph leaves its
//                               subscriber unresolved, so nothing is opened here.
//
// Documented assumptions (graph `unresolved` items):
//   - unresolved.viewmodel-identity: no DataContext is set; bindings render their
//     declared '...' fallbacks, matching the mock.
//   - unresolved.entrance-timing-constants: FadeUp duration is taken as the declared
//     OrbitalFadeUpDuration = 350 ms; the stagger uses the observed per-card delays
//     (0/100/200/300 ms), which the graph records as the page-level behavior.
//   - SettingsService / IProjectContext are app services outside this experiment's
//     scope; their call sites are marked below and the surrounding presentation
//     behavior (trim, Saved! flip, cleared dialog) is implemented as declared.

using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace AbExperiment.B2;

public sealed partial class SettingsPage : Page
{
    /// <summary>
    /// Raised when the header search pill is tapped. The graph declares the event
    /// (component.page-header) but its subscriber is unresolved
    /// (unresolved.search-requested-target), so this page only raises it.
    /// </summary>
    public static event EventHandler? SearchRequested;

    private const int FadeUpDurationMs = 350;   // OrbitalFadeUpDuration (see assumptions above)
    private const int SavedFlashMs = 1500;      // state.profile.saved duration

    private bool _saveFlashInProgress;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    // state.settings.entering — staggered card entrance; the header does not animate.
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Initial value: the graph declares UsernameBox is seeded from
        // SettingsService.GetStoredUsername(); that service is out of scope here.
        // UsernameBox.Text = SettingsService.GetStoredUsername();

        FadeUp(ProfileSection, delayMs: 0);
        FadeUp(AboutSection, delayMs: 100);
        FadeUp(PathsSection, delayMs: 200);
        FadeUp(ActionsSection, delayMs: 300);
    }

    // Local stand-in for AnimationHelper.FadeUp: Opacity 0 -> 1, TranslateY 8 -> 0.
    private static void FadeUp(FrameworkElement element, int delayMs)
    {
        var duration = new Duration(TimeSpan.FromMilliseconds(FadeUpDurationMs));
        var delay = TimeSpan.FromMilliseconds(delayMs);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var opacityAnimation = new DoubleAnimation
        {
            To = 1,
            Duration = duration,
            BeginTime = delay,
            EasingFunction = ease
        };
        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

        var translateAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = duration,
            BeginTime = delay,
            EasingFunction = ease
        };
        Storyboard.SetTarget(translateAnimation, element);
        Storyboard.SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(TranslateTransform.Y)");

        var storyboard = new Storyboard();
        storyboard.Children.Add(opacityAnimation);
        storyboard.Children.Add(translateAnimation);
        storyboard.Begin();
    }

    // control.profile.save — persists the trimmed username, then shows the
    // transient state.profile.saved presentation ("Saved!" for 1500 ms).
    private async void SaveUsernameButton_Click(object sender, RoutedEventArgs e)
    {
        if (_saveFlashInProgress)
        {
            return;
        }

        var username = UsernameBox.Text?.Trim() ?? string.Empty;

        // Persistence hook (service outside this experiment's scope):
        // SettingsService.SaveUsername(username);
        _ = username;

        _saveFlashInProgress = true;
        SaveUsernameButton.Content = "Saved!";
        try
        {
            await System.Threading.Tasks.Task.Delay(SavedFlashMs);
        }
        finally
        {
            SaveUsernameButton.Content = "Save";
            _saveFlashInProgress = false;
        }
    }

    // component.action-button.clear-recents — clears the recent-projects list,
    // then shows control.actions.cleared-dialog.
    private async void ClearRecentsButton_Click(object sender, RoutedEventArgs e)
    {
        // Clearing hook (service outside this experiment's scope):
        // _projectContext.ClearRecentProjects();

        var dialog = new ContentDialog
        {
            Title = "Cleared",
            Content = "Recent projects list has been cleared.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    // component.action-button.open-data-folder — creates and opens the
    // LocalApplicationData/Orbital folder in the OS shell (external navigation).
    private async void OpenDataFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Orbital");
            Directory.CreateDirectory(folder);
            await Windows.System.Launcher.LaunchFolderPathAsync(folder);
        }
        catch
        {
            // Shell launch is best-effort; the graph declares no failure UI.
        }
    }

    // component.action-button.open-docs — launches the docs URL in the system browser.
    private async void OpenDocsButton_Click(object sender, RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(
            new Uri("https://platform.uno/docs/articles/intro.html"));
    }

    // component.page-header search pill — raises the static SearchRequested event.
    private void SearchBorder_Tapped(object sender, TappedRoutedEventArgs e)
    {
        SearchRequested?.Invoke(this, EventArgs.Empty);
    }
}
