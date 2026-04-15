using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using PrecisionDial.Controls;

namespace PrecisionDial.Samples;

public sealed partial class StudioPage : Page
{
    public StudioPage()
    {
        InitializeComponent();

        // Five-mode menu dial driven by DialMode=Menu (cone-of-light + dashed segments per item).
        // Icons use the bundled Phosphor Icons TTF — consistent line-art aesthetic across
        // every target, tinted by the MenuIconRenderer paint color.
        ModeDial.MenuItems = new List<DialMenuItem>
        {
            new() { Label = "MUSIC",    Icon = "\uE33C", Tag = "music"    }, // ph-music-note
            new() { Label = "RADIO",    Icon = "\uE77E", Tag = "radio"    }, // ph-radio
            new() { Label = "PODCAST",  Icon = "\uE326", Tag = "podcast"  }, // ph-microphone
            new() { Label = "VIDEO",    Icon = "\uE4DA", Tag = "video"    }, // ph-video-camera
            new() { Label = "SETTINGS", Icon = "\uE270", Tag = "settings" }, // ph-gear
        };

        ModeDial.SelectionConfirmed += (_, args) =>
        {
            ModeReadout.Text = args.SelectedItem?.Label ?? string.Empty;
        };

        // Live-update the readout while dragging too.
        ModeDial.ValueChanged += (_, _) =>
        {
            var item = ModeDial.SelectedItem;
            if (item is not null)
            {
                ModeReadout.Text = item.Label ?? string.Empty;
            }
        };
    }
}
