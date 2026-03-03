using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace WinampClassic.Controls;

public sealed partial class EqualizerPanel : UserControl
{
    // Cached brushes to avoid repeated allocations
    private static readonly SolidColorBrush ActiveButtonBrush = new(Microsoft.UI.ColorHelper.FromArgb(255, 68, 170, 255));
    private static readonly SolidColorBrush ActiveTextBrush = new(Microsoft.UI.Colors.White);
    private static readonly SolidColorBrush ActiveIndicatorBrush = new(Microsoft.UI.ColorHelper.FromArgb(255, 68, 255, 136));
    private static readonly SolidColorBrush InactiveButtonBrush = new(Microsoft.UI.ColorHelper.FromArgb(255, 200, 202, 208));
    private static readonly SolidColorBrush InactiveTextBrush = new(Microsoft.UI.ColorHelper.FromArgb(255, 32, 32, 48));
    private static readonly SolidColorBrush InactiveIndicatorBrush = new(Microsoft.UI.ColorHelper.FromArgb(255, 64, 80, 96));

    private bool _isEnabled;
    private bool _isAutoEnabled;
    private readonly double[] _bandValues = new double[10];
    private Grid? _activeDragTrack;
    private Border[]? _thumbs;
    private Grid[]? _tracks;

    public event EventHandler? CloseRequested;
    public event EventHandler<double[]>? BandValuesChanged;

    public EqualizerPanel()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _thumbs = [Band1Thumb, Band2Thumb, Band3Thumb, Band4Thumb, Band5Thumb,
                   Band6Thumb, Band7Thumb, Band8Thumb, Band9Thumb, Band10Thumb];
        _tracks = [Band1Track, Band2Track, Band3Track, Band4Track, Band5Track,
                   Band6Track, Band7Track, Band8Track, Band9Track, Band10Track];
    }

    private void OnToggleEnabled(object sender, TappedRoutedEventArgs e)
    {
        _isEnabled = !_isEnabled;
        UpdateOnButtonState();
    }

    private void OnToggleAuto(object sender, TappedRoutedEventArgs e)
    {
        _isAutoEnabled = !_isAutoEnabled;
        UpdateAutoButtonState();
    }

    private void UpdateOnButtonState()
    {
        if (_isEnabled)
        {
            OnButtonBorder.Background = ActiveButtonBrush;
            OnButtonText.Foreground = ActiveTextBrush;
            OnIndicator.Fill = ActiveIndicatorBrush;
        }
        else
        {
            OnButtonBorder.Background = InactiveButtonBrush;
            OnButtonText.Foreground = InactiveTextBrush;
            OnIndicator.Fill = InactiveIndicatorBrush;
        }
    }

    private void UpdateAutoButtonState()
    {
        if (_isAutoEnabled)
        {
            AutoButtonBorder.Background = ActiveButtonBrush;
            AutoButtonText.Foreground = ActiveTextBrush;
            AutoIndicator.Fill = ActiveIndicatorBrush;
        }
        else
        {
            AutoButtonBorder.Background = InactiveButtonBrush;
            AutoButtonText.Foreground = InactiveTextBrush;
            AutoIndicator.Fill = InactiveIndicatorBrush;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void OnBandPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid track)
        {
            _activeDragTrack = track;
            track.CapturePointer(e.Pointer);
            UpdateThumbPosition(track, e);
        }
    }

    private void OnBandPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_activeDragTrack != null && sender == _activeDragTrack)
        {
            UpdateThumbPosition(_activeDragTrack, e);
        }
    }

    private void OnBandPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid track)
        {
            track.ReleasePointerCapture(e.Pointer);
            _activeDragTrack = null;
        }
    }

    private void UpdateThumbPosition(Grid track, PointerRoutedEventArgs e)
    {
        if (_tracks == null || _thumbs == null) return;

        int bandIndex = Array.IndexOf(_tracks, track);
        if (bandIndex < 0) return;

        var position = e.GetCurrentPoint(track).Position;
        double trackHeight = track.ActualHeight;
        const double thumbHeight = 18;
        double usableHeight = trackHeight - thumbHeight;

        double clampedY = Math.Max(thumbHeight / 2, Math.Min(position.Y, trackHeight - thumbHeight / 2));

        double normalizedPosition = (clampedY - thumbHeight / 2) / usableHeight;
        double value = 12 - (normalizedPosition * 24);
        _bandValues[bandIndex] = Math.Round(value, 1);

        double thumbOffset = (clampedY - thumbHeight / 2) - (usableHeight / 2);
        var thumb = _thumbs[bandIndex];
        thumb.VerticalAlignment = VerticalAlignment.Center;
        thumb.Margin = new Thickness(0, thumbOffset * 2, 0, 0);

        BandValuesChanged?.Invoke(this, _bandValues);
    }

    public double[] GetBandValues() => _bandValues;
}
