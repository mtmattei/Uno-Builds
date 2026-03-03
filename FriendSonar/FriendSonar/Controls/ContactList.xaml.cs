using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using FriendSonar.Models;
using Windows.UI;

namespace FriendSonar.Controls;

public sealed partial class ContactList : UserControl
{
    private ObservableCollection<Friend> _allFriends = new();
    public ObservableCollection<Friend> Friends { get; } = new();

    private string _currentSortBy = "Distance";
    private double _currentRange = 3.0;

    public ContactList()
    {
        this.InitializeComponent();
    }

    public void SetFriends(ObservableCollection<Friend> friends, double rangeMiles)
    {
        _currentRange = rangeMiles;
        _allFriends.Clear();
        foreach (var friend in friends)
        {
            _allFriends.Add(friend);
        }

        ApplySortAndFilter();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
        {
            _currentSortBy = item.Tag?.ToString() ?? "Distance";
            ApplySortAndFilter();
        }
    }

    private void ApplySortAndFilter()
    {
        Friends.Clear();

        // Filter by range, then sort
        var filtered = _allFriends.Where(f => f.DistanceMilesValue <= _currentRange);

        var sorted = _currentSortBy switch
        {
            "Name" => filtered.OrderBy(f => f.Name),
            // Active first (most relevant), then Idle, then Away
            "Status" => filtered.OrderByDescending(f => f.Status == FriendStatus.Active)
                                .ThenByDescending(f => f.Status == FriendStatus.Idle)
                                .ThenBy(f => f.DistanceMilesValue),
            _ => filtered.OrderBy(f => f.DistanceMilesValue)
        };

        foreach (var friend in sorted)
        {
            Friends.Add(friend);
        }
    }
}
