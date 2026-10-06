using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FreewriteUno.InlineAi.Presentation.Controls;

public sealed partial class ActionPillView : UserControl
{
    /// <summary>Raised when either segment is tapped, so the host can dismiss the pill as the chat opens.</summary>
    public event RoutedEventHandler? Activated;

    public ActionPillView()
    {
        this.InitializeComponent();
    }

    private void OnSegmentClick(object sender, RoutedEventArgs e) => Activated?.Invoke(this, e);
}
