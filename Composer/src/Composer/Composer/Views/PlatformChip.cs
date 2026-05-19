using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Composer.Views;

/// <summary>
/// Toggleable chip for the foundation panel's platform row. Morphs between an
/// outlined pill with a text label (unselected) and a 28×28 ink-filled circle
/// with a white platform icon (selected). Multi-select; each chip toggles
/// independently. Implemented per the PlatformChip brief in DESIGN.md.
/// </summary>
public sealed class PlatformChip : Control
{
    public static readonly DependencyProperty PlatformKindProperty =
        DependencyProperty.Register(
            nameof(PlatformKind), typeof(PlatformKind), typeof(PlatformChip),
            new PropertyMetadata(PlatformKind.Web, OnPlatformKindChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(PlatformChip),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public PlatformKind PlatformKind
    {
        get => (PlatformKind)GetValue(PlatformKindProperty);
        set => SetValue(PlatformKindProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public event EventHandler<bool>? Toggled;

    private bool _pointerInside;

    public PlatformChip()
    {
        DefaultStyleKey = typeof(PlatformChip);
        IsTabStop = true;

        PointerPressed  += (_, _) => VisualStateManager.GoToState(this, "Pressed", MotionPreferences.AnimationsEnabled);
        PointerReleased += (_, _) =>
        {
            ToggleByUser();
            VisualStateManager.GoToState(this, _pointerInside ? "PointerOver" : "Normal", MotionPreferences.AnimationsEnabled);
        };
        PointerEntered += (_, _) =>
        {
            _pointerInside = true;
            VisualStateManager.GoToState(this, "PointerOver", MotionPreferences.AnimationsEnabled);
        };
        PointerExited += (_, _) =>
        {
            _pointerInside = false;
            VisualStateManager.GoToState(this, "Normal", MotionPreferences.AnimationsEnabled);
        };
        PointerCaptureLost += (_, _) =>
        {
            _pointerInside = false;
            VisualStateManager.GoToState(this, "Normal", MotionPreferences.AnimationsEnabled);
        };

        KeyDown += (_, e) =>
        {
            if (e.Key is VirtualKey.Space or VirtualKey.Enter)
            {
                ToggleByUser();
                e.Handled = true;
            }
        };
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        VisualStateManager.GoToState(this, IsSelected ? "Selected" : "Unselected", false);
    }

    private void ToggleByUser()
    {
        IsSelected = !IsSelected;
        Toggled?.Invoke(this, IsSelected);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PlatformChip chip)
            VisualStateManager.GoToState(chip, chip.IsSelected ? "Selected" : "Unselected", MotionPreferences.AnimationsEnabled);
    }

    private static void OnPlatformKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Icon is bound to PlatformKind via the template's ContentPresenter +
        // template selector — re-evaluation happens automatically.
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new PlatformChipAutomationPeer(this);

    internal sealed class PlatformChipAutomationPeer : FrameworkElementAutomationPeer, IToggleProvider
    {
        public PlatformChipAutomationPeer(PlatformChip owner) : base(owner) { }

        public ToggleState ToggleState =>
            ((PlatformChip)Owner).IsSelected ? ToggleState.On : ToggleState.Off;

        public void Toggle()
        {
            var chip = (PlatformChip)Owner;
            chip.IsSelected = !chip.IsSelected;
            chip.Toggled?.Invoke(chip, chip.IsSelected);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
            => AutomationControlType.CheckBox;

        protected override object GetPatternCore(PatternInterface patternInterface)
            => patternInterface == PatternInterface.Toggle ? this : base.GetPatternCore(patternInterface);

        protected override string GetNameCore()
            => ((PlatformChip)Owner).PlatformKind.DisplayName();
    }
}
