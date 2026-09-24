using System.Collections.ObjectModel;
using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.ViewModels;

public enum ConditionFilter
{
    All,
    Good,
    Attention,
    Critical,
}

public sealed record HistoryRow(Inspection Inspection, string Timestamp)
{
    public string AssetName => Inspection.AssetName;

    public string Condition => Inspection.Condition.ToString();

    public string Inspector => Inspection.Inspector;

    /// <summary>"INS-24091 · Sep 18, 9:24 AM"</summary>
    public string IdAndTime => $"{Inspection.Id} · {Timestamp}";

    public string SpokenText => $"{AssetName}, {Inspection.Id}, {Timestamp}, {Condition}, {Inspector}";

    public override string ToString() => SpokenText;
}

public sealed partial class HistoryViewModel : LoadableViewModel
{
    private readonly IFieldCheckRepository _repository;
    private readonly TimeProvider _clock;
    private IReadOnlyList<Inspection> _all = [];

    public HistoryViewModel(IFieldCheckRepository repository, TimeProvider clock)
    {
        _repository = repository;
        _clock = clock;
        _repository.DataChanged += (_, _) => _ = LoadAsync();
    }

    public ObservableCollection<HistoryRow> Items { get; } = [];

    [ObservableProperty]
    public partial string Subtitle { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAll), nameof(IsGood), nameof(IsAttention), nameof(IsCritical))]
    public partial ConditionFilter Filter { get; set; } = ConditionFilter.All;

    [ObservableProperty]
    public partial bool HasNoResults { get; private set; }

    public bool IsAll { get => Filter == ConditionFilter.All; set { if (value) Filter = ConditionFilter.All; } }

    public bool IsGood { get => Filter == ConditionFilter.Good; set { if (value) Filter = ConditionFilter.Good; } }

    public bool IsAttention { get => Filter == ConditionFilter.Attention; set { if (value) Filter = ConditionFilter.Attention; } }

    public bool IsCritical { get => Filter == ConditionFilter.Critical; set { if (value) Filter = ConditionFilter.Critical; } }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnFilterChanged(ConditionFilter value) => ApplyFilter();

    protected override async Task<bool> LoadCoreAsync()
    {
        _all = (await _repository.GetInspectionsAsync())
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.Id, StringComparer.Ordinal)
            .ToList();
        Subtitle = Formats.Count(_all.Count, "completed inspection", "completed inspections");
        ApplyFilter();
        return _all.Count > 0;
    }

    public static bool Matches(Inspection inspection, string search, ConditionFilter filter)
    {
        var conditionMatches = filter switch
        {
            ConditionFilter.Good => inspection.Condition == InspectionCondition.Good,
            ConditionFilter.Attention => inspection.Condition == InspectionCondition.Attention,
            ConditionFilter.Critical => inspection.Condition == InspectionCondition.Critical,
            _ => true,
        };
        if (!conditionMatches)
        {
            return false;
        }

        var term = search.Trim();
        return term.Length == 0
            || inspection.AssetName.Contains(term, StringComparison.OrdinalIgnoreCase)
            || inspection.AssetId.Contains(term, StringComparison.OrdinalIgnoreCase)
            || inspection.Id.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplyFilter()
    {
        var now = _clock.GetLocalNow().DateTime;
        Items.Clear();
        foreach (var inspection in _all.Where(i => Matches(i, SearchText, Filter)))
        {
            Items.Add(new HistoryRow(inspection, Formats.Timestamp(inspection.Date, now)));
        }

        HasNoResults = _all.Count > 0 && Items.Count == 0;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        Filter = ConditionFilter.All;
    }
}
