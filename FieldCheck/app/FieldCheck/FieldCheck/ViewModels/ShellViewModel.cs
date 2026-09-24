using FieldCheck.Services;

namespace FieldCheck.ViewModels;

public sealed partial class ShellViewModel(Func<INavigator> navigator) : ObservableObject
{
    public const string UserName = "Alex Morgan";
    public const string FacilityName = "Facility A";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboardActive), nameof(IsAssetsActive), nameof(IsHistoryActive))]
    public partial AppSection CurrentSection { get; set; } = AppSection.Dashboard;

    /// <summary>False on pushed pages (detail, form, success), which hide the phone bottom bar.</summary>
    [ObservableProperty]
    public partial bool IsOnSectionRoot { get; set; } = true;

    public bool IsDashboardActive => CurrentSection == AppSection.Dashboard;

    public bool IsAssetsActive => CurrentSection == AppSection.Assets;

    public bool IsHistoryActive => CurrentSection == AppSection.History;

    [RelayCommand]
    private void ShowDashboard() => navigator().ShowSection(AppSection.Dashboard);

    [RelayCommand]
    private void ShowAssets() => navigator().ShowSection(AppSection.Assets);

    [RelayCommand]
    private void ShowHistory() => navigator().ShowSection(AppSection.History);
}
