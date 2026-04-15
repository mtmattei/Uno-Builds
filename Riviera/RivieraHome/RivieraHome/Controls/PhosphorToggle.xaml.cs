using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace RivieraHome.Controls;

public sealed partial class PhosphorToggle : UserControl
{
    private static readonly SolidColorBrush PhosphorPrimary =
        new(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x33, 0xFF, 0x66));
    private static readonly SolidColorBrush PhosphorDim =
        new(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x1A, 0x99, 0x40));
    private static readonly SolidColorBrush PhosphorSubtle =
        new(Microsoft.UI.ColorHelper.FromArgb(0x14, 0x33, 0xFF, 0x66));
    private static readonly SolidColorBrush BorderSubtle =
        new(Microsoft.UI.ColorHelper.FromArgb(0x33, 0x33, 0xFF, 0x66));
    private static readonly SolidColorBrush Transparent =
        new(Microsoft.UI.Colors.Transparent);

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(PhosphorToggle),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(PhosphorToggle),
            new PropertyMetadata(false, OnIsCheckedChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public PhosphorToggle()
    {
        this.InitializeComponent();
        UpdateVisuals(animate: false);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PhosphorToggle toggle)
            toggle.LabelText.Text = toggle.Label;
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PhosphorToggle toggle)
            toggle.UpdateVisuals(animate: true);
    }

    private void OnToggleClick(object sender, RoutedEventArgs e)
    {
        IsChecked = !IsChecked;
    }

    private void UpdateVisuals(bool animate)
    {
        if (ToggleButton == null) return;

        var targetOpacity = IsChecked ? 1.0 : 0.6;

        if (IsChecked)
        {
            ToggleButton.Background = PhosphorSubtle;
            ToggleButton.BorderBrush = PhosphorPrimary;
            if (LabelText != null)
                LabelText.Foreground = PhosphorPrimary;
        }
        else
        {
            ToggleButton.Background = Transparent;
            ToggleButton.BorderBrush = BorderSubtle;
            if (LabelText != null)
                LabelText.Foreground = PhosphorDim;
        }

        if (animate)
        {
            var storyboard = new Storyboard();
            var opacityAnim = new DoubleAnimation
            {
                To = targetOpacity,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(opacityAnim, ToggleButton);
            Storyboard.SetTargetProperty(opacityAnim, "Opacity");
            storyboard.Children.Add(opacityAnim);
            storyboard.Begin();
        }
        else
        {
            ToggleButton.Opacity = targetOpacity;
        }
    }
}
