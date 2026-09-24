using FieldCheck.ViewModels;

namespace FieldCheck.Views;

/// <summary>Asset information sheet shared by the phone detail page and the wide master/detail pane.</summary>
public sealed partial class AssetDetailView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(AssetDetailViewModel), typeof(AssetDetailView), new PropertyMetadata(null, (d, _) => ((AssetDetailView)d).Bindings.Update()));

    /// <summary>Below this pane width the wide header stacks the Start button under the title.</summary>
    private const double CompactHeaderWidth = 560;

    public AssetDetailView()
    {
        InitializeComponent();
        SizeChanged += (_, e) => ArrangeHeader(e.NewSize.Width);
    }

    private void ArrangeHeader(double width)
    {
        var compact = width < CompactHeaderWidth;
        Grid.SetRow(WideStartButton, compact ? 1 : 0);
        Grid.SetColumn(WideStartButton, compact ? 0 : 1);
        WideStartButton.HorizontalAlignment = compact ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
        HeaderGrid.ColumnSpacing = compact ? 0 : 24;
    }

    public AssetDetailViewModel? ViewModel
    {
        get => (AssetDetailViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
}
