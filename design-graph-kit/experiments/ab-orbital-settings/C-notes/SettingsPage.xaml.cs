// SettingsPage.xaml.cs — code-behind for the "Orbital" settings screen (arm C).
// Kept minimal: entrance animation, Save label feedback, Clear-Recents dialog,
// and the docs link. Persistence and the recents/data-folder services are the
// app layer's responsibility (see notes.md) and are marked with TODOs.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;

namespace AbExperiment.C;

public sealed partial class SettingsPage : Page
{
    private const string DocsUrl = "https://platform.uno/docs/articles/intro.html";

    private static readonly TimeSpan CardDuration = TimeSpan.FromMilliseconds(350);
    private static readonly TimeSpan CardStagger = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan SavedLabelHold = TimeSpan.FromMilliseconds(1500);

    private CancellationTokenSource? _saveLabelCts;
    private bool _entrancePlayed;

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var cards = new FrameworkElement[] { ProfileSection, AboutSection, PathsSection, ActionsSection };

        if (_entrancePlayed)
        {
            // Re-navigation: show the settled state without replaying the entrance.
            foreach (var card in cards)
            {
                card.Opacity = 1;
                if (card.RenderTransform is CompositeTransform transform)
                {
                    transform.TranslateY = 0;
                }
            }
            return;
        }

        _entrancePlayed = true;
        PlayEntrance(cards);
    }

    /// <summary>
    /// Cards fade in and rise ~16px, staggered 100 ms apart (0/100/200/300 ms),
    /// 350 ms each, ease-out.
    /// </summary>
    private static void PlayEntrance(FrameworkElement[] cards)
    {
        var storyboard = new Storyboard();

        for (var i = 0; i < cards.Length; i++)
        {
            var beginTime = TimeSpan.FromMilliseconds(CardStagger.TotalMilliseconds * i);

            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(CardDuration),
                BeginTime = beginTime,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(fade, cards[i]);
            Storyboard.SetTargetProperty(fade, "Opacity");
            storyboard.Children.Add(fade);

            var rise = new DoubleAnimation
            {
                From = 16,
                To = 0,
                Duration = new Duration(CardDuration),
                BeginTime = beginTime,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(rise, cards[i]);
            Storyboard.SetTargetProperty(rise, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            storyboard.Children.Add(rise);
        }

        storyboard.Begin();
    }

    private async void SaveUsernameButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: persist UsernameBox.Text through the app's settings service
        // (out of scope for this standalone page, per notes.md).

        // Transient confirmation: "Saved!" for 1.5 s, then revert. Cancel any
        // pending revert so rapid clicks don't cut the confirmation short.
        _saveLabelCts?.Cancel();
        _saveLabelCts = new CancellationTokenSource();
        var token = _saveLabelCts.Token;

        SaveUsernameButton.Content = "Saved!";
        try
        {
            await Task.Delay(SavedLabelHold, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        SaveUsernameButton.Content = "Save";
    }

    private async void ClearRecentsButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: clear the recent-projects list via the app's recents service
        // before confirming (service is out of scope for this standalone page).

        var dialog = new ContentDialog
        {
            Title = "Cleared",
            Content = "Recent projects list has been cleared.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
            RequestedTheme = ElementTheme.Dark,
        };
        await dialog.ShowAsync();
    }

    private void OpenDataFolderButton_Click(object sender, RoutedEventArgs e)
    {
        // Intentionally not wired: the target folder is app-determined and the
        // behavior is left to the app layer (per notes.md).
    }

    private async void OpenDocsButton_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri(DocsUrl));
    }
}
