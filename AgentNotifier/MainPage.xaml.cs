using AgentNotifier.ViewModels;
using System.Collections.Specialized;

namespace AgentNotifier;

public sealed partial class MainPage : Page
{
    private const int WindowWidth = 720;
    private const int BaseHeight = 240;           // Header + Stats + Section label + Footer + Padding
    private const int AgentCardHeight = 72;       // Collapsed card height + spacing (includes ASCII progress bar)
    private const int WindowMinHeight = 280;      // Minimum window height (empty state)
    private const int WindowMaxHeight = 900;      // Maximum window height

    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        this.DataContext = ViewModel;

        ViewModel.Agents.CollectionChanged += OnAgentsCollectionChanged;

        // Initial resize
        Loaded += (s, e) => UpdateWindowSize(ViewModel.Agents.Count);
    }

    private void OnAgentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateWindowSize(ViewModel.Agents.Count);
    }

    private void UpdateWindowSize(int agentCount)
    {
        var window = App.Current.MainWindow;
        if (window?.AppWindow == null) return;

        var calculatedHeight = BaseHeight + (agentCount * AgentCardHeight);
        var newHeight = Math.Clamp(calculatedHeight, WindowMinHeight, WindowMaxHeight);

        window.AppWindow.Resize(new Windows.Graphics.SizeInt32
        {
            Width = WindowWidth,
            Height = newHeight
        });
    }
}
