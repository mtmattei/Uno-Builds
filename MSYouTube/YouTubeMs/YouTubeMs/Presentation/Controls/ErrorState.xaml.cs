using System.Windows.Input;

namespace YouTubeMs.Presentation.Controls;

public sealed partial class ErrorState : UserControl
{
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(ErrorState),
        new PropertyMetadata("Something went wrong loading this section."));

    public static readonly DependencyProperty RetryCommandProperty = DependencyProperty.Register(
        nameof(RetryCommand), typeof(ICommand), typeof(ErrorState),
        new PropertyMetadata(null));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public ErrorState()
    {
        this.InitializeComponent();
    }
}
