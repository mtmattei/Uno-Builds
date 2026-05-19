using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoToolkit.Bench.Controls;

public sealed class FlipChip : ContentControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(FlipChip), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(FlipChip),
            new PropertyMetadata(false, OnIsCheckedChanged));

    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public bool IsChecked { get => (bool)GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }

    public event RoutedEventHandler? ChipTapped;

    public FlipChip()
    {
        DefaultStyleKey = typeof(FlipChip);
        Tapped += (s, e) => ChipTapped?.Invoke(this, new RoutedEventArgs());
        PointerEntered += (s, e) => ApplyHover(true);
        PointerExited  += (s, e) => ApplyHover(false);
    }

    private CompositeTransform? _hoverTransform;
    private Storyboard? _hoverSb;
    private DoubleAnimation? _hoverAnim;

    private void ApplyHover(bool over)
    {
        if (_hoverTransform == null)
        {
            if (GetTemplateChild("ChipRoot") is not Grid root) return;
            _hoverTransform = new CompositeTransform();
            root.RenderTransform = _hoverTransform;
            root.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);

            _hoverAnim = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(140),
                EasingFunction = Easings.Decel,
            };
            Storyboard.SetTarget(_hoverAnim, _hoverTransform);
            Storyboard.SetTargetProperty(_hoverAnim, "TranslateY");
            _hoverSb = new Storyboard();
            _hoverSb.Children.Add(_hoverAnim);
        }
        _hoverSb!.Stop();
        _hoverAnim!.To = over ? -1 : 0;
        _hoverSb.Begin();
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chip = (FlipChip)d;
        VisualStateManager.GoToState(chip, (bool)e.NewValue ? "Checked" : "Unchecked", true);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        VisualStateManager.GoToState(this, IsChecked ? "Checked" : "Unchecked", false);
    }
}
