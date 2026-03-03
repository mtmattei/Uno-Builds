using Microsoft.Extensions.Logging;
using Pens.Models;
using Pens.Services;

namespace Pens.Presentation;

public partial class ScheduleViewModel : ObservableObject
{
    private readonly ISupabaseService _supabase;
    private readonly ILogger<ScheduleViewModel> _logger;

    public ScheduleViewModel(ISupabaseService supabase, ILogger<ScheduleViewModel> logger)
    {
        _supabase = supabase;
        _logger = logger;
        _ = LoadGamesAsync();
    }

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private Game? _nextGame;

    [ObservableProperty]
    private ImmutableList<Game> _upcomingGames = [];

    [ObservableProperty]
    private ImmutableList<GameResult> _lastResults = [];

    [ObservableProperty]
    private string _lastGameDate = "";

    [ObservableProperty]
    private string _lastGameArena = "Dorval Arena";

    [ObservableProperty]
    private bool _isGameToday;

    [ObservableProperty]
    private string _nextGameBadgeText = "NEXT GAME";

    private async Task LoadGamesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var dbGames = await _supabase.GetUpcomingGamesAsync();

            var games = dbGames.Select(g => new Game(
                Opponent: g.Opponent,
                Date: g.GameDate.ToString("ddd, MMM d"),
                Time: FormatTime(g.GameTime),
                Rink: g.Rink,
                IsHome: g.IsHome
            )).ToList();

            if (games.Count > 0)
            {
                NextGame = games[0] with { IsNext = true };
                UpcomingGames = games.Skip(1).ToImmutableList();

                // Check if today is game day
                var firstGame = dbGames[0];
                IsGameToday = firstGame.GameDate.Date == DateTime.Today;
                NextGameBadgeText = IsGameToday ? "GAME NIGHT" : "NEXT GAME";
            }

            // Load past results (games with scores)
            await LoadPastResultsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading games");
            ErrorMessage = "Failed to load schedule";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadPastResultsAsync()
    {
        try
        {
            var pastGames = await _supabase.GetPastGamesAsync();

            if (pastGames.Count > 0)
            {
                var lastGame = pastGames[0];
                LastGameDate = lastGame.GameDate.ToString("MMM d");

                LastResults = pastGames.Select(g =>
                {
                    var homeTeam = g.IsHome ? "Penguins" : g.Opponent;
                    var awayTeam = g.IsHome ? g.Opponent : "Penguins";
                    var homeScore = g.IsHome ? (g.HomeScore ?? 0) : (g.AwayScore ?? 0);
                    var awayScore = g.IsHome ? (g.AwayScore ?? 0) : (g.HomeScore ?? 0);

                    return new GameResult(homeTeam, homeScore, awayTeam, awayScore, IsPenguinsGame: true);
                }).ToImmutableList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error loading past games");
        }
    }

    private static string FormatTime(string time)
    {
        if (TimeOnly.TryParse(time, out var t))
        {
            return t.ToString("h:mm tt");
        }
        return time;
    }
}
