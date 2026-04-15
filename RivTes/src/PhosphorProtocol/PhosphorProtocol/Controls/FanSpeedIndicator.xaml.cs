using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace PhosphorProtocol.Controls;

public sealed partial class FanSpeedIndicator : UserControl
{
    private static readonly double[] BarHeights = [4.5, 7, 9.5, 12, 15];
    private static readonly Windows.UI.Color BrightColor = Windows.UI.Color.FromArgb(255, 58, 171, 166);
    private static readonly Windows.UI.Color OffColor = Windows.UI.Color.FromArgb(255, 5, 18, 18);

    private readonly Rectangle[] _bars = new Rectangle[5];

    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(nameof(Level), typeof(int), typeof(FanSpeedIndicator),
            new PropertyMetadata(0, OnLevelChanged));

    public FanSpeedIndicator()
    {
        this.InitializeComponent();
        BuildBars();
    }

    public int Level { get => (int)GetValue(LevelProperty); set => SetValue(LevelProperty, value); }

    private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { if (d is FanSpeedIndicator fsi) fsi.UpdateBars(); }

    private void BuildBars()
    {
        for (int i = 0; i < 5; i++)
        {
            var bar = new Rectangle
            {
                Width = 3,
                Height = BarHeights[i],
                RadiusX = 1,
                RadiusY = 1,
                Fill = new SolidColorBrush(OffColor),
                VerticalAlignment = VerticalAlignment.Bottom
            };
            _bars[i] = bar;
            BarPanel.Children.Add(bar);
        }
    }

    private void UpdateBars()
    {
        for (int i = 0; i < 5; i++)
        {
            _bars[i].Fill = new SolidColorBrush(i < Level ? BrightColor : OffColor);
        }
    }
}
