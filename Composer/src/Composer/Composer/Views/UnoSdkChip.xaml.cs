using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Composer.Views;

public sealed partial class UnoSdkChip : UserControl
{
    public static readonly DependencyProperty VersionProperty =
        DependencyProperty.Register(
            nameof(Version), typeof(string), typeof(UnoSdkChip),
            new PropertyMetadata("6.5.29", OnVersionChanged));

    public string Version
    {
        get => (string)GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    public UnoSdkChip()
    {
        this.InitializeComponent();
        Render();
    }

    private static void OnVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UnoSdkChip chip) chip.Render();
    }

    private void Render()
    {
        Label.Text = $"UNO.SDK · {Version}";
        Tooltip.Text = $"Uno.Sdk {Version} — latest stable from NuGet";
    }
}
