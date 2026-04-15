using Microsoft.UI.Xaml.Input;
using Orbital.Helpers;

namespace Orbital.Presentation;

public sealed partial class SkillsPage : Page
{
    private ISkillsService? _skillsService;
    private readonly Dictionary<string, Border> _toggleIcons = new();

    private static SolidColorBrush Brush(string key) =>
        (SolidColorBrush)Application.Current.Resources[key];

    public SkillsPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        AnimationHelper.FadeUp(StatsRow, 0);
        AnimationHelper.FadeUp(SkillGroupsPanel, 100);

        try
        {
            _skillsService = ((App)Application.Current).Host!.Services.GetRequiredService<ISkillsService>();
            var skills = await _skillsService.GetSkillsAsync(CancellationToken.None);
            PopulateGroups(skills);
        }
        catch
        {
            // Prevent silent page failure — stats are bound via model feeds
        }
    }

    private void PopulateGroups(ImmutableList<SkillInfo> skills)
    {
        SkillGroupsPanel.Children.Clear();
        _toggleIcons.Clear();

        var groups = skills
            .GroupBy(s => s.Category)
            .OrderBy(g => g.Key switch
            {
                "core" => 0, "styling" => 1, "navigation" => 2,
                "mvux" => 3, "toolkit" => 4, "testing" => 5, _ => 6
            });

        foreach (var group in groups)
        {
            // Section label
            var label = new TextBlock
            {
                Text = group.Key.ToUpperInvariant(),
                Style = (Style)Application.Current.Resources["OrbitalSectionSubLabel"],
                Foreground = Brush("OrbitalText30Brush"),
                Margin = new Thickness(0, 0, 0, 4),
            };
            SkillGroupsPanel.Children.Add(label);

            // Skill cards in 2-col grid
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(12) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                },
                RowSpacing = 8,
            };

            var items = group.ToList();
            for (var i = 0; i < items.Count; i++)
            {
                var skill = items[i];
                var col = (i % 2) * 2;
                var row = i / 2;

                while (grid.RowDefinitions.Count <= row)
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var card = CreateSkillCard(skill);
                Grid.SetColumn(card, col);
                Grid.SetRow(card, row);
                grid.Children.Add(card);
            }

            SkillGroupsPanel.Children.Add(grid);
        }
    }

    private Border CreateSkillCard(SkillInfo skill)
    {
        var card = new Border
        {
            Background = Brush("OrbitalSurface1Brush"),
            BorderBrush = skill.IsActive ? Brush("OrbitalSurface3Brush") : Brush("OrbitalSurface2Brush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(16, 14),
            Tag = skill.Id,
            Opacity = skill.IsActive ? 1.0 : 0.6,
        };
        card.Tapped += OnSkillCardTapped;

        var outerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(12) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };

        // Left: text content
        var textStack = new StackPanel { Spacing = 6 };

        // Name
        textStack.Children.Add(new TextBlock
        {
            Text = skill.Name,
            Style = (Style)Application.Current.Resources["OrbitalMonoConsole"],
            Foreground = Brush("OrbitalText75Brush"),
        });

        // Description
        textStack.Children.Add(new TextBlock
        {
            Text = skill.Description,
            Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
            Foreground = Brush("OrbitalText35Brush"),
            TextWrapping = TextWrapping.Wrap,
            MaxLines = 2,
        });

        // Stats row
        var statsRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        statsRow.Children.Add(CreateStatPill($"{skill.Invocations} calls"));
        statsRow.Children.Add(CreateStatPill($"{skill.Accuracy:P0} accuracy"));
        textStack.Children.Add(statsRow);

        Grid.SetColumn(textStack, 0);
        outerGrid.Children.Add(textStack);

        // Right: toggle indicator
        var toggleBox = CreateToggleIndicator(skill);
        Grid.SetColumn(toggleBox, 2);
        outerGrid.Children.Add(toggleBox);

        card.Child = outerGrid;
        return card;
    }

    private static StackPanel CreateStatPill(string text)
    {
        var pill = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        pill.Children.Add(new Microsoft.UI.Xaml.Shapes.Ellipse
        {
            Width = 4,
            Height = 4,
            Fill = Brush("OrbitalText25Brush"),
            VerticalAlignment = VerticalAlignment.Center,
        });
        pill.Children.Add(new TextBlock
        {
            Text = text,
            Style = (Style)Application.Current.Resources["OrbitalMonoMeta"],
            Foreground = Brush("OrbitalText30Brush"),
            VerticalAlignment = VerticalAlignment.Center,
        });
        return pill;
    }

    private Border CreateToggleIndicator(SkillInfo skill)
    {
        var box = new Border
        {
            Width = 40,
            Height = 24,
            CornerRadius = new CornerRadius(12),
            Background = skill.IsActive ? Brush("OrbitalEmerald500_15Brush") : Brush("OrbitalZinc500_10Brush"),
            VerticalAlignment = VerticalAlignment.Top,
        };

        var dot = new Border
        {
            Width = 16,
            Height = 16,
            CornerRadius = new CornerRadius(8),
            Background = skill.IsActive ? Brush("OrbitalEmerald400Brush") : Brush("OrbitalZinc500Brush"),
            HorizontalAlignment = skill.IsActive ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = new Thickness(4),
        };

        box.Child = dot;
        _toggleIcons[skill.Id] = box;
        return box;
    }

    private async void OnSkillCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is not Border card || card.Tag is not string id || _skillsService is null)
            return;

        var skills = await _skillsService.GetSkillsAsync(CancellationToken.None);
        var skill = skills.FirstOrDefault(s => s.Id == id);
        if (skill is null) return;

        var newActive = !skill.IsActive;
        await _skillsService.ToggleSkillAsync(id, newActive, CancellationToken.None);

        // Update card visual
        card.Opacity = newActive ? 1.0 : 0.6;
        card.BorderBrush = newActive ? Brush("OrbitalSurface3Brush") : Brush("OrbitalSurface2Brush");

        // Update toggle indicator
        if (_toggleIcons.TryGetValue(id, out var toggleBox))
        {
            toggleBox.Background = newActive ? Brush("OrbitalEmerald500_15Brush") : Brush("OrbitalZinc500_10Brush");
            if (toggleBox.Child is Border dot)
            {
                dot.Background = newActive ? Brush("OrbitalEmerald400Brush") : Brush("OrbitalZinc500Brush");
                dot.HorizontalAlignment = newActive ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
        }

        // Stats refresh automatically via model feeds
    }
}
