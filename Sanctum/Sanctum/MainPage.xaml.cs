using Sanctum.Services;
using Sanctum.ViewModels;

namespace Sanctum;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        var appState = App.Services!.GetRequiredService<IAppStateService>();
        var navigation = App.Services!.GetRequiredService<INavigationService>();
        ViewModel = new MainViewModel(appState, navigation);

        this.InitializeComponent();
    }
}
