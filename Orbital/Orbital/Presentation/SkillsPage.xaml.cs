using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class SkillsPage : Page
{
    private ISkillsService? _skillsService;

    private static Brush Brush(string key) => (Brush)Application.Current.Resources[key];

    public SkillsPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnimationHelper.FadeUp(StatsRow, 0);
        AnimationHelper.FadeUp(SkillGroupsRepeater, 100);

        try
        {
            _skillsService = ((App)Application.Current).Host!.Services.GetRequiredService<ISkillsService>();
        }
        catch (Exception ex)
        {
            OrbitalLog.Warn(ex, "SkillsPage.OnLoaded.ResolveService");
        }
    }

    private void OnSkillCardLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is Border card && card.DataContext is SkillInfo skill)
        {
            ApplyVisualState(card, skill.IsActive);
        }
    }

    private async void OnSkillCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not Border card || card.DataContext is not SkillInfo skill || _skillsService is null)
            return;

        var newActive = !skill.IsActive;
        try
        {
            await _skillsService.ToggleSkillAsync(skill.Id, newActive, CancellationToken.None);
            ApplyVisualState(card, newActive);
        }
        catch (Exception ex)
        {
            OrbitalLog.Warn(ex, $"SkillsPage.ToggleSkill({skill.Id})");
        }
    }

    private static void ApplyVisualState(Border card, bool isActive)
    {
        card.Opacity = isActive ? 1.0 : 0.6;
        card.BorderBrush = isActive ? Brush("OrbitalSurface3Brush") : Brush("OrbitalSurface2Brush");

        var track = FindByName<Border>(card, "ToggleTrack");
        var dot = FindByName<Border>(card, "ToggleDot");
        if (track is not null)
        {
            track.Background = isActive ? Brush("OrbitalEmerald500_15Brush") : Brush("OrbitalZinc500_10Brush");
        }
        if (dot is not null)
        {
            dot.Background = isActive ? Brush("OrbitalEmerald400Brush") : Brush("OrbitalZinc500Brush");
            dot.HorizontalAlignment = isActive ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
    }

    private static T? FindByName<T>(DependencyObject root, string name) where T : FrameworkElement
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T fe && fe.Name == name) return fe;
            var result = FindByName<T>(child, name);
            if (result is not null) return result;
        }
        return null;
    }
}
