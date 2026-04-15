using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace PhosphorProtocol.Controls;

public sealed partial class ScanLineOverlay : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    private double _offset;
    private double _flickerBandOffset;
    private int _tickCount;
    private const double LineSpacing = 4.0;
    private const double LineHeight = 2.0;
    private const double FlickerBandHeight = 8.0;
    private const double FlickerBandSpacing = 80.0;

    public ScanLineOverlay()
    {
        this.InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += OnTick;
        SizeChanged += OnSizeChanged;
        Loaded += (_, _) => _timer.Start();
        Unloaded += (_, _) => _timer.Stop();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        GenerateLines();
    }

    private void GenerateLines()
    {
        ScanCanvas.Children.Clear();
        if (ActualHeight <= 0 || ActualWidth <= 0)
        {
            return;
        }

        // Generate enough lines to cover the control plus one extra cycle for seamless scrolling
        var totalHeight = ActualHeight + LineSpacing * 2;
        for (var y = -LineSpacing; y < totalHeight; y += LineSpacing)
        {
            var line = new Rectangle
            {
                Width = ActualWidth,
                Height = LineHeight,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(15, 0, 0, 0)),
                IsHitTestVisible = false,
            };
            Canvas.SetTop(line, y);
            ScanCanvas.Children.Add(line);
        }

        // Generate brightness flicker bands — wider, subtler rectangles that scroll faster
        var flickerTotalHeight = ActualHeight + FlickerBandSpacing * 2;
        for (var y = -FlickerBandSpacing; y < flickerTotalHeight; y += FlickerBandSpacing)
        {
            var band = new Rectangle
            {
                Width = ActualWidth,
                Height = FlickerBandHeight,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(8, 0, 0, 0)),
                IsHitTestVisible = false,
                Tag = "FlickerBand",
            };
            Canvas.SetTop(band, y);
            ScanCanvas.Children.Add(band);
        }

        ScanCanvas.RenderTransform = new TranslateTransform();
    }

    private void OnTick(object? sender, object e)
    {
        _tickCount++;

        // Scroll scanlines
        _offset -= 0.8;
        if (_offset <= -LineSpacing)
        {
            _offset += LineSpacing;
        }

        // Scroll flicker bands faster for CRT refresh artifact effect
        _flickerBandOffset -= 2.4;
        if (_flickerBandOffset <= -FlickerBandSpacing)
        {
            _flickerBandOffset += FlickerBandSpacing;
        }

        // Apply scanline scroll via canvas transform
        if (ScanCanvas.RenderTransform is TranslateTransform transform)
        {
            transform.Y = _offset;
        }

        // Move flicker bands independently at their own speed
        foreach (var child in ScanCanvas.Children)
        {
            if (child is Rectangle rect && rect.Tag is "FlickerBand")
            {
                rect.RenderTransform ??= new TranslateTransform();
                if (rect.RenderTransform is TranslateTransform bandTransform)
                {
                    bandTransform.Y = _flickerBandOffset - _offset; // compensate for canvas transform
                }
            }
        }

        // Phosphor instability flicker — every ~60 ticks, briefly pulse the overlay opacity
        if (_tickCount % 60 == 0 && _random.NextDouble() < 0.7)
        {
            Opacity = 0.6 + _random.NextDouble() * 0.3; // drop to 0.6–0.9
        }
        else if (_tickCount % 60 == 2)
        {
            Opacity = 1.0; // restore after 2 ticks (~100ms)
        }
    }
}
