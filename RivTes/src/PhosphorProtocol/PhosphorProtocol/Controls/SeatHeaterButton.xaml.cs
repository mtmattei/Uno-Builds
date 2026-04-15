using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace PhosphorProtocol.Controls;

public sealed partial class SeatHeaterButton : UserControl
{
    private static readonly Windows.UI.Color PeakColor = Windows.UI.Color.FromArgb(255, 111, 252, 246);
    private static readonly Windows.UI.Color DimColor = Windows.UI.Color.FromArgb(255, 20, 56, 56);
    private static readonly Windows.UI.Color GhostColor = Windows.UI.Color.FromArgb(255, 10, 34, 34);
    private static readonly Windows.UI.Color BrightColor = Windows.UI.Color.FromArgb(255, 58, 171, 166);
    private static readonly Windows.UI.Color AmberColor = Windows.UI.Color.FromArgb(255, 212, 168, 50);

    public static readonly DependencyProperty HeatLevelProperty =
        DependencyProperty.Register(nameof(HeatLevel), typeof(int), typeof(SeatHeaterButton),
            new PropertyMetadata(0, OnHeatLevelChanged));

    public static readonly DependencyProperty SideProperty =
        DependencyProperty.Register(nameof(Side), typeof(string), typeof(SeatHeaterButton),
            new PropertyMetadata("L"));

    public SeatHeaterButton()
    {
        this.InitializeComponent();
        Tapped += (_, _) => HeatLevel = (HeatLevel + 1) % 4;
        Loaded += (_, _) => UpdateVisuals();
    }

    public int HeatLevel { get => (int)GetValue(HeatLevelProperty); set => SetValue(HeatLevelProperty, value); }
    public string Side { get => (string)GetValue(SideProperty); set => SetValue(SideProperty, value); }

    private static void OnHeatLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { if (d is SeatHeaterButton shb) shb.UpdateVisuals(); }

    private void UpdateVisuals()
    {
        bool active = HeatLevel > 0;
        var seatBrush = new SolidColorBrush(active ? PeakColor : DimColor);
        var amberBrush = new SolidColorBrush(AmberColor);

        SeatBack.Stroke = seatBrush;
        SeatTop.Stroke = seatBrush;
        SeatFront.Stroke = seatBrush;
        SeatBase.Stroke = seatBrush;

        OuterBorder.BorderBrush = new SolidColorBrush(active ? BrightColor : GhostColor);
        OuterBorder.Background = active
            ? new SolidColorBrush(Windows.UI.Color.FromArgb(8, 111, 252, 246))
            : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

        Wave1.Stroke = amberBrush;
        Wave2.Stroke = amberBrush;
        Wave3.Stroke = amberBrush;

        Wave1.Visibility = HeatLevel >= 1 ? Visibility.Visible : Visibility.Collapsed;
        Wave2.Visibility = HeatLevel >= 2 ? Visibility.Visible : Visibility.Collapsed;
        Wave3.Visibility = HeatLevel >= 3 ? Visibility.Visible : Visibility.Collapsed;

        LevelText.Text = active ? $"SEAT {Side} {HeatLevel}" : $"SEAT {Side}";
        LevelText.Foreground = active
            ? new SolidColorBrush(AmberColor)
            : new SolidColorBrush(DimColor);
    }
}
