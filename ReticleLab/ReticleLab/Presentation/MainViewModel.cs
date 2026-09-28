namespace ReticleLab.Presentation;

public partial class MainViewModel : ObservableObject
{

    public MainViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }
    public string? Title { get; }


}
