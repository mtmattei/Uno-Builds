using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PrecisionDial.Controls;
using Windows.UI;

namespace PrecisionDial.Samples;

public sealed partial class MenuDialPage : Microsoft.UI.Xaml.Controls.Page
{
    private TextBlock[] _arcItems = null!;

    public MenuDialPage()
    {
        InitializeComponent();

        NavDial.MenuItems = new List<string>
        {
            "Home", "Settings", "Profile", "Search", "Messages", "Favorites",
        };
        NavDial.MenuIcons = new List<string>
        {
            "⌂", "⚙", "○", "⌕", "✉", "★",
        };
        NavDial.SelectionChanged += (_, idx) => NavReadout.Value = idx + 1;

        QualityDial.MenuItems = new List<string> { "Low", "Medium", "High", "Ultra" };
        QualityDial.MenuIcons = new List<string> { "▫", "▪", "◆", "★" };
        QualityDial.SelectionChanged += (_, idx) => QualityReadout.Value = idx + 1;

        _arcItems = new[] { ArcPlay, ArcPause, ArcSkip, ArcShuffle, ArcRepeat };
        ArcDial.ValueChanged += OnArcDialValueChanged;
        UpdateArcHighlight(2);
    }

    private void OnArcDialValueChanged(object? sender, DialValueChangedEventArgs e)
    {
        var normalized = (e.NewValue - ArcDial.Minimum) / (ArcDial.Maximum - ArcDial.Minimum);
        var activeIndex = Math.Clamp((int)Math.Round(normalized * 4), 0, 4);
        UpdateArcHighlight(activeIndex);
    }

    private void UpdateArcHighlight(int activeIndex)
    {
        // Reveal only the currently-selected icon; hide the others completely.
        for (int i = 0; i < _arcItems.Length; i++)
        {
            _arcItems[i].Opacity = i == activeIndex ? 1.0 : 0.0;
        }
    }
}
