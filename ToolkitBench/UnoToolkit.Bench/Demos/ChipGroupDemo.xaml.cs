using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UnoToolkit.Bench.Controls;

namespace UnoToolkit.Bench.Demos;

public sealed partial class ChipGroupDemo : UserControl
{
    private const int CascadeStaggerMs = 70;

    private static readonly string[] Definitions =
    {
        "All", "Field", "Studio", "Archive", "Press",
    };

    private readonly Dictionary<string, FlipChip> _chips = new();
    private readonly HashSet<string> _selected = new() { "All" };
    private bool _busy;

    public ChipGroupDemo()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var id in Definitions)
        {
            var chip = new FlipChip
            {
                Label = id,
                IsChecked = _selected.Contains(id),
            };
            chip.ChipTapped += (s, _) => OnChipTapped(id);
            _chips[id] = chip;
            ChipsHost.Items.Add(chip);
        }
        UpdateReadout();
    }

    public void TriggerChip(string id) => OnChipTapped(id);

    private async void OnChipTapped(string id)
    {
        if (_busy) return;

        if (id == "All")
        {
            // If everyone is checked, cascade-uncheck. Else cascade-check all.
            var allChecked = Definitions.All(_selected.Contains);
            await CascadeAll(check: !allChecked);
        }
        else
        {
            // Single flip — instant
            if (_selected.Contains(id))
                _selected.Remove(id);
            else
                _selected.Add(id);

            // "All" is mutually exclusive with individual selections
            _selected.Remove("All");

            _chips[id].IsChecked = _selected.Contains(id);
            _chips["All"].IsChecked = false;

            // If nothing left, cascade-recheck everyone
            if (_selected.Count == 0)
                await CascadeAll(check: true);
            else
                UpdateReadout();
        }
    }

    private async Task CascadeAll(bool check)
    {
        _busy = true;
        try
        {
            // Decel stagger — each successive gap grows so the wave settles.
            // 70 → 78 → 86 → 94 → 102 ms.
            for (int i = 0; i < Definitions.Length; i++)
            {
                if (i > 0) await Task.Delay(CascadeStaggerMs + (i - 1) * 8);
                var id = Definitions[i];
                _chips[id].IsChecked = check;
                if (check) _selected.Add(id);
                else       _selected.Remove(id);
            }
            await Task.Delay(380);
            UpdateReadout();
        }
        finally
        {
            _busy = false;
        }
    }

    private void UpdateReadout()
    {
        if (_selected.Count == Definitions.Length || (_selected.Contains("All") && _selected.Count == 1))
        {
            Readout.Text = "Selected: All";
            return;
        }
        var ordered = Definitions.Where(id => id != "All" && _selected.Contains(id));
        var joined = string.Join(", ", ordered);
        Readout.Text = "Selected: " + (joined.Length == 0 ? "—" : joined);
    }
}
