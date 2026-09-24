using System.Collections.ObjectModel;
using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.ViewModels;

public sealed partial class DashboardViewModel : LoadableViewModel
{
    private readonly IFieldCheckRepository _repository;
    private readonly INavigator _navigator;
    private readonly TimeProvider _clock;

    public DashboardViewModel(IFieldCheckRepository repository, INavigator navigator, TimeProvider clock)
    {
        _repository = repository;
        _navigator = navigator;
        _clock = clock;
        _repository.DataChanged += (_, _) => _ = LoadAsync();
    }

    public string Greeting => Formats.Greeting(_clock.GetLocalNow().DateTime);

    public string FacilityLine => Formats.FacilityLine(_clock.GetLocalNow().DateTime);

    [ObservableProperty]
    public partial int TotalCount { get; private set; }

    [ObservableProperty]
    public partial int OperationalCount { get; private set; }

    [ObservableProperty]
    public partial int AttentionCount { get; private set; }

    [ObservableProperty]
    public partial int CriticalCount { get; private set; }

    [ObservableProperty]
    public partial string TotalLabel { get; private set; } = "assets";

    /// <summary>Critical first, then Attention, each in fixture order.</summary>
    public ObservableCollection<Asset> NeedsAttention { get; } = [];

    [ObservableProperty]
    public partial bool HasNothingNeedingAttention { get; private set; }

    protected override async Task<bool> LoadCoreAsync()
    {
        var assets = await _repository.GetAssetsAsync();

        TotalCount = assets.Count;
        TotalLabel = assets.Count == 1 ? "asset" : "assets";
        OperationalCount = assets.Count(a => a.Status == AssetStatus.Operational);
        AttentionCount = assets.Count(a => a.Status == AssetStatus.Attention);
        CriticalCount = assets.Count(a => a.Status == AssetStatus.Critical);

        NeedsAttention.Clear();
        foreach (var asset in assets
            .Where(a => a.Status != AssetStatus.Operational)
            .OrderByDescending(a => a.Status == AssetStatus.Critical))
        {
            NeedsAttention.Add(asset);
        }

        HasNothingNeedingAttention = assets.Count > 0 && NeedsAttention.Count == 0;
        OnPropertyChanged(nameof(Greeting));
        OnPropertyChanged(nameof(FacilityLine));
        return assets.Count > 0;
    }

    public void OpenAsset(Asset asset) => _navigator.ShowAsset(asset.Id);

    [RelayCommand]
    private void ViewAllAssets() => _navigator.ShowSection(AppSection.Assets);
}
