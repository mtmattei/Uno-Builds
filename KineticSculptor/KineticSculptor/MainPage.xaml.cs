using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;

namespace KineticSculptor;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        Surface.FirstInteraction += OnFirstInteraction;
    }

    private void OnFirstInteraction(object? sender, EventArgs e)
    {
        Surface.FirstInteraction -= OnFirstInteraction;
        FadeOut(TitleMark);
        FadeOut(HintMark);
    }

    private static void FadeOut(UIElement target)
    {
        var sb = new Storyboard();
        var anim = new DoubleAnimation
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(1.6),
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 }
        };
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, "Opacity");
        sb.Children.Add(anim);
        sb.Begin();
    }
}
