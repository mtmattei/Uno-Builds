using Caffe.Models;

namespace Caffe.Controls;

public sealed partial class EspressoCard : UserControl
{
    public static readonly DependencyProperty EspressoProperty =
        DependencyProperty.Register(
            nameof(Espresso),
            typeof(EspressoItem),
            typeof(EspressoCard),
            new PropertyMetadata(null, OnEspressoChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(EspressoCard),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public EspressoItem? Espresso
    {
        get => (EspressoItem?)GetValue(EspressoProperty);
        set => SetValue(EspressoProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public EspressoCard()
    {
        this.InitializeComponent();
    }

    private static void OnEspressoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EspressoCard card && e.NewValue is EspressoItem item)
        {
            card.VolumeText.Text = item.VolumeDisplay;
            card.NameText.Text = item.Name;
            card.DescriptionText.Text = item.Description;
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(card, $"{item.Name} espresso, {item.VolumeDisplay}");
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EspressoCard card)
        {
            var isSelected = (bool)e.NewValue;
            card.CardBorder.BorderBrush = isSelected
                ? (SolidColorBrush)Application.Current.Resources["CaffePrimaryBrush"]
                : (SolidColorBrush)Application.Current.Resources["CaffeBorderBrush"];

            card.CheckmarkBorder.Visibility = isSelected
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
