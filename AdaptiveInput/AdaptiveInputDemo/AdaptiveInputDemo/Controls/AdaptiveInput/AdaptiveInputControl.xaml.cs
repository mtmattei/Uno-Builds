using AdaptiveInputDemo.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.System;
using Windows.UI;

namespace AdaptiveInputDemo.Controls;

/// <summary>
/// Adaptive Input control that detects input type and shows appropriate picker.
/// </summary>
public sealed partial class AdaptiveInputControl : UserControl
{
    private readonly InputTypeDetector _detector = new();
    private DetectedInputType _currentType = DetectedInputType.None;
    private bool _isPanelOpen;

    // Picker controls - lazy initialized
    private DatePickerPanel? _datePicker;
    private ColorPickerPanel? _colorPicker;
    private MentionPickerPanel? _mentionPicker;
    private TagPickerPanel? _tagPicker;
    private RangePickerPanel? _rangePicker;

    // Cached brushes for performance
    private static readonly SolidColorBrush WhiteBrush = new(Color.FromArgb(255, 255, 255, 255));
    private Brush? _primaryBrush;
    private Brush? _outlineBrush;
    private Brush? _surfaceVariantBrush;
    private Brush? _onSurfaceVariantBrush;

    public AdaptiveInputControl()
    {
        InitializeComponent();
        CacheBrushes();
    }

    private void CacheBrushes()
    {
        _primaryBrush = Application.Current.Resources["PrimaryBrush"] as Brush;
        _outlineBrush = Application.Current.Resources["OutlineBrush"] as Brush;
        _surfaceVariantBrush = Application.Current.Resources["SurfaceVariantBrush"] as Brush;
        _onSurfaceVariantBrush = Application.Current.Resources["OnSurfaceVariantBrush"] as Brush;
    }

