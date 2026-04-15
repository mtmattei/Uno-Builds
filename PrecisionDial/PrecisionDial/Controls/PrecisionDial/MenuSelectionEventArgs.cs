using System;

namespace PrecisionDial.Controls;

/// <summary>
/// Args for <see cref="PrecisionDial.SelectionConfirmed"/> — fired on pointer release in menu mode.
/// </summary>
public sealed class MenuSelectionEventArgs : EventArgs
{
    public int SelectedIndex { get; }
    public DialMenuItem? SelectedItem { get; }

    public MenuSelectionEventArgs(int selectedIndex, DialMenuItem? selectedItem)
    {
        SelectedIndex = selectedIndex;
        SelectedItem = selectedItem;
    }
}
