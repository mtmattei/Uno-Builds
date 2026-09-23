using FieldCheck.ViewModels;

namespace FieldCheck.Views;

/// <summary>Asset information sheet shared by the phone detail page and the wide master/detail pane.</summary>
public sealed partial class AssetDetailView : UserControl
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(AssetDetailViewModel), typeof(AssetDetailView), new PropertyMetadata(null, (d, _) => ((AssetDetailView)d).Bindings.Update()));

    public AssetDetailView()
    {
        InitializeComponent();
    }

    public AssetDetailViewModel? ViewModel
    {
        get => (AssetDetailViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
}
