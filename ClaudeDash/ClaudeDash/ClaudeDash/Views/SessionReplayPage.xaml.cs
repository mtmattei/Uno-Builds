using ClaudeDash.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClaudeDash.Views;

public sealed partial class SessionReplayPage : Page
{
    public SessionReplayPage()
    {
        this.InitializeComponent();
        var host = App.Current.Host!.Services;
        DataContext = ActivatorUtilities.CreateInstance<BindableSessionReplayModel>(host);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack) Frame.GoBack();
    }

    private void TimelineList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Selection handled by data binding
    }

    private void JumpToStart_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BindableSessionReplayModel vm)
            vm.JumpToStart.Execute(null);
    }

    private void StepBack_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BindableSessionReplayModel vm)
            vm.StepBack.Execute(null);
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BindableSessionReplayModel vm)
            vm.TogglePlayback.Execute(null);
    }

    private void StepForward_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BindableSessionReplayModel vm)
            vm.StepForward.Execute(null);
    }

    private void JumpToEnd_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BindableSessionReplayModel vm)
            vm.JumpToEnd.Execute(null);
    }

    private void Speed_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BindableSessionReplayModel vm)
            vm.CycleSpeed.Execute(null);
    }

    private void Scrubber_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (DataContext is BindableSessionReplayModel vm)
            vm.SeekToPosition.Execute((int)e.NewValue);
    }
}
