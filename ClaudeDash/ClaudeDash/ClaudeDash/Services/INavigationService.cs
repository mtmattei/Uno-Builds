namespace ClaudeDash.Services;

public interface INavigationService
{
    void SetFrame(Frame frame);
    bool NavigateTo(string pageKey);
    bool NavigateTo(string pageKey, object? parameter);
    bool GoBack();
    string CurrentPageKey { get; }
    event Action<string>? Navigated;
}
