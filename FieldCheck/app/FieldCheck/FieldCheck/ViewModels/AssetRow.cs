using FieldCheck.Models;

namespace FieldCheck.ViewModels;

/// <summary>An asset list row; carries the master/detail selection highlight.</summary>
public sealed partial class AssetRow(Asset asset) : ObservableObject
{
    public Asset Asset { get; } = asset;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public string Name => Asset.Name;

    public string StatusText => Asset.StatusText;

    public string IdAndType => Asset.IdAndType;

    public string IdAndLocation => Asset.IdAndLocation;

    public string Location => Asset.Location;

    public string SpokenText => Asset.ToString();
}
