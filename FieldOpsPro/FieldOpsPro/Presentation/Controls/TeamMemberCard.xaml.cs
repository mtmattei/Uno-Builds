using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using FieldOpsPro.Models;

namespace FieldOpsPro.Presentation.Controls;

public sealed partial class TeamMemberCard : UserControl
{
    public TeamMemberCard()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAppearance();
    }

    public static readonly DependencyProperty MemberProperty =
        DependencyProperty.Register(nameof(Member), typeof(TeamMember), typeof(TeamMemberCard),
            new PropertyMetadata(null, OnMemberChanged));

    public TeamMember? Member
    {
        get => (TeamMember?)GetValue(MemberProperty);
        set => SetValue(MemberProperty, value);
    }

    private static void OnMemberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TeamMemberCard card)
        {
            card.UpdateAppearance();
        }
    }

    private void UpdateAppearance()
    {
        if (Member == null || NameText == null) return;

        NameText.Text = Member.Name;
        LocationText.Text = Member.LocationDescription;

        MemberAvatar.Initials = Member.Initials;
        MemberAvatar.AvatarColor = Member.AvatarColor;
        MemberAvatar.Status = Member.Status;

        UpdateBatteryIndicator();
        UpdateSignalIndicator();
    }

    private void UpdateBatteryIndicator()
    {
        if (BatteryFill == null || LowBatteryWarning == null || Member == null) return;

        var batteryLevel = Member.BatteryLevel;

        // Calculate fill width (max 16px for 100%)
        var fillWidth = Math.Max(0, Math.Min(16, batteryLevel * 16.0 / 100.0));
        BatteryFill.Width = fillWidth;

        // Set color based on battery level
        if (batteryLevel <= 20)
        {
            BatteryFill.Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 255, 255)); // White for critical
            LowBatteryWarning.Visibility = Visibility.Visible;
            LowBatteryWarning.Foreground = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 255, 255)); // White warning
        }
        else if (batteryLevel <= 40)
        {
            BatteryFill.Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 192, 192, 192)); // Light grey for low
            LowBatteryWarning.Visibility = Visibility.Collapsed;
        }
        else
        {
            BatteryFill.Background = new SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 255, 255, 255)); // White for normal
            LowBatteryWarning.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateSignalIndicator()
    {
        if (Signal1 == null || Member == null) return;

        var signalStrength = Member.SignalStrength; // 0-4 scale

        var activeBrush = new SolidColorBrush(
            Windows.UI.Color.FromArgb(255, 255, 255, 255)); // White
        var inactiveBrush = new SolidColorBrush(
            Windows.UI.Color.FromArgb(255, 64, 64, 64)); // Dark grey

        Signal1.Background = signalStrength >= 1 ? activeBrush : inactiveBrush;
        Signal2.Background = signalStrength >= 2 ? activeBrush : inactiveBrush;
        Signal3.Background = signalStrength >= 3 ? activeBrush : inactiveBrush;
        Signal4.Background = signalStrength >= 4 ? activeBrush : inactiveBrush;
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        // Animate lift effect
        var liftAnimation = new DoubleAnimation
        {
            To = -4,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(liftAnimation, CardTransform);
        Storyboard.SetTargetProperty(liftAnimation, "TranslateY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(liftAnimation);
        storyboard.Begin();

        // Glow border effect
        CardBorder.BorderBrush = new SolidColorBrush(ParseColor("#404040"));
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        // Animate back down
        var dropAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(dropAnimation, CardTransform);
        Storyboard.SetTargetProperty(dropAnimation, "TranslateY");

        var storyboard = new Storyboard();
        storyboard.Children.Add(dropAnimation);
        storyboard.Begin();

        // Remove glow
        CardBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    private static Windows.UI.Color ParseColor(string hex)
    {
        return FieldOpsPro.Presentation.Utils.ColorUtils.ParseColor(hex);
    }
}
