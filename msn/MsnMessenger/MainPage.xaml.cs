using System.ComponentModel;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using MsnMessenger.Models;
using MsnMessenger.Services;

namespace MsnMessenger;

public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private readonly IMsnDataService _dataService;
    private Contact? _selectedContact;

    private bool _isDraggingSplitter;
    private double _dragStartX;
    private double _initialSidebarWidth;

    private readonly List<Storyboard> _ambientStoryboards = new();
    private bool _ambientAnimationsStarted;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPage()
    {
        this.InitializeComponent();

        _dataService = App.Services?.GetService<IMsnDataService>()
            ?? new MsnDataService();

        this.Loaded += OnPageLoaded;
        this.Unloaded += OnPageUnloaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        BuddiesView.DataService = _dataService;
        BuddiesView.OnContactSelected += OnContactSelected;

        ChatOverlay.DataService = _dataService;
        ChatOverlay.OnBackRequested += OnChatBackRequested;

        StartAmbientAnimations();
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        BuddiesView.OnContactSelected -= OnContactSelected;
        ChatOverlay.OnBackRequested -= OnChatBackRequested;

        foreach (var sb in _ambientStoryboards)
        {
            sb.Stop();
        }
        _ambientStoryboards.Clear();
        _ambientAnimationsStarted = false;
    }

    public Visibility IsBuddyListVisible => Visibility.Visible;
    public Visibility IsChatVisible => _selectedContact is not null ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsEmptyStateVisible => _selectedContact is null ? Visibility.Visible : Visibility.Collapsed;

    private void OnContactSelected(Contact contact)
    {
        _selectedContact = contact;
        ChatOverlay.LoadContact(contact);
        NotifyVisibilityChanged();
    }

    private void OnChatBackRequested()
    {
        _selectedContact = null;
        NotifyVisibilityChanged();
    }

    private void NotifyVisibilityChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBuddyListVisible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChatVisible)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmptyStateVisible)));
    }

    private void StartAmbientAnimations()
    {
        // Loaded can re-fire on template re-mount; only start once per attached lifetime.
        if (_ambientAnimationsStarted) return;
        _ambientAnimationsStarted = true;

        StartFloatingOrb(TealOrbTransform, 30, 20, TimeSpan.FromSeconds(20));
        StartFloatingOrb(PinkOrbTransform, -25, -15, TimeSpan.FromSeconds(25));
        StartFloatingOrb(PurpleOrbTransform, 20, -25, TimeSpan.FromSeconds(18));

        StartButterflyOscillation();
        StartFloatingButterfly();
    }

    private void StartFloatingOrb(TranslateTransform transform, double targetX, double targetY, TimeSpan duration)
    {
        var sb = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
        sb.Children.Add(BuildBounceAnimation(transform, "X", 0, targetX, duration));
        sb.Children.Add(BuildBounceAnimation(transform, "Y", 0, targetY, duration));
        _ambientStoryboards.Add(sb);
        sb.Begin();
    }

    private void StartButterflyOscillation()
    {
        var sb = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
        sb.Children.Add(BuildBounceAnimation(ButterflyTransform, "Rotation", 0, 5, TimeSpan.FromSeconds(3)));
        sb.Children.Add(BuildBounceAnimation(ButterflyTransform, "ScaleX", 1.0, 1.1, TimeSpan.FromSeconds(3)));
        _ambientStoryboards.Add(sb);
        sb.Begin();
    }

    private void StartFloatingButterfly()
    {
        var sb = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };
        sb.Children.Add(BuildBounceAnimation(FloatingButterflyTransform, "Y", 0, 15, TimeSpan.FromSeconds(2)));
        _ambientStoryboards.Add(sb);
        sb.Begin();
    }

    private static DoubleAnimationUsingKeyFrames BuildBounceAnimation(DependencyObject target, string property, double start, double peak, TimeSpan halfDuration)
    {
        var ease = new SineEase { EasingMode = EasingMode.EaseInOut };
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = start, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = halfDuration, Value = peak, EasingFunction = ease });
        anim.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = halfDuration + halfDuration, Value = start, EasingFunction = ease });
        Storyboard.SetTarget(anim, target);
        Storyboard.SetTargetProperty(anim, property);
        return anim;
    }

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
        var newWidth = Math.Clamp(_initialSidebarWidth + delta, 250, 450);
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
        SplitterLine.Background = (Brush)Application.Current.Resources["TealPrimaryBrush"];
        SplitterLine.Width = 3;
    }

    private void OnSplitterPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (_isDraggingSplitter) return;
        SplitterLine.Background = (Brush)Application.Current.Resources["GlassBorderBrush"];
        SplitterLine.Width = 2;
    }
}
