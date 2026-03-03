using Microsoft.Extensions.Logging;
using Pens.Models;
using Pens.Services;

namespace Pens.Presentation;

public partial class DutiesViewModel : ObservableObject
{
    private readonly ISupabaseService _supabase;
    private readonly ILogger<DutiesViewModel> _logger;
    private readonly Random _random = new();
    private List<DbPlayer> _allPlayers = [];
    private DbGame? _nextGame;

    public DutiesViewModel(ISupabaseService supabase, ILogger<DutiesViewModel> logger)
    {
        _supabase = supabase;
        _logger = logger;
        _ = LoadDataAsync();
    }

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private string _gameInfo = "";

    [ObservableProperty]
    private ImmutableList<Duty> _duties = [];

    [ObservableProperty]
    private bool _canRandomize = true;

    [ObservableProperty]
    private List<DbPlayer> _availablePlayers = [];

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Load players and next game
            _allPlayers = await _supabase.GetPlayersAsync();
            AvailablePlayers = _allPlayers;
            var upcomingGames = await _supabase.GetUpcomingGamesAsync();

            if (upcomingGames.Count > 0)
            {
                _nextGame = upcomingGames[0];
                GameInfo = $"{_nextGame.GameDate:ddd, MMM d} vs {_nextGame.Opponent}";

                // Load existing duties for this game
                var existingDuties = await _supabase.GetDutiesForGameAsync(_nextGame.Id);

                // Always show all duty types - fill in unassigned ones
                Duties = MapDutiesToDisplay(existingDuties);
            }
            else
            {
                GameInfo = "No upcoming games";
                Duties = CreateEmptyDuties();
                CanRandomize = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading duties");
            ErrorMessage = "Failed to load duties";
            GameInfo = "Error loading data";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RandomizeDutiesAsync()
    {
        if (_nextGame == null || _allPlayers.Count < 4)
            return;

        try
        {
            CanRandomize = false;

            // Get existing duties to preserve beer and food assignments
            var existingDuties = await _supabase.GetDutiesForGameAsync(_nextGame.Id);
            var beerDuty = existingDuties.FirstOrDefault(d => d.DutyType.Equals("beer", StringComparison.OrdinalIgnoreCase));
            var foodDuty = existingDuties.FirstOrDefault(d => d.DutyType.Equals("food", StringComparison.OrdinalIgnoreCase));

            // Get players already assigned to beer/food so we don't double-assign them
            var excludedPlayerIds = new HashSet<int>();
            if (beerDuty?.PlayerId != null) excludedPlayerIds.Add(beerDuty.PlayerId.Value);
            if (foodDuty?.PlayerId != null) excludedPlayerIds.Add(foodDuty.PlayerId.Value);

            // Shuffle remaining players
            var availablePlayers = _allPlayers
                .Where(p => !excludedPlayerIds.Contains(p.Id))
                .OrderBy(_ => _random.Next())
                .ToList();

            var assignments = new Dictionary<string, int>();

            // Preserve beer and food assignments if they exist, otherwise assign new ones
            if (beerDuty?.PlayerId != null)
            {
                assignments["beer"] = beerDuty.PlayerId.Value;
            }
            else if (availablePlayers.Count > 0)
            {
                assignments["beer"] = availablePlayers[0].Id;
                availablePlayers.RemoveAt(0);
            }

            if (foodDuty?.PlayerId != null)
            {
                assignments["food"] = foodDuty.PlayerId.Value;
            }
            else if (availablePlayers.Count > 0)
            {
                assignments["food"] = availablePlayers[0].Id;
                availablePlayers.RemoveAt(0);
            }

            // Randomly assign ice and cooler from remaining available players
            if (availablePlayers.Count > 0)
                assignments["ice"] = availablePlayers[0].Id;
            if (availablePlayers.Count > 1)
                assignments["cooler"] = availablePlayers[1].Id;

            // Save to database
            await _supabase.AssignDutiesAsync(_nextGame.Id, assignments);

            // Reload duties
            var updatedDuties = await _supabase.GetDutiesForGameAsync(_nextGame.Id);
            Duties = MapDutiesToDisplay(updatedDuties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error randomizing duties");
            ErrorMessage = "Failed to assign duties";
        }
        finally
        {
            CanRandomize = true;
        }
    }

    private ImmutableList<Duty> MapDutiesToDisplay(List<DbDuty> dbDuties)
    {
        // Define all duty types
        var allDutyTypes = new[] { "ice", "beer", "cooler", "food" };
        
        var duties = new List<Duty>();
        
        foreach (var dutyTypeStr in allDutyTypes)
        {
            var dbDuty = dbDuties.FirstOrDefault(d => d.DutyType.Equals(dutyTypeStr, StringComparison.OrdinalIgnoreCase));
            var player = dbDuty?.PlayerId != null ? _allPlayers.FirstOrDefault(p => p.Id == dbDuty.PlayerId) : null;
            
            var dutyType = dutyTypeStr switch
            {
                "ice" => DutyType.Ice,
                "beer" => DutyType.Beer,
                "cooler" => DutyType.Cooler,
                "food" => DutyType.Food,
                _ => DutyType.Ice
            };
            
            var roleName = dutyType switch
            {
                DutyType.Ice => "Ice Fee",
                DutyType.Beer => "Beer Run",
                DutyType.Cooler => "Cooler",
                DutyType.Food => "Food & Snacks",
                _ => "Unknown"
            };
            
            bool isManuallyAssigned = (dutyType == DutyType.Beer || dutyType == DutyType.Food);
            duties.Add(new Duty(dutyType, roleName, player?.Name ?? "Unassigned", player?.Id, isManuallyAssigned));
        }
        
        return duties.OrderBy(d => d.Type).ToImmutableList();
    }

    private ImmutableList<Duty> CreateEmptyDuties()
    {
        return
        [
            new Duty(DutyType.Ice, "Ice Fee", "Unassigned", null, false),
            new Duty(DutyType.Beer, "Beer Run", "Unassigned", null, true),
            new Duty(DutyType.Cooler, "Cooler", "Unassigned", null, false),
            new Duty(DutyType.Food, "Food & Snacks", "Unassigned", null, true)
        ];
    }

    [RelayCommand]
    private async Task AssignPlayerToDutyAsync(object parameter)
    {
        if (parameter is not object[] args || args.Length != 2 || _nextGame == null)
            return;

        var dutyType = args[0] as string;
        var playerId = args[1] as int?;

        if (string.IsNullOrEmpty(dutyType) || playerId == null)
            return;

        try
        {
            var assignments = new Dictionary<string, int> { [dutyType] = playerId.Value };
            await _supabase.AssignDutiesAsync(_nextGame.Id, assignments);

            // Reload duties
            var updatedDuties = await _supabase.GetDutiesForGameAsync(_nextGame.Id);
            Duties = MapDutiesToDisplay(updatedDuties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning player to duty");
            ErrorMessage = "Failed to assign player";
        }
    }
}
