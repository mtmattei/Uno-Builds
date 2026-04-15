using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnimationHelper.FadeUp(ProfileSection, 0);
        AnimationHelper.FadeUp(AboutSection, 100);
        AnimationHelper.FadeUp(PathsSection, 200);
        AnimationHelper.FadeUp(ActionsSection, 300);

        LoadUsername();
        WireButtons();
    }

    private void LoadUsername()
    {
        var name = SettingsService.GetStoredUsername();
        if (!string.IsNullOrEmpty(name))
            UsernameBox.Text = name;

        SaveUsernameButton.Click += (_, _) =>
        {
            var newName = UsernameBox.Text?.Trim() ?? "";
            SettingsService.SaveUsername(newName);

            SaveUsernameButton.Content = "Saved!";
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(1500);
                SaveUsernameButton.Content = "Save";
            });
        };
    }

    private void WireButtons()
    {
        ClearRecentsButton.Click += async (_, _) =>
        {
            var host = HostHelper.GetHost();
            if (host is null) return;

            var ctx = host.Services.GetRequiredService<IProjectContext>();
            var recents = await ctx.GetRecentProjectsAsync(CancellationToken.None);
            foreach (var p in recents)
                ctx.RemoveRecentProject(p.SolutionPath);

            var dialog = new ContentDialog
            {
                Title = "Cleared",
                Content = "Recent projects list has been cleared.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };
            await dialog.ShowAsync();
        };

        OpenDataFolderButton.Click += (_, _) =>
        {
            var dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Orbital");
            try
            {
                Directory.CreateDirectory(dataDir);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dataDir,
                    UseShellExecute = true,
                });
            }
            catch { }
        };

        OpenDocsButton.Click += async (_, _) =>
        {
            await Windows.System.Launcher.LaunchUriAsync(
                new Uri("https://platform.uno/docs/articles/intro.html"));
        };
    }
}
