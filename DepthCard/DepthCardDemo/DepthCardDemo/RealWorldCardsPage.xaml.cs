using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using DepthCardDemo.Controls;
using System;

namespace DepthCardDemo;

public sealed partial class RealWorldCardsPage : Page
{
    private DepthCard? _depthCard;

    public RealWorldCardsPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Find the DepthCard in the visual tree
        _depthCard = FindDepthCard(this);
        if (_depthCard != null)
        {
            _depthCard.TiltChanged += OnCardTiltChanged;
        }
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        if (_depthCard != null)
        {
            _depthCard.TiltChanged -= OnCardTiltChanged;
        }
    }

    private void OnCardTiltChanged(object? sender, DepthCardTiltChangedEventArgs e)
    {
        // Update corner highlights based on tilt direction
        // Elevated corners (tilted toward viewer) get brighter

        // Base opacity
        const double baseOpacity = 0.6;
        const double highlightMultiplier = 0.4; // How much to brighten

        // Calculate opacity for each corner based on tilt
        // Top corners brighten when tilted up (negative RotateX)
        // Bottom corners brighten when tilted down (positive RotateX)
        // Left corners brighten when tilted left (negative RotateY)
        // Right corners brighten when tilted right (positive RotateY)

        double verticalTilt = -e.RotateX / 15.0; // Normalize to roughly -1 to 1
        double horizontalTilt = e.RotateY / 15.0;

        verticalTilt = Math.Clamp(verticalTilt, -1, 1);
        horizontalTilt = Math.Clamp(horizontalTilt, -1, 1);

        // Top-left: brighten when tilted up and left
        double topLeftBoost = Math.Max(0, verticalTilt) * Math.Max(0, -horizontalTilt);
        TopLeftCorner.Opacity = baseOpacity + (topLeftBoost * highlightMultiplier);

        // Top-right: brighten when tilted up and right
        double topRightBoost = Math.Max(0, verticalTilt) * Math.Max(0, horizontalTilt);
        TopRightCorner.Opacity = baseOpacity + (topRightBoost * highlightMultiplier);

        // Bottom-left: brighten when tilted down and left
        double bottomLeftBoost = Math.Max(0, -verticalTilt) * Math.Max(0, -horizontalTilt);
        BottomLeftCorner.Opacity = baseOpacity + (bottomLeftBoost * highlightMultiplier);

        // Bottom-right: brighten when tilted down and right
        double bottomRightBoost = Math.Max(0, -verticalTilt) * Math.Max(0, horizontalTilt);
        BottomRightCorner.Opacity = baseOpacity + (bottomRightBoost * highlightMultiplier);
    }

    private DepthCard? FindDepthCard(DependencyObject element)
    {
        if (element is DepthCard card)
            return card;

        int childCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            var result = FindDepthCard(child);
            if (result != null)
                return result;
        }

        return null;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
