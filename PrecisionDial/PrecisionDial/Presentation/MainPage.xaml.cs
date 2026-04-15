using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace PrecisionDial.Presentation;

public sealed partial class MainPage : Page
{
    private static readonly SolidColorBrush AmberBrush =
        new(Color.FromArgb(255, 212, 169, 89));
    private static readonly SolidColorBrush DimBrush =
        new(Color.FromArgb(64, 255, 255, 255));

    public MainPage()
    {
        this.InitializeComponent();

        // On Android, replace the desktop tab UI with the bottom-sheet menu shell.
        if (OperatingSystem.IsAndroid())
        {
            // Hide the tab bar and navigate the content frame to the Android shell.
            // The tab bar is the first child of the root Grid (Border with the tabs).
            if (this.Content is Microsoft.UI.Xaml.Controls.Grid root && root.Children.Count > 0)
            {
                root.Children[0].Visibility = Visibility.Collapsed;
            }
            ContentFrame.Navigate(typeof(PrecisionDial.Samples.AndroidShellPage));
            return;
        }

        SelectTab(0);
    }

    private void OnTabMixerTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) => SelectTab(0);
    private void OnTabMenuTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) => SelectTab(1);
    private void OnTabStudioTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) => SelectTab(2);

    private int _currentTab = -1;

    private void SelectTab(int index)
    {
        TabMixerText.Foreground = index == 0 ? AmberBrush : DimBrush;
        TabMenuText.Foreground = index == 1 ? AmberBrush : DimBrush;
        TabStudioText.Foreground = index == 2 ? AmberBrush : DimBrush;

        if (index == _currentTab) return;
        _currentTab = index;

        _ = index switch
        {
            0 => ContentFrame.Navigate(typeof(PrecisionDial.Samples.SizeVariationsPage)),
            1 => ContentFrame.Navigate(typeof(PrecisionDial.Samples.MenuDialPage)),
            2 => ContentFrame.Navigate(typeof(PrecisionDial.Samples.StudioPage)),
            _ => ContentFrame.Navigate(typeof(PrecisionDial.Samples.SizeVariationsPage)),
        };
    }
}
