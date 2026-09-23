using FieldCheck.Services;
using FieldCheck.ViewModels;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace FieldCheck.Views;

public sealed partial class ShellPage : Page
{
    /// <summary>Window width from which Assets shows master/detail (DESIGN_SPEC adaptive rules).</summary>
    public const double MasterDetailMinWidth = 1000;

    private readonly FrameNavigator _navigator;

    public ShellPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ShellViewModel>();
        _navigator = App.Services.GetRequiredService<FrameNavigator>();
        _navigator.Attach(ContentFrame, () => ActualWidth >= MasterDetailMinWidth);
        _navigator.ShowSection(AppSection.Dashboard);

        // Alt+Left mirrors system back on keyboard platforms.
        var back = new KeyboardAccelerator { Key = VirtualKey.Left, Modifiers = VirtualKeyModifiers.Menu };
        back.Invoked += (_, e) => e.Handled = _navigator.TryHandleSystemBack();
        KeyboardAccelerators.Add(back);

#if HAS_UNO
        // Android system back button / gesture.
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (_, e) =>
        {
            if (!e.Handled)
            {
                e.Handled = _navigator.TryHandleSystemBack();
            }
        };
#endif
    }

    public ShellViewModel ViewModel { get; }
}
