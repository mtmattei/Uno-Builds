using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PrecisionDial.Samples;

public sealed partial class IlluminatedReadout : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(IlluminatedReadout),
            new PropertyMetadata(0.0, static (d, e) => ((IlluminatedReadout)d).UpdateDisplay((double)e.NewValue)));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IlluminatedReadout()
    {
        InitializeComponent();
        UpdateDisplay(0);
    }

    private void UpdateDisplay(double value)
    {
        value = Math.Clamp(value, 0, 100);
        var iv = (int)Math.Round(value);

        var h = iv / 100;
        var t = (iv / 10) % 10;
        var o = iv % 10;

        HundredsText.Text = h.ToString();
        TensText.Text = t.ToString();
        OnesText.Text = o.ToString();
        HundredsGlow.Text = HundredsText.Text;
        TensGlow.Text = TensText.Text;
        OnesGlow.Text = OnesText.Text;

        // Ones: lights 0→33, Tens: lights 15→66, Hundreds: lights 33→100
        var onesBright = Math.Clamp(value / 33.0, 0, 1);
        var tensBright = Math.Clamp((value - 15) / 51.0, 0, 1);
        var hundredsBright = Math.Clamp((value - 33) / 67.0, 0, 1);

        OnesText.Foreground = MakeBrush(onesBright);
        TensText.Foreground = MakeBrush(tensBright);
        HundredsText.Foreground = MakeBrush(hundredsBright);

        OnesGlow.Opacity = onesBright * 0.5;
        TensGlow.Opacity = tensBright * 0.5;
        HundredsGlow.Opacity = hundredsBright * 0.5;
    }

    // Dim: rgba(120,120,120,0.2) → Bright: #FFD4A959
    private static SolidColorBrush MakeBrush(double t)
    {
        var r = (byte)(120 + (212 - 120) * t);
        var g = (byte)(120 + (169 - 120) * t);
        var b = (byte)(120 + (89 - 120) * t);
        var a = (byte)(51 + (255 - 51) * t);
        return new SolidColorBrush(Color.FromArgb(a, r, g, b));
    }
}
