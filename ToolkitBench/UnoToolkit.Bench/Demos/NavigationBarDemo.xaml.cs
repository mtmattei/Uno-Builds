using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoToolkit.Bench.Demos;

public sealed partial class NavigationBarDemo : UserControl
{
    public NavigationBarDemoViewModel ViewModel { get; } = new();

    public NavigationBarDemo()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        ViewModel.PropertyChanged += OnVmPropertyChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleFlap.TargetText = ViewModel.PageTitle;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.PageTitle))
        {
            TitleFlap.TargetText = ViewModel.PageTitle;
            FadeBody();
        }
    }

    private async void OnBackClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.CanGoBack || ViewModel.IsAnimating) return;
        ViewModel.IsAnimating = true;
        ViewModel.GoBack();
        var dur = TitleFlap.EstimatedDuration(ViewModel.PageTitle);
        await System.Threading.Tasks.Task.Delay(dur);
        ViewModel.IsAnimating = false;
    }

    private async void OnForwardClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.CanGoForward || ViewModel.IsAnimating) return;
        ViewModel.IsAnimating = true;
        ViewModel.GoForward();
        var dur = TitleFlap.EstimatedDuration(ViewModel.PageTitle);
        await System.Threading.Tasks.Task.Delay(dur);
        ViewModel.IsAnimating = false;
    }

    public void TriggerForward() => OnForwardClick(this, new RoutedEventArgs());
    public void TriggerBack() => OnBackClick(this, new RoutedEventArgs());

    private void FadeBody()
    {
        var sb = new Storyboard();
        var fadeOut = new DoubleAnimation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            From = BodyHost.Opacity,
            To = 0,
        };
        Storyboard.SetTarget(fadeOut, BodyHost);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");

        var fadeIn = new DoubleAnimation
        {
            BeginTime = TimeSpan.FromMilliseconds(200),
            Duration = TimeSpan.FromMilliseconds(200),
            From = 0,
            To = 1,
        };
        Storyboard.SetTarget(fadeIn, BodyHost);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");

        sb.Children.Add(fadeOut);
        sb.Children.Add(fadeIn);
        sb.Begin();
    }
}

public sealed class NavigationBarDemoViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly List<NavPage> _pages = new()
    {
        new NavPage("FIELD MEMO", "PAGE 1 OF 3", "A bench is restraint, not a stage. Each control sits in its own cell so its gesture can be measured against the others without competition."),
        new NavPage("MAKER NOTE", "PAGE 2 OF 3", "What we keep is the mechanics: the ledger flap, the typewriter strike, the coin landing. What we trim is anything decorative that didn't earn its place."),
        new NavPage("RECEIPT",     "PAGE 3 OF 3", "If a movement could happen on paper, it should look like it could happen on paper. Ink absorbs, hinges weigh, edges meet — those are the cues."),
    };

    private int _depth = 0;
    private bool _isAnimating;

    public int Depth
    {
        get => _depth;
        private set
        {
            if (_depth == value) return;
            _depth = value;
            Notify(nameof(Depth));
            Notify(nameof(PageTitle));
            Notify(nameof(PageBody));
            Notify(nameof(PageMeta));
            Notify(nameof(CanGoBack));
            Notify(nameof(CanGoForward));
        }
    }

    public bool IsAnimating
    {
        get => _isAnimating;
        set
        {
            if (_isAnimating == value) return;
            _isAnimating = value;
            Notify(nameof(IsAnimating));
            Notify(nameof(CanGoBack));
            Notify(nameof(CanGoForward));
        }
    }

    public string PageTitle => _pages[_depth].Title;
    public string PageBody => _pages[_depth].Body;
    public string PageMeta => _pages[_depth].Meta;
    public bool CanGoBack => !_isAnimating && _depth > 0;
    public bool CanGoForward => !_isAnimating && _depth < _pages.Count - 1;

    public void GoBack()    { if (_depth > 0)               Depth--; }
    public void GoForward() { if (_depth < _pages.Count - 1) Depth++; }

    private void Notify([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name!));

    private record NavPage(string Title, string Meta, string Body);
}
