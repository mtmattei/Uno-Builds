using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;

namespace FreewriteUno.InlineAi.Presentation.Controls;

public sealed partial class InlineAiView : UserControl
{
    public InlineAiView()
    {
        this.InitializeComponent();

        // Keep the latest line in view as tokens stream in / turns are added.
        StreamFeed.SizeChanged += (_, _) =>
            StreamScroller.ChangeView(null, StreamScroller.ScrollableHeight, null, disableAnimation: true);
    }

    /// <summary>Called by the host when the card opens so focus lands in the composer.</summary>
    public void FocusComposer() => ComposerBox.Focus(FocusState.Programmatic);

    private void OnComposerKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
        {
            return;
        }

        // Shift+Enter inserts a newline; plain Enter sends.
        var shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
        if (shift.HasFlag(CoreVirtualKeyStates.Down))
        {
            return;
        }

        e.Handled = true;
        if (SendButton.Command?.CanExecute(null) == true)
        {
            SendButton.Command.Execute(null);
        }
    }
}
