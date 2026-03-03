using System.ComponentModel;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MsnMessenger.Models;
using MsnMessenger.Services;
using MsnMessenger.ViewModels;

namespace MsnMessenger;

public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private readonly IMsnDataService _dataService;
    private readonly MainViewModel _viewModel;
    private Contact? _selectedContact;

    // Splitter drag state
    private bool _isDraggingSplitter;
    private double _dragStartX;
    private double _initialSidebarWidth;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPage()
    {
        this.InitializeComponent();

        _dataService = new MsnDataService();
        _viewModel = new MainViewModel(_dataService);

        this.Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        BuddiesView.DataService = _dataService;
        BuddiesView.OnContactSelected += OnContactSelected;

        ChatOverlay.DataService = _dataService;
        ChatOverlay.OnBackRequested += OnChatBackRequested;

        // Start background orb animations
        StartOrbAnimations();
        StartButterflyAnimation();
        StartFloatingButterflyAnimation();
    }

    // Visibility properties - Sidebar is always visible in new layout
    public Visibility IsBuddyListVisible => Visibility.Visible;
    public Visibility IsChatVisible => _selectedContact != null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsEmptyStateVisible => _selectedContact == null ? Visibility.Visible : Visibility.Collapsed;

    private void OnContactSelected(Contact contact)
    {
        _selectedContact = contact;
        ChatOverlay.LoadContact(contact);
        NotifyAllProperties();
    }

    private void OnChatBackRequested()
    {
        _selectedContact = null;
        NotifyAllProperties();
    }

    private void NotifyAllProperties()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBuddyListVisible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChatVisible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmptyStateVisible)));
    }

    #region Animations

    private void StartOrbAnimations()
    {
        // Teal Orb - 20s cycle, gentle movement
        StartFloatingAnimation(TealOrbTransform, 30, 20, TimeSpan.FromSeconds(20));

        // Pink Orb - 25s cycle
        StartFloatingAnimation(PinkOrbTransform, -25, -15, TimeSpan.FromSeconds(25));

        // Purple Orb - 18s cycle
        StartFloatingAnimation(PurpleOrbTransform, 20, -25, TimeSpan.FromSeconds(18));
    }

    private void StartFloatingAnimation(TranslateTransform transform, double targetX, double targetY, TimeSpan duration)
    {
        AnimateOrbToPosition(transform, targetX, targetY, duration);
    }

    private void AnimateOrbToPosition(TranslateTransform transform, double targetX, double targetY, TimeSpan duration)
    {
        var animX = new DoubleAnimation
        {
            To = targetX,
            Duration = new Duration(duration),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var animY = new DoubleAnimation
        {
            To = targetY,
            Duration = new Duration(duration),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animX);
        storyboard.Children.Add(animY);

        Storyboard.SetTarget(animX, transform);
        Storyboard.SetTargetProperty(animX, "X");
        Storyboard.SetTarget(animY, transform);
        Storyboard.SetTargetProperty(animY, "Y");

        storyboard.Completed += (s, e) =>
        {
            // Reverse direction
            var nextX = transform.X == 0 ? targetX : 0;
            var nextY = transform.Y == 0 ? targetY : 0;
            AnimateOrbToPosition(transform, nextX, nextY, duration);
        };

        storyboard.Begin();
    }

    private void StartButterflyAnimation()
    {
        // Gentle rotation oscillation -5° to 5°
        AnimateButterflyRotation(5);
    }

    private void AnimateButterflyRotation(double targetRotation)
    {
        var rotationAnim = new DoubleAnimation
        {
            To = targetRotation,
            Duration = new Duration(TimeSpan.FromSeconds(3)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var scaleAnim = new DoubleAnimation
        {
            To = targetRotation > 0 ? 1.1 : 1.0,
            Duration = new Duration(TimeSpan.FromSeconds(3)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(rotationAnim);
        storyboard.Children.Add(scaleAnim);

        Storyboard.SetTarget(rotationAnim, ButterflyTransform);
        Storyboard.SetTargetProperty(rotationAnim, "Rotation");
        Storyboard.SetTarget(scaleAnim, ButterflyTransform);
        Storyboard.SetTargetProperty(scaleAnim, "ScaleX");

        storyboard.Completed += (s, e) =>
        {
            AnimateButterflyRotation(-targetRotation);
        };

        storyboard.Begin();
    }

    private void StartFloatingButterflyAnimation()
    {
        AnimateFloatingButterfly(15);
    }

    private void AnimateFloatingButterfly(double targetY)
    {
        var anim = new DoubleAnimation
        {
            To = targetY,
            Duration = new Duration(TimeSpan.FromSeconds(2)),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(anim);

        Storyboard.SetTarget(anim, FloatingButterflyTransform);
        Storyboard.SetTargetProperty(anim, "Y");

        storyboard.Completed += (s, e) =>
        {
            AnimateFloatingButterfly(-targetY);
        };

        storyboard.Begin();
    }

    #endregion

    #region Splitter Resize

    private void OnSplitterPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border splitter)
        {
            _isDraggingSplitter = true;
            _dragStartX = e.GetCurrentPoint(this).Position.X;
            _initialSidebarWidth = SidebarColumn.ActualWidth;
            splitter.CapturePointer(e.Pointer);
            e.Handled = true;
        }
    }

    private void OnSplitterPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDraggingSplitter) return;

        var currentX = e.GetCurrentPoint(this).Position.X;
        var delta = currentX - _dragStartX;
        var newWidth = _initialSidebarWidth + delta;

        // Clamp to min/max
        newWidth = Math.Clamp(newWidth, 250, 450);
        SidebarColumn.Width = new GridLength(newWidth);

        e.Handled = true;
    }

    private void OnSplitterPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border splitter)
        {
            _isDraggingSplitter = false;
            splitter.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }

    private void OnSplitterPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        // Highlight splitter on hover
        SplitterLine.Background = (Brush)Application.Current.Resources["TealPrimaryBrush"];
        SplitterLine.Width = 3;
    }

    private void OnSplitterPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDraggingSplitter)
        {
            // Reset splitter appearance
            SplitterLine.Background = (Brush)Application.Current.Resources["GlassBorderBrush"];
            SplitterLine.Width = 2;
        }
    }

    #endregion
}
