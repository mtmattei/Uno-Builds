using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class TaskFilterTabs : UserControl
{
    public event EventHandler<string>? FilterChanged;
    public event EventHandler? NewTaskRequested;

    private string _selectedFilter = "All";

    public TaskFilterTabs()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    public static readonly DependencyProperty UrgentCountProperty =
        DependencyProperty.Register(nameof(UrgentCount), typeof(int), typeof(TaskFilterTabs),
            new PropertyMetadata(3, OnCountChanged));

    public static readonly DependencyProperty UnassignedCountProperty =
        DependencyProperty.Register(nameof(UnassignedCount), typeof(int), typeof(TaskFilterTabs),
            new PropertyMetadata(5, OnCountChanged));

    public int UrgentCount
    {
        get => (int)GetValue(UrgentCountProperty);
        set => SetValue(UrgentCountProperty, value);
    }

    public int UnassignedCount
    {
        get => (int)GetValue(UnassignedCountProperty);
        set => SetValue(UnassignedCountProperty, value);
    }

    public string SelectedFilter => _selectedFilter;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateTabStyles();
        UpdateCounts();
    }

    private void OnTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filter)
        {
            _selectedFilter = filter;
            UpdateTabStyles();
            FilterChanged?.Invoke(this, filter);
        }
    }

    private void OnNewTaskClick(object sender, RoutedEventArgs e)
    {
        NewTaskRequested?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTabStyles()
    {
        var buttons = new[] { AllTab, UrgentTab, ScheduledTab, UnassignedTab };

        foreach (var button in buttons)
        {
            if (button == null) continue;

            var isSelected = button.Tag?.ToString() == _selectedFilter;

            if (isSelected)
            {
                button.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
            }
            else
            {
                button.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                button.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 144, 144, 144));
            }
        }
    }

    private void UpdateCounts()
    {
        if (UrgentCountText != null)
            UrgentCountText.Text = UrgentCount.ToString();

        if (UnassignedCountText != null)
            UnassignedCountText.Text = UnassignedCount.ToString();
    }

    private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TaskFilterTabs tabs)
        {
            tabs.UpdateCounts();
        }
    }
}
