using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoEnterpriseApp.Services;

public interface INavigationService
{
    Frame? Frame { get; set; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }
    
    bool Navigate(Type pageType, object? parameter = null, NavigationTransitionInfo? transitionInfo = null);
    bool Navigate<T>(object? parameter = null, NavigationTransitionInfo? transitionInfo = null) where T : Page;
    void GoBack();
    void GoForward();
    void ClearHistory();
    
    event NavigatedEventHandler? Navigated;
    event NavigationFailedEventHandler? NavigationFailed;
}