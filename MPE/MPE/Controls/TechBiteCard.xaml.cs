using Microsoft.UI.Xaml.Controls;

namespace MPE.Controls;

public sealed partial class TechBiteCard : UserControl
{
    public string Title { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#FFF0F0F0";

    public TechBiteCard()
    {
        this.InitializeComponent();
    }
}
