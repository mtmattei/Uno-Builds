using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Uno.Extensions.Navigation;

namespace PuckUp.ViewModels;

public partial class TeamsViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    
    [ObservableProperty]
    private ObservableCollection<TeamItem> _teams = new();

    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private int _selectedBottomTabIndex = 2; // "Teams" tab

    public Visibility NoTeamsVisibility => Teams.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public TeamsViewModel(INavigator navigator = null)
    {
        _navigator = navigator;
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        // TODO: Replace with actual API data once backend is implemented
        Teams = new ObservableCollection<TeamItem>
        {
            new TeamItem
            {
                Id = "1",
                Name = "Ice Breakers",
                League = "Dorval Youngtimers",
                Players = 16
            },
            new TeamItem
            {
                Id = "2",
                Name = "Puck Masters",
                League = "DWHL",
                Players = 14
            }
        };
    }

    [RelayCommand]
    private void ViewTeam(string teamId)
    {
        // TODO: Implement team details view functionality
    }

    [RelayCommand]
    private void ManageTeam(string teamId)
    {
        // TODO: Implement team management functionality
    }

    [RelayCommand]
    private void CreateTeam()
    {
        // TODO: Implement team creation functionality
    }
    
    partial void OnSelectedBottomTabIndexChanged(int value)
    {
        switch (value)
        {
            case 0: // Find
                _navigator?.NavigateViewModelAsync<HockeyViewModel>(this);
                break;
            case 1: // Upcoming
                _navigator?.NavigateViewModelAsync<UpcomingViewModel>(this);
                break;
            case 2: // Teams (current page)
                // Already on the Teams page, no navigation needed
                break;
            case 3: // Profile
                _navigator?.NavigateViewModelAsync<ProfileViewModel>(this);
                break;
        }
    }
}
