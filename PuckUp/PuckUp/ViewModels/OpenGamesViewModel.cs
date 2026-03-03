using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PuckUp.Models;

namespace PuckUp.ViewModels;

public partial class OpenGamesViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Game> _games = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public OpenGamesViewModel()
    {
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        // TODO: Replace with actual API data once backend is implemented
        Games = new ObservableCollection<Game>
        {
            new Game
            {
                Id = "1",
                League = "Dorval Youngtimers",
                Arena = "Westwood Arena Sports Center",
                GameTime = "Today, 7:30 PM",
                PositionsNeeded = new Dictionary<string, int>
                {
                    { "Goalie", 1 },
                    { "Defense", 2 }
                }
            },
            new Game
            {
                Id = "2",
                League = "DWHL",
                Arena = "Lakeside Arena",
                GameTime = "Tomorrow, 9:00 PM",
                PositionsNeeded = new Dictionary<string, int>
                {
                    { "Forward", 1 }
                }
            }
        };
    }

    [RelayCommand]
    private void SignUp(Game game)
    {
        // TODO: Implement game sign-up functionality
        // This should send the registration request to the backend and handle the response
    }
}
