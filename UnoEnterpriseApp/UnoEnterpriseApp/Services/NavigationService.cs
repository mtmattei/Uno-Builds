using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;

namespace UnoEnterpriseApp.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;
    
    public Frame? Frame
    {
        get => _frame;
        set
        {
            if (_frame != null)
            {
                _frame.Navigated -= OnFrameNavigated;
                _frame.NavigationFailed -= OnFrameNavigationFailed;
            }
            
            _frame = value;
            
            if (_frame != null)
            {
                _frame.Navigated += OnFrameNavigated;
                _frame.NavigationFailed += OnFrameNavigationFailed;
            }
        }
    }
    
    public bool CanGoBack => Frame?.CanGoBack ?? false;
    public bool CanGoForward => Frame?.CanGoForward ?? false;
    
    public event NavigatedEventHandler? Navigated;
    public event NavigationFailedEventHandler? NavigationFailed;
    
    public bool Navigate(Type pageType, object? parameter = null, NavigationTransitionInfo? transitionInfo = null)
    {
        if (Frame == null)
            return false;
            
        return transitionInfo != null 
            ? Frame.Navigate(pageType, parameter, transitionInfo)
            : Frame.Navigate(pageType, parameter);
    }
    
    public bool Navigate<T>(object? parameter = null, NavigationTransitionInfo? transitionInfo = null) where T : Page
    {
        return Navigate(typeof(T), parameter, transitionInfo);
    }
    
    public void GoBack()
    {
        if (CanGoBack)
            Frame?.GoBack();
    }
    
    public void GoForward()
    {
        if (CanGoForward)
            Frame?.GoForward();
    }
    
    public void ClearHistory()
    {
        Frame?.BackStack.Clear();
    }
    
    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        Navigated?.Invoke(sender, e);
    }
    
    private void OnFrameNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        NavigationFailed?.Invoke(sender, e);
    }
}