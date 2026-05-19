using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using YouTubeMs.Services.Motion;
using YouTubeMs.Services.VideoPlayer;

namespace YouTubeMs.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
        this.KeyDown += OnKeyDown;
    }

    // Narrow breakpoint matches Uno Toolkit's default (640px)
    private const double NarrowBreakpoint = 641;

    private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var narrow = e.NewSize.Width < NarrowBreakpoint;
        SidebarColumn.Width = new GridLength(narrow ? 0 : 240);
    }

    private void OnMenuClicked(object sender, RoutedEventArgs e)
    {
        OpenMobileNav();
    }

    private void OpenMobileNav()
    {
        MobileNavHost.Visibility = Visibility.Visible;
        var ease = new QuarticEase { EasingMode = EasingMode.EaseOut };
        var sb = new Storyboard();

        var fade = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(180)), EasingFunction = ease };
        Storyboard.SetTarget(fade, MobileNavBackdrop);
        Storyboard.SetTargetProperty(fade, "Opacity");
        sb.Children.Add(fade);

        var slide = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(260)), EasingFunction = ease };
        Storyboard.SetTarget(slide, MobileNavTranslate);
        Storyboard.SetTargetProperty(slide, "X");
        sb.Children.Add(slide);

        sb.Begin();
    }

    private void CloseMobileNav()
    {
        var ease = new QuarticEase { EasingMode = EasingMode.EaseOut };
        var sb = new Storyboard();

        var fade = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(160)), EasingFunction = ease };
        Storyboard.SetTarget(fade, MobileNavBackdrop);
        Storyboard.SetTargetProperty(fade, "Opacity");
        sb.Children.Add(fade);

        var slide = new DoubleAnimation { To = -280, Duration = new Duration(TimeSpan.FromMilliseconds(220)), EasingFunction = ease };
        Storyboard.SetTarget(slide, MobileNavTranslate);
        Storyboard.SetTargetProperty(slide, "X");
        sb.Children.Add(slide);

        sb.Completed += (_, _) => MobileNavHost.Visibility = Visibility.Collapsed;
        sb.Begin();
    }

    private void OnMobileNavBackdropTapped(object sender, RoutedEventArgs e) => CloseMobileNav();

    private void OnMobileNavItemClicked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string regionName })
        {
            // Route through whichever existing TabBar contains the matching region item
            SelectTabBarItem(PrimaryNavTabBar, regionName);
            SelectTabBarItem(LibraryTabBar, regionName);
        }
        CloseMobileNav();
    }

    private static void SelectTabBarItem(Uno.Toolkit.UI.TabBar tabBar, string regionName)
    {
        foreach (var raw in tabBar.Items)
        {
            if (raw is Uno.Toolkit.UI.TabBarItem item
                && Uno.Extensions.Navigation.UI.Region.GetName(item) == regionName)
            {
                tabBar.SelectedItem = item;
                return;
            }
        }
    }

    // §6.1 Keyboard shortcuts: / focuses search, Esc dismisses overlay or clears search
    private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            if (VideoOverlayHost.Visibility == Visibility.Visible)
            {
                VideoPlayer.Close();
                e.Handled = true;
                return;
            }
            if (SearchResultsHost.Visibility == Visibility.Visible)
            {
                ClearSearch();
                e.Handled = true;
                return;
            }
        }

        // "/" focuses search (skip if focus is already in a text input)
        if (e.Key == (Windows.System.VirtualKey)191) // OEM_2 / forward slash on US layout
        {
            if (FocusManager.GetFocusedElement(this.XamlRoot) is TextBox) return;
            SearchTextBox.Focus(FocusState.Programmatic);
            e.Handled = true;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchResultsHost.Visibility =
            string.IsNullOrWhiteSpace(SearchTextBox.Text) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnClearSearchClick(object sender, RoutedEventArgs e) => ClearSearch();

    private void ClearSearch()
    {
        SearchTextBox.Text = string.Empty;
        SearchResultsHost.Visibility = Visibility.Collapsed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!Motion.ReducedMotion)
        {
            // Single bell nudge ~1.2s after first paint to draw attention to the dot
            _ = Dispatcher.RunIdleAsync(_ =>
            {
                var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
                t.Tick += (s, _) => { ((DispatcherTimer)s!).Stop(); ShakeBell(); };
                t.Start();
            });
        }

        HookRouteAnimations();
        HookSidebarPill();

        VideoPlayer.OpenRequested += OnVideoOpenRequested;
        VideoPlayer.CloseRequested += OnVideoCloseRequested;
    }

    // §3.1 Sliding sidebar pill
    private void HookSidebarPill()
    {
        PrimaryNavTabBar.SelectionChanged += OnSidebarSelectionChanged;
        LibraryTabBar.SelectionChanged += OnSidebarSelectionChanged;
        // Initial position once layout settles
        this.Dispatcher.RunIdleAsync(_ => MoveSidebarPillToSelected(animated: false));
    }

    private void OnSidebarSelectionChanged(Uno.Toolkit.UI.TabBar sender, Uno.Toolkit.UI.TabBarSelectionChangedEventArgs e)
    {
        // When one TabBar gets a selection, the other's gets cleared by Region nav.
        // We pick whichever has a SelectedItem.
        MoveSidebarPillToSelected(animated: true);
    }

    private void MoveSidebarPillToSelected(bool animated)
    {
        var selected = (PrimaryNavTabBar.SelectedItem as Microsoft.UI.Xaml.FrameworkElement)
                    ?? (LibraryTabBar.SelectedItem as Microsoft.UI.Xaml.FrameworkElement);

        if (selected is null)
        {
            SidebarPill.Opacity = 0;
            return;
        }

        try
        {
            var transform = selected.TransformToVisual(SidebarTabsHost);
            var pos = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
            var targetY = pos.Y;
            var targetWidth = selected.ActualWidth > 0 ? selected.ActualWidth : SidebarPill.Width;

            SidebarPill.Width = targetWidth;

            if (Motion.ReducedMotion || !animated)
            {
                SidebarPillTranslate.Y = targetY;
                SidebarPill.Opacity = 1;
                return;
            }

            var ease = new QuarticEase { EasingMode = EasingMode.EaseOut };
            var sb = new Storyboard();

            var slide = new DoubleAnimation
            {
                To = targetY,
                Duration = new Duration(TimeSpan.FromMilliseconds(220)),
                EasingFunction = ease,
            };
            Storyboard.SetTarget(slide, SidebarPillTranslate);
            Storyboard.SetTargetProperty(slide, "Y");
            sb.Children.Add(slide);

            if (SidebarPill.Opacity < 1)
            {
                var fade = new DoubleAnimation
                {
                    To = 1,
                    Duration = new Duration(TimeSpan.FromMilliseconds(180)),
                    EasingFunction = ease,
                };
                Storyboard.SetTarget(fade, SidebarPill);
                Storyboard.SetTargetProperty(fade, "Opacity");
                sb.Children.Add(fade);
            }
            sb.Begin();
        }
        catch
        {
            // Layout may not be ready on first call; retry next idle
        }
    }

    // §2.2 Route cross-fade: each routed child fades 0→1, Y -6→0 when its Visibility flips Visible
    private void HookRouteAnimations()
    {
        foreach (var child in ShellContent.Children)
        {
            if (child is FrameworkElement fe)
            {
                fe.RegisterPropertyChangedCallback(VisibilityProperty, OnRouteVisibilityChanged);
                if (fe.RenderTransform is not TranslateTransform)
                {
                    fe.RenderTransform = new TranslateTransform { Y = 0 };
                }
            }
        }
    }

    private void OnRouteVisibilityChanged(DependencyObject d, DependencyProperty prop)
    {
        if (Motion.ReducedMotion) return;
        if (d is not FrameworkElement el) return;
        if (el.Visibility != Visibility.Visible) return;

        var ease = new QuarticEase { EasingMode = EasingMode.EaseOut };
        var translate = el.RenderTransform as TranslateTransform ?? new TranslateTransform();
        el.RenderTransform = translate;
        translate.Y = -4;
        el.Opacity = 0;

        var sb = new Storyboard();
        var fade = new DoubleAnimation
        {
            From = 0, To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
            BeginTime = TimeSpan.FromMilliseconds(40),
            EasingFunction = ease,
        };
        Storyboard.SetTarget(fade, el);
        Storyboard.SetTargetProperty(fade, "Opacity");
        sb.Children.Add(fade);

        var slide = new DoubleAnimation
        {
            From = -4, To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
            BeginTime = TimeSpan.FromMilliseconds(40),
            EasingFunction = ease,
        };
        Storyboard.SetTarget(slide, translate);
        Storyboard.SetTargetProperty(slide, "Y");
        sb.Children.Add(slide);

        sb.Begin();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        VideoPlayer.OpenRequested -= OnVideoOpenRequested;
        VideoPlayer.CloseRequested -= OnVideoCloseRequested;

        if (PrimaryNavTabBar != null) PrimaryNavTabBar.SelectionChanged -= OnSidebarSelectionChanged;
        if (LibraryTabBar != null) LibraryTabBar.SelectionChanged -= OnSidebarSelectionChanged;
    }

    private VideoPlayer.VideoOpenRequest? _currentVideo;

    private void OnVideoOpenRequested(VideoPlayer.VideoOpenRequest req)
    {
        _currentVideo = req;
        OverlayTitle.Text = req.Title;
        OverlayChannel.Text = req.ChannelName;

        VideoOverlayHost.Visibility = Visibility.Visible;

        // 1) Run entrance animation IMMEDIATELY so the modal is visible
        PlayOverlayEntrance();

#if HAS_UNO_SKIA
        // Skia desktop: WebView2 doesn't render YouTube embed reliably.
        // Show poster + "Watch on YouTube" external launcher fallback.
        OverlayWebView.Visibility = Visibility.Collapsed;
        ExternalPlayerPanel.Visibility = Visibility.Visible;
        try { PosterImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(req.PosterUrl)); }
        catch { /* missing poster is fine */ }
