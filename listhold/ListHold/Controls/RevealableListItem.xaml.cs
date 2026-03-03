using ListHold.Models;
using ListHold.ViewModels;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace ListHold.Controls;

public sealed partial class RevealableListItem : UserControl
{
    private ListItemViewModel? _viewModel;
    private bool _isPointerOverActionButton;
    private LinearGradientBrush? _holdGradientBrush;
    private Brush? _surfaceBrush;
    private bool _stage1Animating;
    private bool _stage2Animating;
    private bool _stage3Animating;
    private bool _buttonsCreated;

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ListItemViewModel),
            typeof(RevealableListItem),
            new PropertyMetadata(null, OnViewModelChanged));

    public ListItemViewModel? ViewModel
    {
        get => (ListItemViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public RevealableListItem()
    {
        this.InitializeComponent();
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RevealableListItem control)
        {
            control.SetupViewModel(e.NewValue as ListItemViewModel);
        }
    }

    private void SetupViewModel(ListItemViewModel? viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;

        if (_viewModel == null)
            return;

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        TitleText.Text = _viewModel.Item.Title;
        PreviewText.Text = _viewModel.Item.Preview;
        DetailsText.Text = _viewModel.Item.Details;
        MetaItemsControl.ItemsSource = _viewModel.Item.Meta.ToList();

        // Reset button creation flag for new viewmodel
        _buttonsCreated = false;

        UpdateVisualState();
    }

    private void CreateActionButtons()
    {
        if (_viewModel == null || _buttonsCreated) return;

        _buttonsCreated = true;
        var buttons = new List<Button>();
        for (int i = 0; i < _viewModel.Item.Actions.Count; i++)
        {
            var action = _viewModel.Item.Actions[i];
            var button = new Button
            {
                Content = action,
                Style = i == 0
                    ? (Style)Application.Current.Resources["PrimaryActionButtonStyle"]
                    : (Style)Application.Current.Resources["SecondaryActionButtonStyle"]
            };
            button.PointerPressed += OnActionButtonPointerPressed;
            buttons.Add(button);
        }
        ActionsItemsControl.ItemsSource = buttons;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(ListItemViewModel.HoldProgress):
                    ProgressRing.Progress = _viewModel?.HoldProgress ?? 0;
                    UpdateHoldBackground();
                    break;

                case nameof(ListItemViewModel.State):
                    ProgressRing.State = _viewModel?.State ?? HoldState.Idle;
                    UpdateLockedState();
                    break;

                case nameof(ListItemViewModel.ShowStage1):
                case nameof(ListItemViewModel.ShowStage2):
                case nameof(ListItemViewModel.ShowStage3):
                    UpdateStageVisibility();
                    break;

                case nameof(ListItemViewModel.ContainerScale):
                    UpdateContainerScale();
                    break;
            }
        });
    }

    private void UpdateVisualState()
    {
        if (_viewModel == null)
            return;

        ProgressRing.Progress = _viewModel.HoldProgress;
        ProgressRing.State = _viewModel.State;
        UpdateStageVisibility();
        UpdateLockedState();
        UpdateContainerScale();
        UpdateHoldBackground();
    }

    private void UpdateHoldBackground()
    {
        if (_viewModel == null) return;

        // Cache surface brush
        _surfaceBrush ??= (Brush)Application.Current.Resources["SurfaceBrush"];

        var progress = _viewModel.HoldProgress;
        if (progress > 0 && _viewModel.State == HoldState.Holding)
        {
            var maxOpacity = 0.08;
            var tintColor = Windows.UI.Color.FromArgb(
                (byte)(maxOpacity * 255),
                0x3b, 0x82, 0xf6);
            var halfTint = Windows.UI.Color.FromArgb(
                (byte)(maxOpacity * 0.5 * 255),
                0x3b, 0x82, 0xf6);
            var white = Windows.UI.Color.FromArgb(255, 255, 255, 255);

            // Smooth eased gradient positions
            var edgePos = Math.Min(progress * 1.1, 1.0);
            var fadeStart = Math.Max(0, edgePos - 0.25);
            var fadeMid = Math.Max(0, edgePos - 0.12);

            // Reuse or create gradient brush
            if (_holdGradientBrush == null)
            {
                _holdGradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(1, 0)
                };
                _holdGradientBrush.GradientStops.Add(new GradientStop { Color = tintColor, Offset = 0 });
                _holdGradientBrush.GradientStops.Add(new GradientStop { Color = tintColor, Offset = fadeStart });
                _holdGradientBrush.GradientStops.Add(new GradientStop { Color = halfTint, Offset = fadeMid });
                _holdGradientBrush.GradientStops.Add(new GradientStop { Color = white, Offset = edgePos });
                _holdGradientBrush.GradientStops.Add(new GradientStop { Color = white, Offset = 1 });
                ContainerBorder.Background = _holdGradientBrush;
            }
            else
            {
                // Update existing gradient stops
                _holdGradientBrush.GradientStops[1].Offset = fadeStart;
                _holdGradientBrush.GradientStops[2].Offset = fadeMid;
                _holdGradientBrush.GradientStops[3].Offset = edgePos;
            }
        }
        else if (_viewModel.State != HoldState.Locked)
        {
            _holdGradientBrush = null;
            ContainerBorder.Background = _surfaceBrush;
        }
    }

    private void UpdateStageVisibility()
    {
        if (_viewModel == null)
            return;

        AnimateStage1(_viewModel.ShowStage1);
        AnimateStage2(_viewModel.ShowStage2);
        AnimateStage3(_viewModel.ShowStage3);
    }

    private void AnimateStage1(bool show)
    {
        if (show && Stage1Container.Visibility == Visibility.Collapsed && !_stage1Animating)
        {
            _stage1Animating = true;
            RunStageAnimation(Stage1Container, 0, () => _stage1Animating = false);
        }
        else if (!show && Stage1Container.Visibility == Visibility.Visible)
        {
            _stage1Animating = false;
            Stage1Container.Visibility = Visibility.Collapsed;
            Stage1Container.Opacity = 0;
        }
    }

    private void AnimateStage2(bool show)
    {
        if (show && Stage2Container.Visibility == Visibility.Collapsed && !_stage2Animating)
        {
            _stage2Animating = true;
            RunStageAnimation(Stage2Container, 50, () => _stage2Animating = false);
        }
        else if (!show && Stage2Container.Visibility == Visibility.Visible)
        {
            _stage2Animating = false;
            Stage2Container.Visibility = Visibility.Collapsed;
            Stage2Container.Opacity = 0;
        }
    }

    private void AnimateStage3(bool show)
    {
        if (show && Stage3Container.Visibility == Visibility.Collapsed && !_stage3Animating)
        {
            _stage3Animating = true;
            // Ensure buttons are created before animating (only once)
            CreateActionButtons();
            RunStageAnimation(Stage3Container, 100, () => _stage3Animating = false);
        }
        else if (!show && Stage3Container.Visibility == Visibility.Visible)
        {
            _stage3Animating = false;
            Stage3Container.Visibility = Visibility.Collapsed;
            Stage3Container.Opacity = 0;
        }
    }

    private void RunStageAnimation(Border container, int delayMs, Action onComplete)
    {
        container.Visibility = Visibility.Visible;
        container.Opacity = 0;
        container.RenderTransform = new TranslateTransform { Y = -8 };

        var storyboard = new Storyboard();

        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(opacityAnimation, container);
        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");
        storyboard.Children.Add(opacityAnimation);

        var translateAnimation = new DoubleAnimation
        {
            From = -8,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(delayMs),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(translateAnimation, container.RenderTransform);
        Storyboard.SetTargetProperty(translateAnimation, "Y");
        storyboard.Children.Add(translateAnimation);

        storyboard.Completed += (s, e) => onComplete();
        storyboard.Begin();
    }

    private void UpdateLockedState()
    {
        if (_viewModel == null)
            return;

        var isLocked = _viewModel.State == HoldState.Locked;

        if (isLocked)
        {
            // Blue-tinted shadow and subtle border glow for locked state
            ContainerBorder.Translation = new System.Numerics.Vector3(0, 0, 16);
            ContainerBorder.Shadow = new ThemeShadow();
            ContainerBorder.Background = (Brush)Application.Current.Resources["SurfaceBrush"];
            ContainerBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 0x3b, 0x82, 0xf6));
            ContainerBorder.BorderThickness = new Thickness(1);
            HoldHintText.Opacity = 0;
        }
        else
        {
            ContainerBorder.Translation = new System.Numerics.Vector3(0, 0, 0);
            ContainerBorder.Shadow = null;
            ContainerBorder.BorderBrush = null;
            ContainerBorder.BorderThickness = new Thickness(0);
        }
    }

    private void UpdateContainerScale()
    {
        if (_viewModel == null)
            return;

        ContainerScale.ScaleX = _viewModel.ContainerScale;
        ContainerScale.ScaleY = _viewModel.ContainerScale;
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (_viewModel?.State == HoldState.Idle)
        {
            // Show hint and subtle hover shadow only when not locked
            HoldHintText.Opacity = 1;
            ContainerBorder.Translation = new System.Numerics.Vector3(0, 0, 4);
            ContainerBorder.Shadow = new ThemeShadow();
        }
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_isPointerOverActionButton)
            return;

        _viewModel?.StartHold();
        (sender as UIElement)?.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _viewModel?.EndHold();
        (sender as UIElement)?.ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _viewModel?.EndHold();

        // Hide hint and remove hover shadow
        HoldHintText.Opacity = 0;
        if (_viewModel?.State != HoldState.Locked)
        {
            ContainerBorder.Translation = new System.Numerics.Vector3(0, 0, 0);
            ContainerBorder.Shadow = null;
        }
    }

    private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        _viewModel?.EndHold();
        (sender as UIElement)?.ReleasePointerCapture(e.Pointer);
    }

    private void OnActionButtonPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isPointerOverActionButton = true;
        e.Handled = true;

        DispatcherQueue.TryEnqueue(() => _isPointerOverActionButton = false);
    }

    private void OnProgressRingPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_viewModel?.State == HoldState.Locked)
        {
            // Collapse when clicking the ring while locked
            _viewModel.CollapseCommand.Execute(null);

            // Reset visual state
            ContainerBorder.Translation = new System.Numerics.Vector3(0, 0, 0);
            ContainerBorder.Shadow = null;
            ContainerBorder.Background = (Brush)Application.Current.Resources["SurfaceBrush"];
            ContainerBorder.BorderBrush = null;
            ContainerBorder.BorderThickness = new Thickness(0);

            e.Handled = true;
        }
    }
}
