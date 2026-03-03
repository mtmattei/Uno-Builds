namespace UnoVox.Presentation;

public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;

    [ObservableProperty]
    private string? name;

    public MainViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
        GoToVoxelEditor = new AsyncRelayCommand(GoToVoxelEditorView);
    }
    public string? Title { get; }

    public ICommand GoToVoxelEditor { get; }


    private async Task GoToVoxelEditorView()
    {
        await _navigator.NavigateViewModelAsync<VoxelEditorViewModel>(this);
    }
}
