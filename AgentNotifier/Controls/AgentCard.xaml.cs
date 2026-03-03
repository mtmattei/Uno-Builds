using AgentNotifier.ViewModels;
using Microsoft.UI.Xaml.Media.Animation;
using System.ComponentModel;

namespace AgentNotifier.Controls;

public sealed partial class AgentCard : UserControl
{
    private Storyboard? _pulseAnimation;
    private AgentViewModel? _currentViewModel;

    public AgentCard()
    {
        this.InitializeComponent();
        this.DataContextChanged += OnDataContextChanged;
        this.Loaded += OnLoaded;
        this.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _pulseAnimation = Resources["PulseAnimation"] as Storyboard;
        UpdatePulseAnimation();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopPulseAnimation();
        if (_currentViewModel != null)
        {
            _currentViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _currentViewModel = null;
        }
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        // Unsubscribe from old ViewModel
        if (_currentViewModel != null)
        {
            _currentViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        // Subscribe to new ViewModel
        _currentViewModel = args.NewValue as AgentViewModel;
        if (_currentViewModel != null)
        {
            _currentViewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdatePulseAnimation();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AgentViewModel.IsWaitingForInput) ||
            e.PropertyName == nameof(AgentViewModel.Status))
        {
            DispatcherQueue.TryEnqueue(() => UpdatePulseAnimation());
        }
    }

    private void UpdatePulseAnimation()
    {
        if (_currentViewModel == null || _pulseAnimation == null) return;

        if (_currentViewModel.IsWaitingForInput)
        {
            StartPulseAnimation();
        }
        else
        {
            StopPulseAnimation();
        }
    }

    private void StartPulseAnimation()
    {
        if (_pulseAnimation == null) return;

        try
        {
            _pulseAnimation.Begin();
        }
        catch
        {
            // Animation may already be running
        }
    }

    private void StopPulseAnimation()
    {
        if (_pulseAnimation == null) return;

        try
        {
            _pulseAnimation.Stop();
            StatusBadgeBorder.Opacity = 0;
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

}
