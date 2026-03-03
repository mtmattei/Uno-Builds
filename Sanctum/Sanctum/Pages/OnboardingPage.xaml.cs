using Microsoft.UI.Xaml.Input;
using Sanctum.Models;
using Sanctum.Services;
using Sanctum.ViewModels;

namespace Sanctum.Pages;

public sealed partial class OnboardingPage : UserControl
{
    public OnboardingViewModel ViewModel { get; }

    public OnboardingPage()
    {
        var appState = App.Services!.GetRequiredService<IAppStateService>();
        var mockData = App.Services!.GetRequiredService<IMockDataService>();
        ViewModel = new OnboardingViewModel(appState, mockData);

        this.InitializeComponent();
    }

    // Helper methods for x:Bind
    public Visibility IsStep(int step, int currentStep)
    {
        return step == currentStep ? Visibility.Visible : Visibility.Collapsed;
    }

    public double GetStepWidth(int step, int currentStep)
    {
        return step == currentStep ? 32 : 8;
    }

    public SolidColorBrush GetStepColor(int step, int currentStep)
    {
        if (step <= currentStep)
        {
            return (SolidColorBrush)Application.Current.Resources["PrimaryBrush"];
        }
        return (SolidColorBrush)Application.Current.Resources["OutlineBrush"];
    }

    public SolidColorBrush GetToggleBackground(bool isEnabled)
    {
        if (isEnabled)
        {
            return (SolidColorBrush)Application.Current.Resources["PrimaryBrush"];
        }
        return (SolidColorBrush)Application.Current.Resources["SurfaceVariantBrush"];
    }

    public SolidColorBrush GetToggleForeground(bool isEnabled)
    {
        if (isEnabled)
        {
            return (SolidColorBrush)Application.Current.Resources["OnPrimaryBrush"];
        }
        return (SolidColorBrush)Application.Current.Resources["OnSurfaceVariantBrush"];
    }

    public string GetToggleText(bool isEnabled)
    {
        return isEnabled ? "Focus Mode Active" : "Slide to Enable";
    }

    public string GetGenerateButtonText(bool isLoading)
    {
        return isLoading ? "Generating..." : "Generate Plan";
    }

    private void GoalCard_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border && border.Tag is GoalOption goal)
        {
            ViewModel.ToggleGoalCommand.Execute(goal);
        }
    }

    private void SourceCard_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border && border.Tag is SourceOption source)
        {
            ViewModel.ToggleSourceCommand.Execute(source);
        }
    }

    private void SlideToggle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (!ViewModel.IsFocusModeEnabled)
        {
            ViewModel.EnableFocusModeCommand.Execute(null);
        }
    }
}
