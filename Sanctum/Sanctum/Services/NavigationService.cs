namespace Sanctum.Services;

/// <summary>
/// Simple frame navigation service
/// </summary>
public class NavigationService : INavigationService
{
    private Frame? _frame;

    public void Initialize(Frame frame)
    {
        _frame = frame;
    }

    public void NavigateTo<T>() where T : Page
    {
        _frame?.Navigate(typeof(T));
    }

    public void NavigateTo(Type pageType)
    {
        _frame?.Navigate(pageType);
    }

    public void NavigateTo(Type pageType, object parameter)
    {
        _frame?.Navigate(pageType, parameter);
    }

    public void GoBack()
    {
        if (_frame?.CanGoBack == true)
        {
            _frame.GoBack();
        }
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;
}

public interface INavigationService
{
    void Initialize(Frame frame);
    void NavigateTo<T>() where T : Page;
    void NavigateTo(Type pageType);
    void NavigateTo(Type pageType, object parameter);
    void GoBack();
    bool CanGoBack { get; }
}
