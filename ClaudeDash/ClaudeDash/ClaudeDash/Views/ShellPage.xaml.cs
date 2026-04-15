using ClaudeDash.Services;
using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Input;

namespace ClaudeDash.Views;

public sealed partial class ShellPage : Page
{
    private readonly INavigationService _navService;
    private readonly ISearchIndexService _searchService;
    private readonly IBackgroundScannerService _scanner;
    private readonly SlideOverService _slideOverService;
    private readonly SearchOverlayViewModel _searchViewModel;

    // Toast queue
    private readonly Queue<string> _toastQueue = new();
    private bool _toastShowing;

    // Clock timer
    private DispatcherTimer? _clockTimer;

    public ShellPage()
    {
        var host = App.Current.Host!.Services;
        _navService = host.GetRequiredService<INavigationService>();
        _searchService = host.GetRequiredService<ISearchIndexService>();
        _scanner = host.GetRequiredService<IBackgroundScannerService>();
        _slideOverService = host.GetRequiredService<SlideOverService>();
        _searchViewModel = new SearchOverlayViewModel(_searchService, _navService);

        this.InitializeComponent();

        // Create MVUX DataContext via DI (BindableModel mirrors Model's constructor)
        DataContext = ActivatorUtilities.CreateInstance<BindableShellModel>(host);

        // Wire nav frame and defer navigation so the window can render first
        _navService.SetFrame(ContentFrame);
        DispatcherQueue.TryEnqueue(() => _navService.NavigateTo("home"));

        SearchOverlayControl.Initialize(_searchViewModel);

        // Wire slide-over panel
        _slideOverService.ShowRequested += (title, content) =>
            DispatcherQueue.TryEnqueue(() => SlideOverControl.Show(title, content));
        _slideOverService.HideRequested += () =>
            DispatcherQueue.TryEnqueue(() => SlideOverControl.Hide());

        // Start background scanner
        _scanner.ScanCompleted += OnScanCompleted;
        _scanner.Start();

        // Clock timer — minimal, updates a bound state
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (_, _) =>
        {
            if (DataContext is BindableShellModel vm)
                vm.UpdateTime.Execute(null);
        };
        _clockTimer.Start();
    }

    // Handle sidebar nav selection
    internal void NavigateToPage(string pageKey)
    {
        _navService.NavigateTo(pageKey);
    }

    private void OnScanCompleted(ScanSnapshot snapshot)
    {
        if (snapshot.ChangedCategories.Count > 0)
        {
            DispatcherQueue.TryEnqueue(() =>
                ShowToast($"Scan: {string.Join(", ", snapshot.ChangedCategories)} updated"));
        }
    }

    // --- Keyboard shortcuts ---
    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
        var isCtrl = ctrlState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        if (isCtrl)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.K:
                    _searchViewModel.Toggle();
                    e.Handled = true;
                    return;
            }
        }

        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            if (SlideOverControl.IsOpen)
            {
                _slideOverService.Hide();
                e.Handled = true;
                return;
            }
            if (_searchViewModel.IsOpen)
            {
                _searchViewModel.Close();
                e.Handled = true;
            }
        }
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is NavItem item)
        {
            _navService.NavigateTo(item.Key);
        }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e) => _searchViewModel.Toggle();

    private void OpenDocs_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { FileName = "https://platform.uno/docs/articles/intro.html", UseShellExecute = true });
        }
        catch { }
    }

    // --- Toast ---
    public void ShowToast(string message)
    {
        _toastQueue.Enqueue(message);
        if (!_toastShowing)
            _ = ProcessToastQueueAsync();
    }

    private async Task ProcessToastQueueAsync()
    {
        _toastShowing = true;
        while (_toastQueue.Count > 0)
        {
            var msg = _toastQueue.Dequeue();
            ToastText.Text = msg;
            ToastBorder.Visibility = Visibility.Visible;
            ToastBorder.Opacity = 1;
            await Task.Delay(2500);
            ToastBorder.Opacity = 0;
            await Task.Delay(300);
            ToastBorder.Visibility = Visibility.Collapsed;
        }
        _toastShowing = false;
    }
}
