using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Composer.Views.Controls;

/// <summary>
/// Brief 03 §3 — flat preview card for an upcoming layer. Stacks below the
/// active canvas with progressive opacity. Pointer events disabled — these
/// are read-only previews.
/// </summary>
public sealed partial class FuturePreviewCard : UserControl
{
    public static readonly DependencyProperty LayerLabelProperty =
        DependencyProperty.Register(
            nameof(LayerLabel), typeof(string), typeof(FuturePreviewCard),
            new PropertyMetadata(string.Empty, OnLabelOrHintChanged));

    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(
            nameof(Hint), typeof(string), typeof(FuturePreviewCard),
            new PropertyMetadata(string.Empty, OnLabelOrHintChanged));

    public string LayerLabel
    {
        get => (string)GetValue(LayerLabelProperty);
        set => SetValue(LayerLabelProperty, value);
    }

    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    public FuturePreviewCard() => this.InitializeComponent();

    private static void OnLabelOrHintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FuturePreviewCard c) c.Render();
    }

    private void Render()
    {
        HeaderText.Text = $"{LayerLabel}  ·  UPCOMING";
        HintText.Text   = Hint;
    }
}
