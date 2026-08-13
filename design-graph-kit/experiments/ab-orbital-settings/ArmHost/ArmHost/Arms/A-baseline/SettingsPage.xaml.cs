using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AbExperiment.A;

/// <summary>
/// "Orbital" Settings screen. The brief describes a static populated mock, so the
/// code-behind stays minimal: click handlers are stubs to be wired to a service
/// layer later, except documentation, which safely launches the public docs URL.
/// The "..." values in About/Paths represent the still-loading state from the mock
/// and are populated by whatever backend fills them in later (via the named
/// TextBlocks: UnoSdkValue, DotnetRuntimeValue, RendererValue, PlatformValue,
/// ProjectRootValue, RecentDbValue, SkillsPathValue).
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void OnSaveDisplayName(object sender, RoutedEventArgs e)
    {
        // TODO: persist DisplayNameBox.Text to user preferences.
    }

    private void OnClearRecentProjects(object sender, RoutedEventArgs e)
    {
        // TODO: clear the recent-projects database.
    }

    private void OnOpenDataFolder(object sender, RoutedEventArgs e)
    {
        // TODO: open the app data folder in the platform file explorer.
    }

    private async void OnOpenDocumentation(object sender, RoutedEventArgs e)
    {
        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://platform.uno/docs/"));
    }
}
