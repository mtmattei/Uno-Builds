using Pens.Services;

namespace Pens.Presentation;

public partial class PlayerPickerViewModel : ObservableObject
{
    private readonly ISupabaseService _supabase;
    private readonly IPlayerIdentityService _identity;
    private readonly Action _onPlayerSelected;

    public PlayerPickerViewModel(ISupabaseService supabase, IPlayerIdentityService identity, Action onPlayerSelected)
    {
        _supabase = supabase;
        _identity = identity;
        _onPlayerSelected = onPlayerSelected;
        _ = LoadPlayersAsync();
    }

    public ObservableCollection<PlayerPickerItem> Players { get; } = [];

    [ObservableProperty]
    private bool _isLoading = true;

    private async Task LoadPlayersAsync()
    {
        try
        {
            IsLoading = true;
            var dbPlayers = await _supabase.GetPlayersAsync();

            foreach (var player in dbPlayers)
            {
                Players.Add(new PlayerPickerItem(
                    player.Id,
                    player.Name,
                    player.Number,
                    player.IsInjured ? "IR" : player.Position,
                    () => SelectPlayer(player.Id, player.Name)
                ));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading players: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SelectPlayer(int playerId, string playerName)
    {
        _identity.SetCurrentPlayer(playerId, playerName);
        _onPlayerSelected();
    }
}

public partial record PlayerPickerItem(int Id, string Name, int Number, string Position, Action SelectAction)
{
    public ICommand SelectCommand => new RelayCommand(SelectAction);
}
