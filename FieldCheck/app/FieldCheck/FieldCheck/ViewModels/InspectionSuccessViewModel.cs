using FieldCheck.Models;
using FieldCheck.Services;

namespace FieldCheck.ViewModels;

public sealed partial class InspectionSuccessViewModel(IFieldCheckRepository repository, INavigator navigator) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspectionId), nameof(AssetName), nameof(ConditionText), nameof(DetailLine))]
    public partial Inspection? Inspection { get; private set; }

    public string InspectionId => Inspection?.Id ?? string.Empty;

    public string AssetName => Inspection?.AssetName ?? string.Empty;

    public string ConditionText => Inspection?.Condition.ToString() ?? string.Empty;

    public string DetailLine => Inspection is null ? string.Empty : $"{Formats.Date(Inspection.Date)} · {Inspection.Inspector}";

    public async Task LoadAsync(string inspectionId)
    {
        try
        {
            var inspections = await repository.GetInspectionsAsync();
            Inspection = inspections.FirstOrDefault(i => i.Id == inspectionId);
        }
        catch (RepositoryException)
        {
            Inspection = null;
        }
    }

    [RelayCommand]
    private void ViewAsset()
    {
        if (Inspection is not null)
        {
            navigator.ReturnToAsset(Inspection.AssetId);
        }
    }

    [RelayCommand]
    private void ViewHistory() => navigator.ShowSection(AppSection.History);
}
