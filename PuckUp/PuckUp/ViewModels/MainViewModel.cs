using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using PuckUp.Models;
using Uno.Extensions.Navigation;

namespace PuckUp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;

    [ObservableProperty] private string? name;

    public MainViewModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Main";
        Title += $" - {localizer["ApplicationName"]}";
        Title += $" - {appInfo?.Value?.Environment}";
    }

    public string? Title { get; }
}
