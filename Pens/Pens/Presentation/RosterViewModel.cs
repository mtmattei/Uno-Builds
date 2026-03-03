using Microsoft.Extensions.Logging;
using Pens.Models;
using Pens.Services;

namespace Pens.Presentation;

public partial class RosterViewModel : ObservableObject
{
    private readonly ISupabaseService _supabase;
    private readonly IPlayerIdentityService _identity;
    private readonly ILogger<RosterViewModel> _logger;
    private readonly List<PlayerViewModel> _allPlayers = [];

    // Week starts on Monday for attendance tracking
    private DateTime CurrentWeekOf => DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

    public RosterViewModel(ISupabaseService supabase, IPlayerIdentityService identity, ILogger<RosterViewModel> logger)
    {
        _supabase = supabase;
        _identity = identity;
        _logger = logger;
        _ = LoadPlayersAsync();
    }

    public ObservableCollection<PlayerViewModel> Players { get; } = [];

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private int _inCount;

    [ObservableProperty]
    private int _outCount;

    [ObservableProperty]
    private int _pendingCount;

    public int? CurrentPlayerId => _identity.CurrentPlayerId;

    private async Task LoadPlayersAsync()
    {
        if (Players.Count > 0) return; // Already loaded

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Batch load: fetch players and all attendance for this week in parallel
            var playersTask = _supabase.GetPlayersAsync();
            var attendanceTask = _supabase.GetAllAttendanceForWeekAsync(CurrentWeekOf);
            await Task.WhenAll(playersTask, attendanceTask);

            var dbPlayers = playersTask.Result;
            var allAttendance = attendanceTask.Result;

            // Create a lookup for quick access
            var attendanceLookup = allAttendance.ToDictionary(a => a.PlayerId, a => a.Status);

            foreach (var dbPlayer in dbPlayers)
            {
                // Look up attendance from the batch result
                var statusString = attendanceLookup.GetValueOrDefault(dbPlayer.Id);
                var status = statusString switch
                {
                    "in" => PlayerStatus.In,
                    "out" => PlayerStatus.Out,
                    _ => PlayerStatus.Pending
                };

                var isCurrentPlayer = dbPlayer.Id == _identity.CurrentPlayerId;
                var player = new PlayerViewModel(
                    dbPlayer.Id,
                    dbPlayer.Name,
                    dbPlayer.Number,
                    dbPlayer.Position,
                    status,
                    this,
                    dbPlayer.IsInjured,
                    isCurrentPlayer
                );
                _allPlayers.Add(player);
                Players.Add(player);
            }

            UpdateCounts();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading players");
            ErrorMessage = "Failed to load roster";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void UpdateCounts()
    {
        InCount = _allPlayers.Count(p => p.Status == PlayerStatus.In);
        OutCount = _allPlayers.Count(p => p.Status == PlayerStatus.Out);
        PendingCount = _allPlayers.Count(p => p.Status == PlayerStatus.Pending);
    }

    public async Task SaveAttendanceAsync(int playerId, PlayerStatus status)
    {
        try
        {
            ErrorMessage = null;
            var statusString = status switch
            {
                PlayerStatus.In => "in",
                PlayerStatus.Out => "out",
                _ => "pending"
            };
            await _supabase.UpsertAttendanceAsync(playerId, CurrentWeekOf, statusString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving attendance for player {PlayerId}", playerId);
            ErrorMessage = "Failed to save";
        }
    }
}