    #region Dependency Properties

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(AdaptiveInputControl),
            new PropertyMetadata("Type anything...", OnPlaceholderChanged));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdaptiveInputControl control)
        {
            control.InputTextBox.PlaceholderText = e.NewValue as string ?? "Type anything...";
        }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(AdaptiveInputControl),
            new PropertyMetadata(string.Empty, OnValueChanged));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdaptiveInputControl control && e.NewValue is string newValue)
        {
            if (control.InputTextBox.Text != newValue)
            {
                control.InputTextBox.Text = newValue;
            }
        }
    }

    public static readonly DependencyProperty IsDisabledProperty =
        DependencyProperty.Register(nameof(IsDisabled), typeof(bool), typeof(AdaptiveInputControl),
            new PropertyMetadata(false, OnIsDisabledChanged));

    public bool IsDisabled
    {
        get => (bool)GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }

    private static void OnIsDisabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdaptiveInputControl control)
        {
            control.InputTextBox.IsEnabled = !(bool)e.NewValue;
            control.SubmitButton.IsEnabled = !(bool)e.NewValue;
        }
    }

    #endregion

    #region Events

    public event EventHandler<InputSubmittedEventArgs>? Submitted;
    public event EventHandler<InputChangedEventArgs>? InputChanged;
    public event EventHandler<TypeDetectedEventArgs>? TypeDetected;

    #endregion

    #region Event Handlers

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var text = InputTextBox.Text;
        Value = text;

        var result = _detector.Detect(text);
        var previousType = _currentType;
        _currentType = result.Type;

        // Update badge
        UpdateTypeBadge(result.Type);

        // Fire events
        InputChanged?.Invoke(this, new InputChangedEventArgs(result.Type, text));

        if (previousType != result.Type)
        {
            TypeDetected?.Invoke(this, new TypeDetectedEventArgs(result.Type));

            // Show/update panel if type changed and is actionable
            if (result.Type != DetectedInputType.None &&
                result.Type != DetectedInputType.Url &&
                result.Type != DetectedInputType.Email)
            {
                ShowPanel(result.Type, text);
            }
            else
            {
                HidePanel();
            }
        }
        else if (_isPanelOpen)
        {
            // Update existing panel content
            UpdatePanelContent(result.Type, text);
        }
    }

    private void OnInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Enter:
                if (!_isPanelOpen)
                {
                    SubmitValue();
                }
                e.Handled = true;
                break;

            case VirtualKey.Escape:
                if (_isPanelOpen)
                {
                    HidePanel();
                    e.Handled = true;
                }
                break;

            case VirtualKey.Tab:
                if (_isPanelOpen && PickerContent.Content is Control pickerControl)
                {
                    pickerControl.Focus(FocusState.Keyboard);
                    e.Handled = true;
                }
                break;
        }
    }

    private void OnInputGotFocus(object sender, RoutedEventArgs e)
    {
        InputContainer.BorderBrush = _primaryBrush;
        // Hide placeholder on focus
        InputTextBox.PlaceholderText = string.Empty;
    }

    private void OnInputLostFocus(object sender, RoutedEventArgs e)
    {
        InputContainer.BorderBrush = _outlineBrush;
        // Restore placeholder if text is empty
        if (string.IsNullOrEmpty(InputTextBox.Text))
        {
            InputTextBox.PlaceholderText = Placeholder;
        }
    }

    private void OnSubmitClicked(object sender, RoutedEventArgs e)
    {
        SubmitValue();
    }

    private void OnClosePanel(object sender, RoutedEventArgs e)
    {
        HidePanel();
        InputTextBox.Focus(FocusState.Programmatic);
    }

    #endregion

    #region Badge Updates

    private void UpdateTypeBadge(DetectedInputType type)
    {
        var (icon, label, colorKey) = InputTypeDetector.GetTypeDisplayInfo(type);

        TypeIcon.Glyph = icon;
        TypeLabel.Text = label;

        // Update badge background color
        if (Application.Current.Resources.TryGetValue(colorKey, out var colorObj) && colorObj is Color color)
        {
            TypeBadge.Background = new SolidColorBrush(color);
            TypeIcon.Foreground = WhiteBrush;
            TypeLabel.Foreground = WhiteBrush;
        }
        else
        {
            TypeBadge.Background = _surfaceVariantBrush!;
            TypeIcon.Foreground = _onSurfaceVariantBrush!;
            TypeLabel.Foreground = _onSurfaceVariantBrush!;
        }
    }

    #endregion

    #region Panel Management

    private void ShowPanel(DetectedInputType type, string currentValue)
    {
        var picker = GetOrCreatePicker(type);
        if (picker == null) return;

        UpdatePanelHeader(type);
        PickerContent.Content = picker;
        UpdatePanelContent(type, currentValue);

        if (!_isPanelOpen)
        {
            _isPanelOpen = true;
            PanelContainer.Visibility = Visibility.Visible;
            AnimatePanelIn();
        }
    }

    private void HidePanel()
    {
        if (_isPanelOpen)
        {
            _isPanelOpen = false;
            AnimatePanelOut();
        }
    }

    private void UpdatePanelHeader(DetectedInputType type)
    {
        var (icon, label, _) = InputTypeDetector.GetTypeDisplayInfo(type);
        PanelIcon.Glyph = icon;
        PanelTitle.Text = $"Select {label}";
    }

    private Control? GetOrCreatePicker(DetectedInputType type)
    {
        return type switch
        {
            DetectedInputType.Date => _datePicker ??= CreateDatePicker(),
            DetectedInputType.Color => _colorPicker ??= CreateColorPicker(),
            DetectedInputType.Mention => _mentionPicker ??= CreateMentionPicker(),
            DetectedInputType.Tag => _tagPicker ??= CreateTagPicker(),
            DetectedInputType.NumberRange => _rangePicker ??= CreateRangePicker(),
            _ => null
        };
    }

    private void UpdatePanelContent(DetectedInputType type, string currentValue)
    {
        switch (type)
        {
            case DetectedInputType.Mention:
                _mentionPicker?.UpdateFilter(currentValue.TrimStart('@'));
                break;
            case DetectedInputType.Tag:
                _tagPicker?.UpdateFilter(currentValue.TrimStart('#'));
                break;
            case DetectedInputType.Color:
                _colorPicker?.UpdateValue(currentValue);
                break;
            case DetectedInputType.NumberRange:
                _rangePicker?.UpdateValue(currentValue);
                break;
        }
    }

    #endregion

    #region Picker Factories

    private DatePickerPanel CreateDatePicker()
    {
        var picker = new DatePickerPanel();
        picker.DateSelected += (s, date) =>
        {
            InputTextBox.Text = date.ToString("MMM d, yyyy");
            HidePanel();
            InputTextBox.Focus(FocusState.Programmatic);
        };
        return picker;
    }

    private ColorPickerPanel CreateColorPicker()
    {
        var picker = new ColorPickerPanel();
        picker.ColorSelected += (s, color) =>
        {
            InputTextBox.Text = color;
            HidePanel();
            InputTextBox.Focus(FocusState.Programmatic);
        };
        return picker;
    }

    private MentionPickerPanel CreateMentionPicker()
    {
        var picker = new MentionPickerPanel();
        picker.MentionSelected += (s, mention) =>
        {
            InputTextBox.Text = $"@{mention}";
            HidePanel();
            InputTextBox.Focus(FocusState.Programmatic);
        };
        return picker;
    }

    private TagPickerPanel CreateTagPicker()
    {
        var picker = new TagPickerPanel();
        picker.TagSelected += (s, tag) =>
        {
            InputTextBox.Text = $"#{tag}";
            HidePanel();
            InputTextBox.Focus(FocusState.Programmatic);
        };
        return picker;
    }

    private RangePickerPanel CreateRangePicker()
    {
        var picker = new RangePickerPanel();
        picker.RangeSelected += (s, range) =>
        {
            InputTextBox.Text = range;
            HidePanel();
            InputTextBox.Focus(FocusState.Programmatic);
        };
        return picker;
    }

    #endregion

    #region Animations

    private void AnimatePanelIn()
    {
        var storyboard = new Storyboard();

        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(opacityAnimation, PanelContainer);
        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

        var translateAnimation = new DoubleAnimation
        {
            From = -8,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(translateAnimation, PanelContainer);
        Storyboard.SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

        storyboard.Children.Add(opacityAnimation);
        storyboard.Children.Add(translateAnimation);
        storyboard.Begin();
    }

    private void AnimatePanelOut()
    {
        var storyboard = new Storyboard();

        var opacityAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        Storyboard.SetTarget(opacityAnimation, PanelContainer);
        Storyboard.SetTargetProperty(opacityAnimation, "Opacity");

        var translateAnimation = new DoubleAnimation
        {
            From = 0,
            To = -4,
            Duration = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        Storyboard.SetTarget(translateAnimation, PanelContainer);
        Storyboard.SetTargetProperty(translateAnimation, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

        storyboard.Completed += (s, e) =>
        {
            PanelContainer.Visibility = Visibility.Collapsed;
        };

        storyboard.Children.Add(opacityAnimation);
        storyboard.Children.Add(translateAnimation);
        storyboard.Begin();
    }

    #endregion

    #region Submit

    private void SubmitValue()
    {
        var result = _detector.Detect(InputTextBox.Text);
        Submitted?.Invoke(this, new InputSubmittedEventArgs(result.Type, InputTextBox.Text));
        HidePanel();
    }

    #endregion

    #region Public Methods

    public void Clear()
    {
        InputTextBox.Text = string.Empty;
        HidePanel();
    }

    public void Focus()
    {
        InputTextBox.Focus(FocusState.Programmatic);
    }

    #endregion
}

#region Event Args

public class InputSubmittedEventArgs : EventArgs
{
    public DetectedInputType Type { get; }
    public string Value { get; }

    public InputSubmittedEventArgs(DetectedInputType type, string value)
    {
        Type = type;
        Value = value;
    }
}

public class InputChangedEventArgs : EventArgs
{
    public DetectedInputType Type { get; }
    public string Value { get; }

    public InputChangedEventArgs(DetectedInputType type, string value)
    {
        Type = type;
        Value = value;
    }
}

public class TypeDetectedEventArgs : EventArgs
{
    public DetectedInputType Type { get; }

    public TypeDetectedEventArgs(DetectedInputType type)
    {
        Type = type;
    }
}

#endregion
