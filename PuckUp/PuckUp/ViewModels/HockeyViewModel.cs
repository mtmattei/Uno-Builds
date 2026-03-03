using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using PuckUp.Presentation;
using Uno.Extensions.Navigation;
using Windows.UI;

namespace PuckUp.ViewModels;

public partial class HockeyViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    
    // Private service to get theme colors
    private readonly SolidColorBrush _primaryColorBrush;
    private readonly SolidColorBrush _surfaceVariantColorBrush;
    private readonly SolidColorBrush _transparentBrush;

    [ObservableProperty]
    private int _selectedTabIndex;
    
    [ObservableProperty]
    private int _selectedBottomTabIndex;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private object _currentTabView;

    [ObservableProperty]
    private SolidColorBrush _findPlayersTabColor;

    [ObservableProperty]
    private SolidColorBrush _openGamesTabColor;

    [ObservableProperty]
    private SolidColorBrush _findPlayersUnderlineColor;

    [ObservableProperty]
    private SolidColorBrush _openGamesUnderlineColor;

    [ObservableProperty] 
    private Visibility _findPlayersUnderlineVisibility = Visibility.Visible;

    [ObservableProperty] 
    private Visibility _openGamesUnderlineVisibility = Visibility.Collapsed;

    public FindPlayersViewModel FindPlayersViewModel { get; } = new();
    
    public OpenGamesViewModel OpenGamesViewModel { get; } = new();

    [ObservableProperty]
    private ObservableCollection<string> _arenas = new(new[] { "All Arenas", "Maple Arena", "Northside Ice Center", "Oak Community Center", "Lakeside Arena" });
    
    [ObservableProperty]
    private ObservableCollection<string> _leagues = new(new[] { "All Leagues", "Adult Rec A", "Adult Rec B", "Competitive A" });
    
    [ObservableProperty]
    private ObservableCollection<string> _positions = new(new[] { "All Positions", "Goalie", "Defense", "Forward" });
    
    [ObservableProperty]
    private ObservableCollection<string> _timeFrames = new(new[] { "Any Time", "Today", "Tomorrow", "This Weekend", "Next Week" });
    
    [ObservableProperty]
    private string _selectedArena = "All Arenas";
    
    [ObservableProperty]
    private string _selectedLeague = "All Leagues";
    
    [ObservableProperty]
    private string _selectedPosition = "All Positions";
    
    [ObservableProperty]
    private string _selectedTimeFrame = "Any Time";

    public HockeyViewModel(INavigator navigator = null)
    {
        _navigator = navigator;
        
        // Create brushes from theme resources
        var primaryColor = (Color)Application.Current.Resources["PrimaryColor"];
        var surfaceVariantColor = (Color)Application.Current.Resources["OnSurfaceVariantColor"];
        
        _primaryColorBrush = new SolidColorBrush(primaryColor);
        _surfaceVariantColorBrush = new SolidColorBrush(surfaceVariantColor);
        _transparentBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)); // Transparent color

        // Initialize with default colors
        FindPlayersTabColor = _primaryColorBrush;
        OpenGamesTabColor = _surfaceVariantColorBrush;
        FindPlayersUnderlineColor = _primaryColorBrush;
        OpenGamesUnderlineColor = _transparentBrush;

        // Initialize with FindPlayersView and SelectedTabIndex = 0
        SelectedTabIndex = 0;
        SelectedBottomTabIndex = 0; // Default to "Find" tab
        CurrentTabView = new FindPlayersView { DataContext = FindPlayersViewModel };
    }

    [RelayCommand]
    private void ShowArenaFilter()
    {
        // TODO: This is currently handled by XAML flyouts
        // Consider implementing additional filter logic here if needed
    }

    [RelayCommand]
    private void ShowLeagueFilter()
    {
        // TODO: This is currently handled by XAML flyouts
        // Consider implementing additional filter logic here if needed
    }

    [RelayCommand]
    private void ShowPositionFilter()
    {
        // TODO: This is currently handled by XAML flyouts
        // Consider implementing additional filter logic here if needed
    }

    [RelayCommand]
    private void ShowTimeFilter()
    {
        // TODO: This is currently handled by XAML flyouts
        // Consider implementing additional filter logic here if needed
    }

    [RelayCommand]
    private void SwitchToFindPlayers()
    {
        SelectedTabIndex = 0;
    }

    [RelayCommand]
    private void SwitchToOpenGames()
    {
        SelectedTabIndex = 1;
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        switch (value)
        {
            case 0: // Find Players
                FindPlayersTabColor = _primaryColorBrush;
                OpenGamesTabColor = _surfaceVariantColorBrush;
                FindPlayersUnderlineVisibility = Visibility.Visible;
                OpenGamesUnderlineVisibility = Visibility.Collapsed;
                CurrentTabView = new FindPlayersView { DataContext = FindPlayersViewModel };
                break;
            case 1: // Open Games
                FindPlayersTabColor = _surfaceVariantColorBrush;
                OpenGamesTabColor = _primaryColorBrush;
                FindPlayersUnderlineVisibility = Visibility.Collapsed;
                OpenGamesUnderlineVisibility = Visibility.Visible;
                CurrentTabView = new OpenGamesView { DataContext = OpenGamesViewModel };
                break;
        }
    }
    
    partial void OnSelectedBottomTabIndexChanged(int value)
    {
        switch (value)
        {
            case 0: // Find (current page)
                // Already on the Hockey page, no navigation needed
                break;
            case 1: // Upcoming
                _navigator?.NavigateViewModelAsync<UpcomingViewModel>(this);
                break;
            case 2: // Teams
                _navigator?.NavigateViewModelAsync<TeamsViewModel>(this);
                break;
            case 3: // Profile
                _navigator?.NavigateViewModelAsync<ProfileViewModel>(this);
                break;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        // TODO: Implement search functionality
        // This should filter both players and games lists based on the search text
    }

    partial void OnSelectedArenaChanged(string value)
    {
        // TODO: Implement arena filtering
        // This should filter the players and games lists based on the selected arena
    }

    partial void OnSelectedLeagueChanged(string value)
    {
        // TODO: Implement league filtering
        // This should filter the players and games lists based on the selected league
    }

    partial void OnSelectedPositionChanged(string value)
    {
        // TODO: Implement position filtering
        // This should filter the players and games lists based on the selected position
    }

    partial void OnSelectedTimeFrameChanged(string value)
    {
        // TODO: Implement time frame filtering
        // This should filter the players and games lists based on the selected time frame
    }
}
