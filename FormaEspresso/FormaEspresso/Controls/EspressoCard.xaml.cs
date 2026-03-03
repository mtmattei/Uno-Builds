using Microsoft.UI.Xaml.Media;
using FormaEspresso.Models;

namespace FormaEspresso.Controls;

public sealed partial class EspressoCard : UserControl
{
    private static readonly SolidColorBrush Stone900 = new(Windows.UI.Color.FromArgb(255, 28, 25, 23));
    private static readonly SolidColorBrush Stone800 = new(Windows.UI.Color.FromArgb(255, 41, 37, 36));
    private static readonly SolidColorBrush Stone400 = new(Windows.UI.Color.FromArgb(255, 168, 162, 158));
    private static readonly SolidColorBrush Stone200 = new(Windows.UI.Color.FromArgb(255, 231, 229, 227));
    private static readonly SolidColorBrush Stone100 = new(Windows.UI.Color.FromArgb(255, 245, 245, 244));
    private static readonly SolidColorBrush White = new(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    private static readonly SolidColorBrush White60 = new(Windows.UI.Color.FromArgb(153, 255, 255, 255));
    private static readonly SolidColorBrush White70 = new(Windows.UI.Color.FromArgb(179, 255, 255, 255));
    private static readonly SolidColorBrush White50 = new(Windows.UI.Color.FromArgb(128, 255, 255, 255));
    private static readonly SolidColorBrush White10 = new(Windows.UI.Color.FromArgb(26, 255, 255, 255));
    private static readonly SolidColorBrush Amber700 = new(Windows.UI.Color.FromArgb(255, 180, 83, 9));
    private static readonly SolidColorBrush Amber400 = new(Windows.UI.Color.FromArgb(255, 251, 191, 36));
    private static readonly SolidColorBrush Transparent = new(Windows.UI.Color.FromArgb(0, 0, 0, 0));

    public EspressoCard()
    {
        this.InitializeComponent();
    }

    public static readonly DependencyProperty ItemProperty =
        DependencyProperty.Register(nameof(Item), typeof(EspressoItem), typeof(EspressoCard), new PropertyMetadata(null));

    public EspressoItem Item
    {
        get => (EspressoItem)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(EspressoCard),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EspressoCard card)
        {
            var isSelected = (bool)e.NewValue;
            var storyboard = isSelected
                ? card.Resources["SelectAnimation"] as Storyboard
                : card.Resources["DeselectAnimation"] as Storyboard;

            storyboard?.Begin();

            if (isSelected)
            {
                card.CardBorder.Translation = new System.Numerics.Vector3(0, 0, 32);
            }
            else
            {
                card.CardBorder.Translation = new System.Numerics.Vector3(0, 0, 0);
            }
        }
    }

    public static Brush GetCardBackground(bool isSelected) => isSelected ? Stone900 : White;
    public static Brush GetTextForeground(bool isSelected) => isSelected ? White : Stone900;
    public static Brush GetNoteColor(bool isSelected) => isSelected ? White60 : Stone400;
    public static Brush GetDescriptionColor(bool isSelected) => isSelected ? White70 : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 120, 113, 108));
    public static Brush GetLabelColor(bool isSelected) => isSelected ? White50 : Stone400;
    public static Brush GetBadgeBackground(bool isSelected) => isSelected ? White10 : Stone100;
    public static Brush GetBadgeText(bool isSelected) => isSelected ? White70 : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 120, 113, 108));
    public static Brush GetDividerColor(bool isSelected) => isSelected ? White10 : Stone100;
    public static Brush GetCheckBorder(bool isSelected) => isSelected ? Amber400 : Stone200;
    public static Brush GetCheckBackground(bool isSelected) => isSelected ? Amber400 : Transparent;

    public static Brush GetIntensityBar(int intensity, int barIndex, bool isSelected)
    {
        var isActive = intensity >= barIndex;
        if (isSelected)
        {
            return isActive ? Amber400 : White10;
        }
        return isActive ? Amber700 : Stone200;
    }
}
