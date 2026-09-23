using System.Collections.ObjectModel;
using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.ViewModels;

public enum AssetFilter
{
    All,
    Operational,
    Attention,
    Critical,
}

public sealed partial class AssetsViewModel : LoadableViewModel
{
    private readonly IFieldCheckRepository _repository;
    private readonly INavigator _navigator;
    private IReadOnlyList<Asset> _all = [];
    private string? _pendingSelectionId;

    public AssetsViewModel(IFieldCheckRepository repository, INavigator navigator, AssetDetailViewModel detail)
    {
        _repository = repository;
        _navigator = navigator;
        Detail = detail;
        _repository.DataChanged += (_, _) => _ = LoadAsync();
    }

    /// <summary>Detail pane used by the wide master/detail layout.</summary>
    public AssetDetailViewModel Detail { get; }

    public ObservableCollection<Asset> Items { get; } = [];

    [ObservableProperty]
    public partial string Subtitle { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAll), nameof(IsOperational), nameof(IsAttention), nameof(IsCritical))]
    public partial AssetFilter Filter { get; set; } = AssetFilter.All;

    [ObservableProperty]
    public partial bool HasNoResults { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection), nameof(ShowSelectionPrompt))]
    public partial Asset? SelectedAsset { get; set; }

    /// <summary>Set by the view when the window is wide enough for master/detail.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSelectionPrompt))]
    public partial bool IsWide { get; set; }

    public bool HasSelection => SelectedAsset is not null;

    public bool ShowSelectionPrompt => IsWide && SelectedAsset is null;

    public bool IsAll { get => Filter == AssetFilter.All; set { if (value) Filter = AssetFilter.All; } }

    public bool IsOperational { get => Filter == AssetFilter.Operational; set { if (value) Filter = AssetFilter.Operational; } }

    public bool IsAttention { get => Filter == AssetFilter.Attention; set { if (value) Filter = AssetFilter.Attention; } }

    public bool IsCritical { get => Filter == AssetFilter.Critical; set { if (value) Filter = AssetFilter.Critical; } }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnFilterChanged(AssetFilter value) => ApplyFilter();

    partial void OnSelectedAssetChanged(Asset? value)
    {
        if (value is not null)
        {
            Detail.Load(value.Id);
        }
    }

    protected override async Task<bool> LoadCoreAsync()
    {
        _all = await _repository.GetAssetsAsync();
        Subtitle = Formats.Count(_all.Count, "equipment record", "equipment records");

        // Keep the selection pointing at the refreshed record.
        var selectedId = _pendingSelectionId ?? SelectedAsset?.Id;
        _pendingSelectionId = null;
        SelectedAsset = selectedId is null ? null : _all.FirstOrDefault(a => a.Id == selectedId);

        ApplyFilter();
        return _all.Count > 0;
    }

    public static bool Matches(Asset asset, string search, AssetFilter filter)
    {
        var statusMatches = filter switch
        {
            AssetFilter.Operational => asset.Status == AssetStatus.Operational,
            AssetFilter.Attention => asset.Status == AssetStatus.Attention,
            AssetFilter.Critical => asset.Status == AssetStatus.Critical,
            _ => true,
        };
        if (!statusMatches)
        {
            return false;
        }

        var term = search.Trim();
        return term.Length == 0
            || asset.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
            || asset.Id.Contains(term, StringComparison.OrdinalIgnoreCase)
            || asset.Type.Contains(term, StringComparison.OrdinalIgnoreCase)
            || asset.Location.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyFilter()
    {
        var selected = SelectedAsset;
        Items.Clear();
        foreach (var asset in _all.Where(a => Matches(a, SearchText, Filter)))
        {
            Items.Add(asset);
        }

        HasNoResults = _all.Count > 0 && Items.Count == 0;

        // Clearing the list resets list selection in the view; restore it.
        SelectedAsset = selected is null ? null : Items.FirstOrDefault(a => a.Id == selected.Id) ?? selected;
    }

    public void OpenAsset(Asset asset)
    {
        if (IsWide)
        {
            SelectedAsset = asset;
        }
        else
        {
            _navigator.ShowAsset(asset.Id);
        }
    }

    /// <summary>Selects an asset by id (used when navigating from elsewhere on wide layouts).</summary>
    public void Select(string assetId)
    {
        if (!IsReady)
        {
            _pendingSelectionId = assetId;
            return;
        }

        SelectedAsset = _all.FirstOrDefault(a => a.Id == assetId);
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        Filter = AssetFilter.All;
    }
}
