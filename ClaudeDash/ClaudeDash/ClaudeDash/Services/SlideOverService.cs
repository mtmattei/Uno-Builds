namespace ClaudeDash.Services;

public class SlideOverService : ISlideOverService
{
    public bool IsOpen { get; private set; }

    public event Action<string, UIElement>? ShowRequested;
    public event Action? HideRequested;

    public void Show(string title, UIElement content)
    {
        IsOpen = true;
        ShowRequested?.Invoke(title, content);
    }

    public void Hide()
    {
        IsOpen = false;
        HideRequested?.Invoke();
    }
}
