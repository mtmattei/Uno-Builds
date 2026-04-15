namespace ClaudeDash.Controls;

public sealed partial class InteractiveBarChart : UserControl
{
    private static readonly FontFamily MonoFont = new("JetBrains Mono, Cascadia Code, Consolas, monospace");

    public static readonly DependencyProperty BarColorProperty =
        DependencyProperty.Register(nameof(BarColor), typeof(Color), typeof(InteractiveBarChart),
            new PropertyMetadata(ColorHelper.FromArgb(255, 176, 176, 176)));

    public static readonly DependencyProperty ChartHeightProperty =
        DependencyProperty.Register(nameof(ChartHeight), typeof(double), typeof(InteractiveBarChart),
            new PropertyMetadata(120.0));

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    public double ChartHeight
    {
        get => (double)GetValue(ChartHeightProperty);
        set => SetValue(ChartHeightProperty, value);
    }

    private List<ActivityDataPoint>? _currentData;
    private List<HourlyActivity>? _currentHourlyData;

    public InteractiveBarChart()
    {
        this.InitializeComponent();
        this.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_currentData != null)
            RenderBars(_currentData);
        else if (_currentHourlyData != null)
            RenderHourlyBars(_currentHourlyData);
    }

    public void SetData(IEnumerable<ActivityDataPoint> data)
    {
        _currentData = data.ToList();
        _currentHourlyData = null;
        RenderBars(_currentData);
    }

    public void SetHourlyData(IEnumerable<HourlyActivity> data)
    {
        _currentHourlyData = data.ToList();
        _currentData = null;
        RenderHourlyBars(_currentHourlyData);
    }

    private void RenderBars(List<ActivityDataPoint> data)
    {
        ChartCanvas.Children.Clear();
        XAxisPanel.Children.Clear();
        YAxisPanel.Children.Clear();

        if (data.Count == 0) return;

        var canvasWidth = ChartCanvas.ActualWidth;
        var canvasHeight = ChartCanvas.ActualHeight;
        if (canvasWidth <= 0 || canvasHeight <= 0) return;

        var maxValue = data.Max(d => d.MaxValue);
        if (maxValue <= 0) maxValue = 1;

        var gap = 2.0;
        var totalGaps = (data.Count - 1) * gap;
        var barWidth = Math.Max((canvasWidth - totalGaps) / data.Count, 2);

        var brush = new SolidColorBrush(BarColor);
        var hoverColor = ColorHelper.FromArgb(255,
            (byte)Math.Min(BarColor.R + 40, 255),
            (byte)Math.Min(BarColor.G + 40, 255),
            (byte)Math.Min(BarColor.B + 40, 255));

        // Y-axis labels (top, middle, bottom)
        RenderYAxis(maxValue, canvasHeight);

        for (int i = 0; i < data.Count; i++)
        {
            var point = data[i];
            var barHeight = maxValue > 0
                ? (point.Value / maxValue) * canvasHeight
                : 0;
            barHeight = Math.Max(barHeight, 2);

            var x = i * (barWidth + gap);
            var y = canvasHeight - barHeight;

            var bar = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = brush,
                RadiusX = 1,
                RadiusY = 1,
                Tag = point.TooltipText
            };

            Canvas.SetLeft(bar, x);
            Canvas.SetTop(bar, y);

            bar.PointerEntered += (s, e) =>
            {
                if (s is Rectangle r)
                {
                    r.Fill = new SolidColorBrush(hoverColor);
                    TooltipText.Text = r.Tag as string ?? "";
                    TooltipBorder.Visibility = Visibility.Visible;
                }
            };
            bar.PointerExited += (s, e) =>
            {
                if (s is Rectangle r)
                {
                    r.Fill = brush;
                    TooltipBorder.Visibility = Visibility.Collapsed;
                }
            };

            ChartCanvas.Children.Add(bar);

            // X-axis labels: show every Nth label to avoid crowding
            var labelInterval = Math.Max(data.Count / 6, 1);
            if (i % labelInterval == 0 || i == data.Count - 1)
            {
                var label = new TextBlock
                {
                    Text = point.Label.ToLowerInvariant(),
                    FontFamily = MonoFont,
                    FontSize = 9,
                    Foreground = (Brush)Application.Current.Resources["TextMutedBrush"]
                };
                Canvas.SetLeft(label, x);
                Canvas.SetTop(label, 2);
                XAxisPanel.Children.Add(label);
            }
        }
    }

    private void RenderHourlyBars(List<HourlyActivity> data)
    {
        ChartCanvas.Children.Clear();
        XAxisPanel.Children.Clear();
        YAxisPanel.Children.Clear();

        if (data.Count == 0) return;

        var canvasWidth = ChartCanvas.ActualWidth;
        var canvasHeight = ChartCanvas.ActualHeight;
        if (canvasWidth <= 0 || canvasHeight <= 0) return;

        var maxValue = data.Max(d => d.MaxValue);
        if (maxValue <= 0) maxValue = 1;

        var gap = 2.0;
        var totalGaps = (data.Count - 1) * gap;
        var barWidth = Math.Max((canvasWidth - totalGaps) / data.Count, 2);

        var brush = new SolidColorBrush(BarColor);
        var hoverColor = ColorHelper.FromArgb(255,
            (byte)Math.Min(BarColor.R + 40, 255),
            (byte)Math.Min(BarColor.G + 40, 255),
            (byte)Math.Min(BarColor.B + 40, 255));

        RenderYAxis(maxValue, canvasHeight);

        for (int i = 0; i < data.Count; i++)
        {
            var point = data[i];
            var barHeight = maxValue > 0
                ? (point.Value / maxValue) * canvasHeight
                : 0;
            barHeight = Math.Max(barHeight, 2);

            var x = i * (barWidth + gap);
            var y = canvasHeight - barHeight;

            var bar = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = brush,
                RadiusX = 1,
                RadiusY = 1,
                Tag = point.TooltipText
            };

            Canvas.SetLeft(bar, x);
            Canvas.SetTop(bar, y);

            bar.PointerEntered += (s, e) =>
            {
                if (s is Rectangle r)
                {
                    r.Fill = new SolidColorBrush(hoverColor);
                    TooltipText.Text = r.Tag as string ?? "";
                    TooltipBorder.Visibility = Visibility.Visible;
                }
            };
            bar.PointerExited += (s, e) =>
            {
                if (s is Rectangle r)
                {
                    r.Fill = brush;
                    TooltipBorder.Visibility = Visibility.Collapsed;
                }
            };

            ChartCanvas.Children.Add(bar);

            // X-axis: show every 4th hour label
            if (i % 4 == 0)
            {
                var label = new TextBlock
                {
                    Text = point.HourLabel.ToLowerInvariant(),
                    FontFamily = MonoFont,
                    FontSize = 9,
                    Foreground = (Brush)Application.Current.Resources["TextMutedBrush"]
                };
                Canvas.SetLeft(label, x);
                Canvas.SetTop(label, 2);
                XAxisPanel.Children.Add(label);
            }
        }
    }

    private void RenderYAxis(double maxValue, double canvasHeight)
    {
        YAxisPanel.Children.Clear();

        // Three labels: max, mid, 0
        var labels = new[] { maxValue, maxValue / 2, 0.0 };
        var positions = new[] { 0.0, 0.5, 1.0 };

        for (int i = 0; i < labels.Length; i++)
        {
            var text = labels[i] >= 1000 ? $"{labels[i] / 1000:F0}k"
                     : labels[i] >= 1 ? $"{labels[i]:F0}"
                     : "0";

            var tb = new TextBlock
            {
                Text = text,
                FontFamily = MonoFont,
                FontSize = 9,
                Foreground = (Brush)Application.Current.Resources["TextMutedBrush"],
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, positions[i] * Math.Max(canvasHeight - 12, 0), 0, 0)
            };
            YAxisPanel.Children.Add(tb);
        }
    }
}
