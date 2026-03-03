using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uno.Extensions.Navigation;

namespace PuckUp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    
    [ObservableProperty]
    private string _userName = "John Doe";

    [ObservableProperty]
    private int _gamesPlayed = 27;

    [ObservableProperty]
    private string _favoritePosition = "Goalie";

    [ObservableProperty]
    private string _averageRating = "4.8";

    [ObservableProperty]
    private string _aboutMe = "Hockey player for 15 years. Looking for casual games on weeknights. Can play goalie or defense, but prefer to be in net.";

    [ObservableProperty]
    private string _preferredLeagues = "Adult Rec A, Dorval Youngtimers";

    [ObservableProperty]
    private string _preferredArenas = "Westwood Arena, Maple Arena";

    [ObservableProperty]
    private string _preferredPositions = "Goalie, Defense";

    [ObservableProperty]
    private string _preferredSchedule = "Weeknights after 7PM";
    
    [ObservableProperty]
    private int _selectedBottomTabIndex = 3; // "Profile" tab

    public ProfileViewModel(INavigator navigator = null)
    {
        _navigator = navigator;
        // TODO: Load user profile data from API once backend is implemented
    }

    [RelayCommand]
    private void EditProfile()
    {
        // TODO: Implement profile editing functionality
    }

    [RelayCommand]
    private void GoToSettings()
    {
        // TODO: Implement navigation to settings page
    }

    [RelayCommand]
    private void Logout()
    {
        // TODO: Implement logout functionality
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
            case 2: // Teams
                _navigator?.NavigateViewModelAsync<TeamsViewModel>(this);
                break;
            case 3: // Profile (current page)
                // Already on the Profile page, no navigation needed
                break;
        }
    }
}
