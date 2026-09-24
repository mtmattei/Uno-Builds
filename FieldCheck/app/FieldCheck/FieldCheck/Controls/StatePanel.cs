using System.Windows.Input;
using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace FieldCheck.Controls;

/// <summary>Loading / empty / error presentation for a repository-backed screen. Hidden when ready.</summary>
public sealed partial class StatePanel : ContentControl
{
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State), typeof(LoadState), typeof(StatePanel), new PropertyMetadata(LoadState.Loading, (d, _) => ((StatePanel)d).Apply()));

    public static readonly DependencyProperty RetryCommandProperty = DependencyProperty.Register(
        nameof(RetryCommand), typeof(ICommand), typeof(StatePanel), new PropertyMetadata(null, (d, e) => ((StatePanel)d)._retry.Command = (ICommand?)e.NewValue));

    private readonly ProgressRing _ring = new() { Width = 28, Height = 28, IsActive = false, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBlock _title = new() { FontSize = 19, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _message = new() { FontSize = 14, TextWrapping = TextWrapping.Wrap, MaxWidth = 460, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly Button _retry = new() { Content = "Retry", MinWidth = 140, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 8, 0, 0) };

    public StatePanel()
    {
        IsTabStop = false;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _title.Foreground = Brush("InkBrush");
        _message.Foreground = Brush("MutedBrush");
        _retry.Style = (Style)Application.Current.Resources["SecondaryButtonStyle"];
        AutomationProperties.SetName(_retry, "Retry loading data");
        Content = new StackPanel
        {
            Spacing = 10,
            Padding = new Thickness(0, 32, 0, 32),
            Children = { _ring, _title, _message, _retry },
        };
        Loaded += (_, _) => Apply();
        Apply();
    }

    public LoadState State
    {
        get => (LoadState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public string LoadingText { get; set; } = "Loading…";

    public string EmptyTitle { get; set; } = "Nothing here yet";

    public string EmptyMessage { get; set; } = string.Empty;

    public string ErrorTitle { get; set; } = "Couldn't load data";

    private void Apply()
    {
        Visibility = State == LoadState.Ready ? Visibility.Collapsed : Visibility.Visible;
        _ring.IsActive = State == LoadState.Loading;
        _ring.Visibility = State == LoadState.Loading ? Visibility.Visible : Visibility.Collapsed;
        _retry.Visibility = State == LoadState.Error ? Visibility.Visible : Visibility.Collapsed;
        (_title.Text, _message.Text) = State switch
        {
            LoadState.Loading => (LoadingText, string.Empty),
            LoadState.Empty => (EmptyTitle, EmptyMessage),
            LoadState.Error => (ErrorTitle, LoadableViewModel.ErrorMessage),
            _ => (string.Empty, string.Empty),
        };
        _title.FontSize = State == LoadState.Loading ? 15 : 19;
        _title.Foreground = Brush(State == LoadState.Loading ? "MutedBrush" : "InkBrush");
        _message.Visibility = _message.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        AutomationProperties.SetLiveSetting(_title, Microsoft.UI.Xaml.Automation.Peers.AutomationLiveSetting.Polite);
    }

    private static Brush Brush(string key) => (Brush)Application.Current.Resources[key];
}
