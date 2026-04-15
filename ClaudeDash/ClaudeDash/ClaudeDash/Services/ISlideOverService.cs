namespace ClaudeDash.Services;

public interface ISlideOverService
{
    void Show(string title, UIElement content);
    void Hide();
    bool IsOpen { get; }
}
