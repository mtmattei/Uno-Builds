using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace PhosphorProtocol.Controls;

public sealed partial class BatteryIndicator : UserControl
{
    private static readonly Windows.UI.Color OffColor = Windows.UI.Color.FromArgb(255, 5, 18, 18);
    private static readonly Windows.UI.Color AmberColor = Windows.UI.Color.FromArgb(255, 212, 168, 50);
    private static readonly Windows.UI.Color GlowColor = Windows.UI.Color.FromArgb(255, 36, 112, 112);
    private static readonly Windows.UI.Color BrightColor = Windows.UI.Color.FromArgb(255, 58, 171, 166);
    private static readonly Windows.UI.Color PeakColor = Windows.UI.Color.FromArgb(255, 111, 252, 246);
    private static readonly Windows.UI.Color DimColor = Windows.UI.Color.FromArgb(255, 20, 56, 56);

    private readonly DispatcherTimer _blinkTimer;
    private bool _blinkState;
    private readonly List<Rectangle> _segments = new();

    public static readonly DependencyProperty PercentProperty =
        DependencyProperty.Register(nameof(Percent), typeof(int), typeof(BatteryIndicator),
            new PropertyMetadata(0, OnPercentChanged));

    public static readonly DependencyProperty SegmentCountProperty =
        DependencyProperty.Register(nameof(SegmentCount), typeof(int), typeof(BatteryIndicator),
            new PropertyMetadata(20, OnSegmentCountChanged));

    public static readonly DependencyProperty IsChargingProperty =
        DependencyProperty.Register(nameof(IsCharging), typeof(bool), typeof(BatteryIndicator),
            new PropertyMetadata(false, OnIsChargingChanged));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(BatteryIndicator),
            new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

    public static readonly DependencyProperty ShowGlowProperty =
        DependencyProperty.Register(nameof(ShowGlow), typeof(bool), typeof(BatteryIndicator),
            new PropertyMetadata(false, OnPercentChanged));

    public BatteryIndicator()
    {
        this.InitializeComponent();
        _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _blinkTimer.Tick += (_, _) =>
        {
            _blinkState = !_blinkState;
            UpdateSegments();
        };
        Loaded += (_, _) =>
        {
            BuildSegments();
            // Defer resize to after layout pass
            DispatcherQueue.TryEnqueue(() => ResizeSegments());
        };
        SizeChanged += (_, _) => ResizeSegments();
        SegmentPanel.SizeChanged += (_, _) => ResizeSegments();
    }

    public int Percent { get => (int)GetValue(PercentProperty); set => SetValue(PercentProperty, value); }
    public int SegmentCount { get => (int)GetValue(SegmentCountProperty); set => SetValue(SegmentCountProperty, value); }
    public bool IsCharging { get => (bool)GetValue(IsChargingProperty); set => SetValue(IsChargingProperty, value); }
    public Orientation Orientation { get => (Orientation)GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public bool ShowGlow { get => (bool)GetValue(ShowGlowProperty); set => SetValue(ShowGlowProperty, value); }

    private static void OnPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { if (d is BatteryIndicator bi) bi.UpdateSegments(); }
    private static void OnSegmentCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { if (d is BatteryIndicator bi) bi.BuildSegments(); }
    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { if (d is BatteryIndicator bi) bi.BuildSegments(); }
    private static void OnIsChargingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BatteryIndicator bi)
        {
            if ((bool)e.NewValue) bi._blinkTimer.Start();
            else { bi._blinkTimer.Stop(); bi.UpdateSegments(); }
        }
    }

    private void BuildSegments()
    {
        SegmentPanel.Children.Clear();
        _segments.Clear();

        bool isVertical = Orientation == Orientation.Vertical;
        SegmentPanel.Orientation = isVertical ? Orientation.Vertical : Orientation.Horizontal;

        for (int i = 0; i < SegmentCount; i++)
        {
            var segment = new Rectangle
            {
                Width = isVertical ? 32 : 5,
                Height = isVertical ? 4 : 12,
                RadiusX = 1,
                RadiusY = 1,
                Fill = new SolidColorBrush(OffColor)
            };

            _segments.Add(segment);
            if (isVertical)
                SegmentPanel.Children.Insert(0, segment);
            else
                SegmentPanel.Children.Add(segment);
        }

        if (!isVertical)
        {
            var cap = new Rectangle
            {
                Width = 3, Height = 7,
                RadiusX = 0, RadiusY = 0,
                Fill = new SolidColorBrush(DimColor)
            };
            SegmentPanel.Children.Add(cap);
        }

        UpdateSegments();
    }

    private void ResizeSegments()
    {
        if (_segments.Count == 0 || Orientation != Orientation.Vertical) return;

        // Use the root grid's actual height which stretches to fill the parent
        double availableHeight = RootGrid.ActualHeight;
        if (availableHeight <= 0) return;

        double spacing = SegmentPanel.Spacing;
        double totalSpacing = spacing * (_segments.Count - 1);
        double segmentHeight = Math.Max(2, Math.Floor((availableHeight - totalSpacing) / _segments.Count));

        foreach (var seg in _segments)
            seg.Height = segmentHeight;
    }

    private void UpdateSegments()
    {
        if (_segments.Count == 0) return;

        int filledCount = (int)Math.Round(Percent / 100.0 * SegmentCount);

        for (int i = 0; i < _segments.Count; i++)
        {
            Windows.UI.Color color;
            if (i < filledCount)
            {
                double ratio = (double)i / SegmentCount;
                if (ratio < 0.2) color = AmberColor;
                else if (ratio < 0.4) color = GlowColor;
                else color = BrightColor;
            }
            else if (IsCharging && i == filledCount && _blinkState)
            {
                color = DimColor;
            }
            else
            {
                color = OffColor;
            }
            _segments[i].Fill = new SolidColorBrush(color);
        }

        if (ShowGlow) UpdateGlow();
    }

    private void UpdateGlow()
    {
        double ratio = Percent / 100.0;
        Windows.UI.Color glowColor;
        if (ratio < 0.2) glowColor = AmberColor;
        else if (ratio < 0.5) glowColor = GlowColor;
        else glowColor = PeakColor;

        byte alpha = (byte)(ratio * 80);
        glowColor = Windows.UI.Color.FromArgb(alpha, glowColor.R, glowColor.G, glowColor.B);
        GlowBorder.Background = new SolidColorBrush(glowColor);
        GlowBorder.Visibility = Visibility.Visible;
    }
}
