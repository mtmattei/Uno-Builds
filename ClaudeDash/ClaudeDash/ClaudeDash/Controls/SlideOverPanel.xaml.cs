using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace ClaudeDash.Controls;

public sealed partial class SlideOverPanel : UserControl
{
    public SlideOverPanel()
    {
        this.InitializeComponent();
    }

    public bool IsOpen { get; private set; }

    public void Show(string title, UIElement content)
    {
        TitleText.Text = title;
        ContentHost.Content = content;
        Visibility = Visibility.Visible;
        IsOpen = true;

        AnimatePanel(fromX: 400, toX: 0, durationMs: 250);
    }

    public void Hide()
    {
        if (!IsOpen) return;
        IsOpen = false;

        AnimatePanel(fromX: 0, toX: 400, durationMs: 200, onCompleted: () =>
        {
            Visibility = Visibility.Collapsed;
            ContentHost.Content = null;
        });
    }

    private void AnimatePanel(double fromX, double toX, int durationMs, Action? onCompleted = null)
    {
        var animation = new DoubleAnimation
        {
            From = fromX,
            To = toX,
            Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
            EasingFunction = toX == 0
                ? new CubicEase { EasingMode = EasingMode.EaseOut }
                : new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, PanelTranslate);
        Storyboard.SetTargetProperty(animation, "X");

        if (onCompleted != null)
        {
            storyboard.Completed += (s, e) => onCompleted();
        }

        storyboard.Begin();
    }

    private void Backdrop_Tapped(object sender, TappedRoutedEventArgs e)
    {
        Hide();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
