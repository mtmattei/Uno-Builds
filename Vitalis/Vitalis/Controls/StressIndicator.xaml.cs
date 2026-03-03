using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Vitalis.Controls;

public sealed partial class StressIndicator : UserControl
{
    public static readonly DependencyProperty StressLevelProperty =
        DependencyProperty.Register(nameof(StressLevel), typeof(double), typeof(StressIndicator),
            new PropertyMetadata(0.0));

    public double StressLevel
    {
        get => (double)GetValue(StressLevelProperty);
        set => SetValue(StressLevelProperty, value);
    }

    public StressIndicator()
    {
        this.InitializeComponent();
    }
}
