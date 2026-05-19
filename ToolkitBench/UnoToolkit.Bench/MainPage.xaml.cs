using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;

namespace UnoToolkit.Bench;

public sealed partial class MainPage : Page
{
    private CancellationTokenSource? _loopCts;

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
        this.KeyDown += OnKeyDown;
        // Pause/resume the demo loops when the window is hidden/restored — saves
        // UI-thread + GC work we can't see anyway. Per Uno perf docs, manually-started
        // animations don't pause on minimize.
        App.WindowActiveChanged += OnWindowActiveChanged;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        App.WindowActiveChanged -= OnWindowActiveChanged;
        StopLoop();
    }

    private void OnWindowActiveChanged(object? sender, bool active)
    {
        if (active && _loopCts == null) StartLoop();
        else if (!active) StopLoop();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus(FocusState.Programmatic);

        // First-paint fade-in — staggered reveal of the 5 demo rows.
        var demos = new FrameworkElement[] { NavBar, TabBar, ChipGroup, Drawer, Loading };
        foreach (var d in demos) d.Opacity = 0;
        for (int i = 0; i < demos.Length; i++)
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation
            {
                BeginTime = TimeSpan.FromMilliseconds(i * 100),
                Duration = TimeSpan.FromMilliseconds(420),
                From = 0, To = 1,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            };
            Storyboard.SetTarget(anim, demos[i]);
            Storyboard.SetTargetProperty(anim, "Opacity");
            sb.Children.Add(anim);
            sb.Begin();
        }

        await Task.Delay(2500);
        StartLoop();
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Escape) StopLoop();
        else if (e.Key == VirtualKey.Space)
        {
            if (_loopCts == null || _loopCts.IsCancellationRequested) StartLoop();
            else StopLoop();
        }
    }

    private void StartLoop()
    {
        StopLoop();
        _loopCts = new CancellationTokenSource();
        var ct = _loopCts.Token;
        // All five demos animate concurrently and continuously — independent rhythms.
        _ = NavBarLoop(ct);
        _ = TabBarLoop(ct);
        _ = ChipGroupLoop(ct);
        _ = DrawerLoop(ct);
        _ = LoadingLoop(ct);
    }

    private void StopLoop()
    {
        _loopCts?.Cancel();
        _loopCts = null;
    }

    private async Task NavBarLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            NavBar.TriggerForward();
            await Wait(1700, ct);
            NavBar.TriggerForward();
            await Wait(1700, ct);
            NavBar.TriggerBack();
            await Wait(1700, ct);
            NavBar.TriggerBack();
            await Wait(1700, ct);
        }
    }

    private async Task TabBarLoop(CancellationToken ct)
    {
        await Wait(400, ct);
        var tabs = new[] { 1, 2, 3, 0 }; // Search → Library → Profile → Home
        var i = 0;
        while (!ct.IsCancellationRequested)
        {
            TabBar.TriggerTab(tabs[i % tabs.Length]);
            i++;
            await Wait(2000, ct);
        }
    }

    private async Task ChipGroupLoop(CancellationToken ct)
    {
        await Wait(800, ct);
        var ids = new[] { "Field", "Studio", "Archive", "Press" };
        while (!ct.IsCancellationRequested)
        {
            foreach (var id in ids)
            {
                ChipGroup.TriggerChip(id);
                await Wait(900, ct);
            }
            // Cascade via "All" — flips everyone at once.
            ChipGroup.TriggerChip("All");
            await Wait(2400, ct);
        }
    }

    private async Task DrawerLoop(CancellationToken ct)
    {
        await Wait(1200, ct);
        while (!ct.IsCancellationRequested)
        {
            Drawer.TriggerToggle(); // open
            await Wait(2600, ct);
            Drawer.TriggerToggle(); // close
            await Wait(1800, ct);
        }
    }

    private async Task LoadingLoop(CancellationToken ct)
    {
        await Wait(1600, ct);
        while (!ct.IsCancellationRequested)
        {
            Loading.TriggerAction();          // start
            await Wait(2400, ct);
            Loading.TriggerAction();          // reset
            await Wait(1500, ct);
        }
    }

    private static async Task Wait(int ms, CancellationToken ct)
    {
        try { await Task.Delay(ms, ct); }
        catch (TaskCanceledException) { }
    }
}
