using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Uno.Extensions.Navigation;

namespace PuckUp.ViewModels;

public partial class UpcomingViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    
    [ObservableProperty]
    private ObservableCollection<UpcomingGameItem> _upcomingGames = new();

    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private int _selectedBottomTabIndex = 1; // "Upcoming" tab

    public Visibility NoGamesVisibility => UpcomingGames.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public UpcomingViewModel(INavigator navigator = null)
    {
        _navigator = navigator;
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        // TODO: Replace with actual API data once backend is implemented
        UpcomingGames = new ObservableCollection<UpcomingGameItem>
        {
            new UpcomingGameItem
            {
                Date = "Wednesday, April 9",
                Time = "7:30 PM",
                Arena = "Westwood Arena",
                League = "Dorval Youngtimers",
                Position = "Goalie"
            },
            new UpcomingGameItem
            {
                Date = "Saturday, April 12",
                Time = "10:00 AM",
                Arena = "Lakeside Arena",
                League = "DWHL",
                Position = "Defense"
            },
            new UpcomingGameItem
            {
                Date = "Monday, April 14",
                Time = "9:15 PM",
                Arena = "Oak Community Center",
                League = "Adult Rec B",
                Position = "Forward"
            }
        };
    }
    
    partial void OnSelectedBottomTabIndexChanged(int value)
    {
        switch (value)
        {
            case 0: // Find
                _navigator?.NavigateViewModelAsync<HockeyViewModel>(this);
                break;
            case 1: // Upcoming (current page)
                // Already on the Upcoming page, no navigation needed
                break;
            case 2: // Teams
                _navigator?.NavigateViewModelAsync<TeamsViewModel>(this);
                break;
            case 3: // Profile
                _navigator?.NavigateViewModelAsync<ProfileViewModel>(this);
                break;
        }
    }
}