#else
        OverlayWebView.Visibility = Visibility.Visible;
        ExternalPlayerPanel.Visibility = Visibility.Collapsed;
        // 2) Initialize WebView2 in background (don't block the UI)
        _ = LoadVideoIntoWebViewAsync(req);
#endif
    }

    private async void OnWatchExternalClick(object sender, RoutedEventArgs e)
    {
        if (_currentVideo is null) return;
        try
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(_currentVideo.WatchUrl));
            VideoPlayer.Close();
        }
        catch
        {
            // Launch failed; leave modal open
        }
    }

    private void PlayOverlayEntrance()
    {
        if (Motion.ReducedMotion)
        {
            OverlayBackdrop.Opacity = 1;
            OverlayCard.Opacity = 1;
            OverlayCardTransform.ScaleX = 1;
            OverlayCardTransform.ScaleY = 1;
            ShellContent.Opacity = 0.35;
            return;
        }

        var sb = new Storyboard();
        var ease = new QuarticEase { EasingMode = EasingMode.EaseOut };

        // Backdrop fades in first — sets the stage
        var fadeBackdrop = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(120)), EasingFunction = ease };
        Storyboard.SetTarget(fadeBackdrop, OverlayBackdrop);
        Storyboard.SetTargetProperty(fadeBackdrop, "Opacity");
        sb.Children.Add(fadeBackdrop);

        var dimContent = new DoubleAnimation { To = 0.35, Duration = new Duration(TimeSpan.FromMilliseconds(120)), EasingFunction = ease };
        Storyboard.SetTarget(dimContent, ShellContent);
        Storyboard.SetTargetProperty(dimContent, "Opacity");
        sb.Children.Add(dimContent);

        // Card lands ~60ms later, no bounce
        var cardDelay = TimeSpan.FromMilliseconds(60);
        var fadeCard = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = ease, BeginTime = cardDelay };
        Storyboard.SetTarget(fadeCard, OverlayCard);
        Storyboard.SetTargetProperty(fadeCard, "Opacity");
        sb.Children.Add(fadeCard);

        var scaleX = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = ease, BeginTime = cardDelay };
        Storyboard.SetTarget(scaleX, OverlayCardTransform);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");
        sb.Children.Add(scaleX);

        var scaleY = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = ease, BeginTime = cardDelay };
        Storyboard.SetTarget(scaleY, OverlayCardTransform);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");
        sb.Children.Add(scaleY);

        sb.Begin();
    }

    private async System.Threading.Tasks.Task LoadVideoIntoWebViewAsync(VideoPlayer.VideoOpenRequest req)
    {
        try
        {
            await OverlayWebView.EnsureCoreWebView2Async();
            var html = $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<style>
  html, body {{ margin:0; padding:0; background:#000; height:100%; overflow:hidden; }}
  .stage {{ position:relative; width:100%; height:100%; }}
  iframe {{ border:0; width:100%; height:100%; display:block; }}

  .curtain {{
    position:absolute; top:0; bottom:0; width:50%;
    pointer-events:none; z-index:10;
    will-change:transform;
    transform:translateX(0);
    transition:transform 700ms cubic-bezier(0.23, 1, 0.32, 1) 220ms;
    box-shadow: inset 0 0 60px rgba(0,0,0,0.55);
  }}
  .curtain-left {{
    left:0;
    background: linear-gradient(90deg, #0A1D2A 0%, #142E3A 60%, #1B3F4F 100%);
    border-right: 1px solid rgba(0,0,0,0.4);
  }}
  .curtain-right {{
    right:0;
    background: linear-gradient(270deg, #0A1D2A 0%, #142E3A 60%, #1B3F4F 100%);
    border-left: 1px solid rgba(0,0,0,0.4);
  }}
  .reveal .curtain-left {{ transform:translateX(-100%); }}
  .reveal .curtain-right {{ transform:translateX(100%); }}
</style>
</head>
<body>
  <div class='stage' id='stage'>
    <iframe src='https://www.youtube.com/embed/{req.YouTubeId}?autoplay=1&rel=0&modestbranding=1'
            allow='autoplay; encrypted-media; picture-in-picture; fullscreen'
            allowfullscreen></iframe>
    <div class='curtain curtain-left'></div>
    <div class='curtain curtain-right'></div>
  </div>
  <script>
    requestAnimationFrame(() => {{
      document.getElementById('stage').classList.add('reveal');
    }});
  </script>
</body>
</html>";
            OverlayWebView.NavigateToString(html);
        }
        catch
        {
            // WebView2 init failed
        }
    }

    private void OnVideoCloseRequested()
    {
        var sb = new Storyboard();
        var ease = new QuarticEase { EasingMode = EasingMode.EaseOut };

        var fadeBackdrop = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(140)), EasingFunction = ease };
        Storyboard.SetTarget(fadeBackdrop, OverlayBackdrop);
        Storyboard.SetTargetProperty(fadeBackdrop, "Opacity");
        sb.Children.Add(fadeBackdrop);

        var fadeCard = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(140)), EasingFunction = ease };
        Storyboard.SetTarget(fadeCard, OverlayCard);
        Storyboard.SetTargetProperty(fadeCard, "Opacity");
        sb.Children.Add(fadeCard);

        var scaleX = new DoubleAnimation { To = 0.96, Duration = new Duration(TimeSpan.FromMilliseconds(140)), EasingFunction = ease };
        Storyboard.SetTarget(scaleX, OverlayCardTransform);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");
        sb.Children.Add(scaleX);

        var scaleY = new DoubleAnimation { To = 0.96, Duration = new Duration(TimeSpan.FromMilliseconds(140)), EasingFunction = ease };
        Storyboard.SetTarget(scaleY, OverlayCardTransform);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");
        sb.Children.Add(scaleY);

        var restoreContent = new DoubleAnimation { To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(140)), EasingFunction = ease };
        Storyboard.SetTarget(restoreContent, ShellContent);
        Storyboard.SetTargetProperty(restoreContent, "Opacity");
        sb.Children.Add(restoreContent);

        sb.Completed += (_, _) =>
        {
            VideoOverlayHost.Visibility = Visibility.Collapsed;
            try { OverlayWebView.Source = new Uri("about:blank"); } catch { /* ignored */ }
        };
        sb.Begin();
    }

    private void OnCloseOverlayClick(object sender, RoutedEventArgs e) => VideoPlayer.Close();

    private void OnBackdropTapped(object sender, TappedRoutedEventArgs e) => VideoPlayer.Close();


    // Search field focus glow (§3.2)
    private void OnSearchFocus(object sender, RoutedEventArgs e)
    {
        AnimateOpacity(SearchGlow, 1, 250);
        AnimateOpacity(SearchIcon, 1, 250);
    }

    private void OnSearchBlur(object sender, RoutedEventArgs e)
    {
        AnimateOpacity(SearchGlow, 0, 200);
        AnimateOpacity(SearchIcon, 0.7, 200);
    }

    private static void AnimateOpacity(UIElement target, double to, double durationMs)
    {
        var anim = new DoubleAnimation
        {
            To = to,
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, "Opacity");
        new Storyboard { Children = { anim } }.Begin();
    }

    // Bell shake on close (§4.3) — shake on every click for clear feedback
    private bool _notifPanelOpen;
    private void OnBellClicked(object sender, RoutedEventArgs e)
    {
        _notifPanelOpen = !_notifPanelOpen;
        if (!_notifPanelOpen)
            ShakeBell();
    }

    private void ShakeBell()
    {
        var anim = new DoubleAnimationUsingKeyFrames
        {
            Duration = new Duration(TimeSpan.FromMilliseconds(600)),
        };
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 0, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(100), Value = 12, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(220), Value = -10, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(340), Value = 6, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(460), Value = -4, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(600), Value = 0, EasingFunction = ease });
        Storyboard.SetTarget(anim, BellRotate);
        Storyboard.SetTargetProperty(anim, "Angle");
        new Storyboard { Children = { anim } }.Begin();
    }

}
