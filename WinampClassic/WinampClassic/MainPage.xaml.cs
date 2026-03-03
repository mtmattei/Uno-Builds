using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Storage;
using WinampClassic.ViewModels;

namespace WinampClassic;

public sealed partial class MainPage : Page
{
    private static readonly SolidColorBrush WhiteBrush = new(Microsoft.UI.Colors.White);

    private PlayerViewModel? _viewModel;
    private DispatcherTimer? _animationTimer;
    private double _tickerPosition;
    private double _tickerTextWidth;
    private readonly Random _random = new();
    private bool _isSeeking;
    private bool _isEqualizerVisible;
    private ScaleTransform[]? _spectrumBarTransforms;
    private bool _isDraggingWindow;
    private Windows.Foundation.Point _dragStartPoint;

    // Tick counter to alternate update rates (spectrum+ticker every 50ms, UI every 100ms)
    private int _tickCount;

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
        this.DragOver += OnDragOver;
        this.Drop += OnDrop;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel = App.PlayerViewModel;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        InitializeSpectrumBars();
        InitializeTickerAnimation();
        StartAnimationTimer();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _animationTimer?.Stop();
        _animationTimer = null;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void InitializeSpectrumBars()
    {
        const int barCount = 24;
        const double maxBarHeight = 50;
        var bars = new List<Border>(barCount);
        _spectrumBarTransforms = new ScaleTransform[barCount];

        for (int i = 0; i < barCount; i++)
        {
            var scaleTransform = new ScaleTransform { ScaleY = 0.04 };
            _spectrumBarTransforms[i] = scaleTransform;

            var bar = new Border
            {
                Width = 5,
                Height = maxBarHeight,
                Background = WhiteBrush,
                VerticalAlignment = VerticalAlignment.Bottom,
                RenderTransform = scaleTransform,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 1.0)
            };
            bars.Add(bar);
        }
        SpectrumBars.ItemsSource = bars;
    }

    private void InitializeTickerAnimation()
    {
        _tickerPosition = 0;
        SongTitleTicker.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
        _tickerTextWidth = SongTitleTicker.DesiredSize.Width;
    }

    private void StartAnimationTimer()
    {
        _tickCount = 0;
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTimerTick;
        _animationTimer.Start();
    }

    /// <summary>
    /// Single consolidated timer: spectrum + ticker every tick (50ms), UI updates every other tick (100ms).
    /// </summary>
    private void OnAnimationTimerTick(object? sender, object e)
    {
        UpdateSpectrum();
        UpdateTicker();

        _tickCount++;
        if (_tickCount % 2 == 0)
        {
            UpdateUI();
        }
    }

    private void UpdateSpectrum()
    {
        if (_spectrumBarTransforms == null) return;

        bool isPlaying = _viewModel?.IsPlaying ?? false;
        const double maxBarHeight = 50.0;

        for (int i = 0; i < _spectrumBarTransforms.Length; i++)
        {
            var transform = _spectrumBarTransforms[i];
            double targetHeight = isPlaying
                ? _random.NextDouble() * 45 + 4
                : _random.NextDouble() * 12 + 3;

            // Smooth interpolation via ScaleY (no layout invalidation)
            transform.ScaleY = transform.ScaleY * 0.6 + (targetHeight / maxBarHeight) * 0.4;
        }
    }

    private void UpdateTicker()
    {
        if (TickerCanvas.ActualWidth > 0 && _tickerTextWidth > 0)
        {
            _tickerPosition -= 1.5;

            if (_tickerPosition < -_tickerTextWidth - 50)
            {
                _tickerPosition = TickerCanvas.ActualWidth;
            }

            TickerTransform.X = _tickerPosition;
        }
    }

    private void UpdateUI()
    {
        if (_viewModel == null) return;

        TimeDisplay.Text = _viewModel.TimeDisplay;
        PlaybackStatusIcon.Text = _viewModel.PlaybackStatusIcon;

        if (!_isSeeking && SeekSlider.ActualWidth > 0)
        {
            SeekSlider.Value = _viewModel.SeekPosition;
            UpdateSeekBarVisuals(_viewModel.SeekPosition);
        }

        if (_viewModel.CurrentTrack != null)
        {
            BitrateDisplay.Text = $"KBPS: {_viewModel.CurrentTrack.BitrateDisplay}";
            SampleRateDisplay.Text = $"KHZ: {_viewModel.CurrentTrack.SampleRateDisplay}";
            ChannelDisplay.Text = _viewModel.CurrentTrack.ChannelModeDisplay;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.CurrentTrack) && _viewModel?.CurrentTrack != null)
        {
            SongTitleTicker.Text = _viewModel.CurrentTrack.DisplayName.ToUpperInvariant();
            SongTitleTicker.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
            _tickerTextWidth = SongTitleTicker.DesiredSize.Width;
            _tickerPosition = TickerCanvas.ActualWidth;
        }
    }

    private void OnTitleBarPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            _isDraggingWindow = true;
            _dragStartPoint = e.GetCurrentPoint(null).Position;
            element.CapturePointer(e.Pointer);
        }
    }

    private void OnTitleBarPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDraggingWindow) return;

        var appWindow = App.MainWindow?.AppWindow;
        if (appWindow == null) return;

        var currentPoint = e.GetCurrentPoint(null).Position;
        var deltaX = currentPoint.X - _dragStartPoint.X;
        var deltaY = currentPoint.Y - _dragStartPoint.Y;

        var pos = appWindow.Position;
        appWindow.Move(new PointInt32 { X = pos.X + (int)deltaX, Y = pos.Y + (int)deltaY });
    }

    private void OnTitleBarPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement element && _isDraggingWindow)
        {
            _isDraggingWindow = false;
            element.ReleasePointerCapture(e.Pointer);
        }
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e)
    {
        if (App.MainWindow?.AppWindow?.Presenter is OverlappedPresenter presenter)
        {
            presenter.Minimize();
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => App.MainWindow?.Close();

    // Transport handlers - unified for both Click (Button) and Tapped (Grid) events
    private async void OnOpenFileClick(object sender, RoutedEventArgs e) => await OpenFileAsync();
    private async void OnOpenFileTapped(object sender, TappedRoutedEventArgs e) => await OpenFileAsync();
    private async Task OpenFileAsync()
    {
        if (_viewModel != null)
        {
            await _viewModel.OpenFileCommand.ExecuteAsync(null);
        }
    }

    private void OnPlayClick(object sender, RoutedEventArgs e) => _viewModel?.PlayCommand.Execute(null);
    private void OnPlayTapped(object sender, TappedRoutedEventArgs e) => _viewModel?.PlayCommand.Execute(null);

    private void OnPauseTapped(object sender, TappedRoutedEventArgs e) => _viewModel?.PauseCommand.Execute(null);

    private void OnStopTapped(object sender, TappedRoutedEventArgs e) => _viewModel?.StopCommand.Execute(null);

    private void OnPreviousTapped(object sender, TappedRoutedEventArgs e) => _viewModel?.PreviousCommand.Execute(null);

    private void OnNextTapped(object sender, TappedRoutedEventArgs e) => _viewModel?.NextCommand.Execute(null);

    private void OnTimeDisplayTapped(object sender, TappedRoutedEventArgs e) =>
        _viewModel?.ToggleTimeDisplayCommand.Execute(null);

    private void OnSeekValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_isSeeking && SeekSlider.ActualWidth > 0)
        {
            UpdateSeekBarVisuals(e.NewValue);
        }
    }

    private void UpdateSeekBarVisuals(double percentage)
    {
        if (SeekSlider.ActualWidth <= 0) return;

        var trackWidth = SeekSlider.ActualWidth;
        var thumbWidth = SeekThumb.Width;
        var progressWidth = trackWidth * (percentage / 100.0);

        SeekProgress.Width = progressWidth;

        var thumbPosition = (trackWidth - thumbWidth) * (percentage / 100.0);
        Canvas.SetLeft(SeekThumb, thumbPosition);
        Canvas.SetLeft(SeekThumbShadow, thumbPosition + 1);
    }

    private void OnSeekPointerPressed(object sender, PointerRoutedEventArgs e) => _isSeeking = true;

    private void OnSeekPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isSeeking = false;
        _viewModel?.SeekToPosition(SeekSlider.Value);
    }

    private void OnVolumeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.Volume = e.NewValue / 100.0;
        }
        UpdateVolumeBarVisuals(e.NewValue);
    }

    private void UpdateVolumeBarVisuals(double percentage)
    {
        if (VolumeSlider.ActualWidth <= 0) return;

        const double trackWidth = 110.0;
        const double thumbWidth = 10.0;
        var progressWidth = (trackWidth - 4) * (percentage / 100.0);

        VolumeProgress.Width = progressWidth;

        var thumbPosition = (trackWidth - thumbWidth) * (percentage / 100.0);
        Canvas.SetLeft(VolumeThumb, thumbPosition);
        Canvas.SetLeft(VolumeThumbShadow, thumbPosition + 1);
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Space:
                _viewModel?.PlayPauseCommand.Execute(null);
                break;
            case Windows.System.VirtualKey.X:
                _viewModel?.PlayCommand.Execute(null);
                break;
            case Windows.System.VirtualKey.C:
                _viewModel?.PauseCommand.Execute(null);
                break;
            case Windows.System.VirtualKey.V:
                _viewModel?.StopCommand.Execute(null);
                break;
            case Windows.System.VirtualKey.Z:
                _viewModel?.PreviousCommand.Execute(null);
                break;
            case Windows.System.VirtualKey.B:
                _viewModel?.NextCommand.Execute(null);
                break;
            case Windows.System.VirtualKey.Up:
                _viewModel?.VolumeUp();
                VolumeSlider.Value = (_viewModel?.Volume ?? 0.75) * 100;
                break;
            case Windows.System.VirtualKey.Down:
                _viewModel?.VolumeDown();
                VolumeSlider.Value = (_viewModel?.Volume ?? 0.75) * 100;
                break;
            case Windows.System.VirtualKey.Left:
                _viewModel?.SeekBackwardCommand.Execute(null);
                break;
            case Windows.System.VirtualKey.Right:
                _viewModel?.SeekForwardCommand.Execute(null);
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        e.DragUIOverride.Caption = "Drop to play";
        e.DragUIOverride.IsCaptionVisible = true;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            return;

        var items = await e.DataView.GetStorageItemsAsync();
        if (items.Count > 0 && items[0] is StorageFile file && _viewModel != null)
        {
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (extension is ".mp3" or ".wav" or ".flac" or ".m4a" or ".ogg")
            {
                await _viewModel.LoadFileAsync(file);
                _viewModel.PlayCommand.Execute(null);
            }
        }
    }

    private const int BaseWindowHeight = 300;
    private const int EqPanelHeight = 220;

    private void OnConfigClick(object sender, TappedRoutedEventArgs e)
    {
        _isEqualizerVisible = !_isEqualizerVisible;

        if (_isEqualizerVisible)
        {
            // Realize the deferred EQ panel
            FindName("EqualizerPanel");
        }

        if (EqualizerPanel != null)
        {
            EqualizerPanel.Visibility = _isEqualizerVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        EqNotch.Visibility = _isEqualizerVisible ? Visibility.Visible : Visibility.Collapsed;
        ConfigArrowRotation.Angle = _isEqualizerVisible ? -155 : 25;
        ResizeWindowForEqualizer(_isEqualizerVisible);
    }

    private void OnEqualizerCloseRequested(object? sender, EventArgs e)
    {
        _isEqualizerVisible = false;
        if (EqualizerPanel != null)
        {
            EqualizerPanel.Visibility = Visibility.Collapsed;
        }
        EqNotch.Visibility = Visibility.Collapsed;
        ConfigArrowRotation.Angle = 25;
        ResizeWindowForEqualizer(false);
    }

    private void ResizeWindowForEqualizer(bool showEqualizer)
    {
        var appWindow = App.MainWindow?.AppWindow;
        if (appWindow != null)
        {
            var newHeight = showEqualizer ? BaseWindowHeight + EqPanelHeight : BaseWindowHeight;
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 620, Height = newHeight });
        }
    }

    private void OnTransportButtonPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid button)
        {
            double existingAngle = 0;
            if (button.RenderTransform is RotateTransform rt)
            {
                existingAngle = rt.Angle;
            }

            var transform = new TransformGroup();
            transform.Children.Add(new ScaleTransform { ScaleX = 0.92, ScaleY = 0.92 });
            if (existingAngle != 0)
            {
                transform.Children.Add(new RotateTransform { Angle = existingAngle });
            }

            button.RenderTransform = transform;
            button.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
            button.Opacity = 0.85;
        }
    }

    private void OnTransportButtonReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid button)
        {
            if (button.Name == "PrevButton")
            {
                button.RenderTransform = new RotateTransform { Angle = 25 };
            }
            else if (button.Name == "NextButton")
            {
                button.RenderTransform = new RotateTransform { Angle = -25 };
            }
            else
            {
                button.RenderTransform = null;
            }
            button.Opacity = 1.0;
        }
    }
}
