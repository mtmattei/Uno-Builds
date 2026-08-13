// SettingsPage code-behind — implements the Design Graph's behavioral facts:
//   state.settings.entering  : staggered fade-up of the four section cards on Loaded
//                              (0/100/200/300 ms).
//   state.profile.saved      : Save button content becomes "Saved!" for 1.5 s
//                              (triggered by control.profile.save).
//   dialog.recents-cleared   : ContentDialog shown by control.actions.clear-recents.
//   control.actions.open-docs: launches the docs URI declared in the graph.
//
// Deliberately NOT wired (graph-unresolved or app-level concerns, per rules 2/7):
//   - SearchButton has no Click: unresolved.header.search-target leaves its target UI
//     unknown ("global command palette" vs "search overlay").
//   - OnOpenDataFolderClick is a stub: the graph declares the button but supplies no
//     folder path or launch target.
//   - Username persistence (SettingsService) and the env/path value bindings belong to
//     the host app's services/ViewModel and are outside this isolated page.

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace AbExperiment.B;

public sealed partial class SettingsPage : Page
{
    // control.actions.open-docs -> properties.uri (declared in the graph).
    private const string DocsUri = "https://platform.uno/docs/articles/intro.html";

    private static readonly TimeSpan SavedFlashDuration = TimeSpan.FromSeconds(1.5);
    private const int EnterStaggerMs = 100;
    private const int EnterDurationMs = 350;
    private const double EnterRiseDistance = 16;

    private bool _savedFlashRunning;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    // state.settings.entering — fade-up each section card with a 100 ms stagger.
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        FrameworkElement[] sections = { ProfileSection, AboutSection, PathsSection, ActionsSection };
        for (var i = 0; i < sections.Length; i++)
        {
            PlayFadeUp(sections[i], delayMs: i * EnterStaggerMs);
        }
    }

    private static void PlayFadeUp(FrameworkElement element, int delayMs)
    {
        var translate = new TranslateTransform { Y = EnterRiseDistance };
        element.RenderTransform = translate;
        element.Opacity = 0;

        var duration = new Duration(TimeSpan.FromMilliseconds(EnterDurationMs));
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var fade = new DoubleAnimation { From = 0, To = 1, Duration = duration, EasingFunction = ease };
        Storyboard.SetTarget(fade, element);
        Storyboard.SetTargetProperty(fade, "Opacity");

        var rise = new DoubleAnimation { From = EnterRiseDistance, To = 0, Duration = duration, EasingFunction = ease };
        Storyboard.SetTarget(rise, translate);
        Storyboard.SetTargetProperty(rise, "Y");

        var storyboard = new Storyboard { BeginTime = TimeSpan.FromMilliseconds(delayMs) };
        storyboard.Children.Add(fade);
        storyboard.Children.Add(rise);
        storyboard.Begin();
    }

    // control.profile.save -> state.profile.saved.
    // In the host app the username is persisted through SettingsService before the
    // confirmation flash; persistence is out of scope for this isolated page.
    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (_savedFlashRunning)
        {
            return;
        }

        _savedFlashRunning = true;
        var original = SaveButton.Content;
        SaveButton.Content = "Saved!";

        try
        {
            await Task.Delay(SavedFlashDuration);
        }
        finally
        {
            SaveButton.Content = original;
            _savedFlashRunning = false;
        }
    }

    // control.actions.clear-recents -> component.dialog.recents-cleared.
    // In the host app the recents store is cleared first; here only the declared
    // confirmation dialog is shown.
    private async void OnClearRecentsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Cleared",
            Content = "Recent projects list has been cleared.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };

        await dialog.ShowAsync();
    }

    // control.actions.open-data-folder — declared in the graph with no target path or
    // launch URI, so no behavior is invented (rule 2). The host app supplies the folder.
    private void OnOpenDataFolderClick(object sender, RoutedEventArgs e)
    {
        // Intentionally left as a stub; see header comment.
    }

    // control.actions.open-docs — semanticRole externalLink; the graph declares the
    // handler launches the docs URI via Launcher.LaunchUriAsync.
    private async void OnOpenDocsClick(object sender, RoutedEventArgs e)
    {
        _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(DocsUri));
    }
}
