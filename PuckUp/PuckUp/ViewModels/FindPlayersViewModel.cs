using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PuckUp.Models;

namespace PuckUp.ViewModels;

public partial class FindPlayersViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Player> _players = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public FindPlayersViewModel()
    {
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        // TODO: Replace with actual API data once backend is implemented
        Players = new ObservableCollection<Player>
        {
            new Player
            {
                Id = "1",
                Name = "Mike Johnson",
                Position = "Goalie",
                League = "Adult Rec A",
                Arena = "Maple Arena",
                Availability = "Available tonight",
                Rating = 4.8
            },
            new Player
            {
                Id = "2",
                Name = "Sarah Williams",
                Position = "Defense",
                League = "Adult Rec B",
                Arena = "Oak Community Center",
                Availability = "Available weekends",
                Rating = 4.5
            }
        };
    }

    [RelayCommand]
    private void InviteToGame(Player player)
    {
        // TODO: Implement game invitation functionality
        // This should send an invitation to the player and track the response
    }

    [RelayCommand]
    private void SendMessage(Player player)
    {
        // TODO: Implement messaging functionality
        // This should open a chat/messaging dialog with the selected player
    }
}
