using ClaudeDash.Helpers;

namespace ClaudeDash.Services;

public class NavigationService : INavigationService
{
    private Frame? _frame;

    public string CurrentPageKey { get; private set; } = "home";
    public event Action<string>? Navigated;

    public void SetFrame(Frame frame) => _frame = frame;

    public bool NavigateTo(string pageKey)
    {
        return NavigateTo(pageKey, null);
    }

    public bool NavigateTo(string pageKey, object? parameter)
    {
        var pageType = PageRegistry.GetPageType(pageKey);
        if (pageType is null || _frame is null) return false;

        CurrentPageKey = pageKey;
        var result = _frame.Navigate(pageType, parameter);
        if (result)
            Navigated?.Invoke(pageKey);
        return result;
    }

    public bool GoBack()
    {
        if (_frame?.CanGoBack != true) return false;
        _frame.GoBack();
        return true;
    }
}
