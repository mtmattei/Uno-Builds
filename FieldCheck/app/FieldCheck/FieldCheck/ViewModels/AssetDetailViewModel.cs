using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.ViewModels;

public sealed partial class AssetDetailViewModel : LoadableViewModel, IDisposable
{
    private readonly IFieldCheckRepository _repository;
    private readonly INavigator _navigator;
    private string? _assetId;

    public AssetDetailViewModel(IFieldCheckRepository repository, INavigator navigator)
    {
        _repository = repository;
        _navigator = navigator;
        _repository.DataChanged += OnDataChanged;
    }

    public void Dispose() => _repository.DataChanged -= OnDataChanged;

    private void OnDataChanged(object? sender, EventArgs e)
    {
        if (_assetId is not null)
        {
            _ = LoadAsync();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastInspectionText), nameof(HasNoInspection))]
    public partial Asset? Asset { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLatestInspection), nameof(HasNoInspection), nameof(LatestTitle), nameof(LatestConditionText), nameof(LatestSummary), nameof(LatestMeta))]
    public partial Inspection? LatestInspection { get; private set; }

    public string LastInspectionText => Asset is null ? string.Empty : Formats.Date(Asset.LastInspection);

    public bool HasLatestInspection => LatestInspection is not null;

    public bool HasNoInspection => Asset is not null && LatestInspection is null;

    public string LatestConditionText => LatestInspection?.Condition.ToString() ?? string.Empty;

    public string LatestTitle => LatestInspection is null ? string.Empty : Formats.ConditionTitle(LatestInspection.Condition);

    /// <summary>The issue description when one was recorded, otherwise the notes.</summary>
    public string LatestSummary => LatestInspection switch
    {
        null => string.Empty,
        { IssueDescription: { Length: > 0 } issue } => issue,
        { Notes: { Length: > 0 } notes } => notes,
        _ => "No notes recorded.",
    };

    public string LatestMeta => LatestInspection is null
        ? string.Empty
        : $"{LatestInspection.Id} · {Formats.Date(LatestInspection.Date)} · {LatestInspection.Inspector}";

    public void Load(string assetId)
    {
        _assetId = assetId;
        _ = LoadAsync();
    }

    protected override async Task<bool> LoadCoreAsync()
    {
        var id = _assetId;
        var assets = await _repository.GetAssetsAsync();
        var inspections = await _repository.GetInspectionsAsync();
        if (id != _assetId)
        {
            // A newer selection superseded this load.
            return Asset is not null;
        }

        Asset = assets.FirstOrDefault(a => a.Id == id);
        LatestInspection = inspections
            .Where(i => i.AssetId == id)
            .OrderByDescending(i => i.Date)
            .FirstOrDefault();
        OnPropertyChanged(nameof(HasNoInspection));
        return Asset is not null;
    }

    [RelayCommand]
    private void StartInspection()
    {
        if (Asset is not null)
        {
            _navigator.StartInspection(Asset.Id);
        }
    }

    [RelayCommand]
    private void GoBack() => _navigator.GoBack();
}
