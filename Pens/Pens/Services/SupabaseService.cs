using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pens.Models;
using Supabase;

namespace Pens.Services;

public interface ISupabaseService
{
    Task<List<DbPlayer>> GetPlayersAsync();
    Task<List<DbAttendance>> GetAllAttendanceForWeekAsync(DateTime weekOf);
    Task<List<DbChatMessage>> GetChatMessagesAsync(int limit = 50);
    Task<DbChatMessage> SendChatMessageAsync(int? playerId, string playerName, string message);
    Task<DbBeerTracker?> GetBeerTrackerAsync(string season = "2024-2025");
    Task UpdateBeerCountAsync(int consumedCases, string season = "2024-2025");
    Task<DbAttendance?> GetAttendanceAsync(int playerId, DateTime weekOf);
    Task UpsertAttendanceAsync(int playerId, DateTime weekOf, string status);
    Task<List<DbGame>> GetUpcomingGamesAsync();
    Task<List<DbGame>> GetPastGamesAsync();
    Task<List<DbDuty>> GetDutiesForGameAsync(int gameId);
    Task AssignDutiesAsync(int gameId, Dictionary<string, int> dutyAssignments);
    Task SubscribeToChatMessagesAsync(Action<DbChatMessage> onNewMessage, CancellationToken cancellationToken = default);
    void UnsubscribeFromChat();
}

public class SupabaseService : ISupabaseService, IDisposable
{
    private readonly Supabase.Client _client;
    private readonly ILogger<SupabaseService> _logger;
    private Action<DbChatMessage>? _onNewMessage;
    private CancellationTokenSource? _pollingCts;

    public SupabaseService(IConfiguration configuration, ILogger<SupabaseService> logger)
    {
        _logger = logger;
        var url = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url not configured");
        var key = configuration["Supabase:AnonKey"] ?? throw new InvalidOperationException("Supabase:AnonKey not configured");

        _client = new Supabase.Client(url, key);
    }

    public async Task<List<DbPlayer>> GetPlayersAsync()
    {
        var response = await _client.From<DbPlayer>()
            .Order("number", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<List<DbChatMessage>> GetChatMessagesAsync(int limit = 50)
    {
        var response = await _client.From<DbChatMessage>()
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Limit(limit)
            .Get();
        return response.Models.OrderBy(m => m.CreatedAt).ToList();
    }

    public async Task<DbChatMessage> SendChatMessageAsync(int? playerId, string playerName, string message)
    {
        var chatMessage = new DbChatMessage
        {
            PlayerId = playerId,
            PlayerName = playerName,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        var response = await _client.From<DbChatMessage>().Insert(chatMessage);
        return response.Models.First();
    }

    public async Task<DbBeerTracker?> GetBeerTrackerAsync(string season = "2024-2025")
    {
        var response = await _client.From<DbBeerTracker>()
            .Where(b => b.Season == season)
            .Single();
        return response;
    }

    public async Task UpdateBeerCountAsync(int consumedCases, string season = "2024-2025")
    {
        await _client.From<DbBeerTracker>()
            .Where(b => b.Season == season)
            .Set(b => b.ConsumedCases, consumedCases)
            .Set(b => b.UpdatedAt, DateTime.UtcNow)
            .Update();
    }

    public async Task<DbAttendance?> GetAttendanceAsync(int playerId, DateTime weekOf)
    {
        try
        {
            var response = await _client.From<DbAttendance>()
                .Where(a => a.PlayerId == playerId)
                .Where(a => a.WeekOf == weekOf.Date)
                .Single();
            return response;
        }
        catch (Exception ex)
        {
            // Single() throws if no record found - this is expected behavior
            _logger.LogDebug(ex, "No attendance record found for player {PlayerId} on {WeekOf}", playerId, weekOf.Date);
            return null;
        }
    }

    public async Task<List<DbAttendance>> GetAllAttendanceForWeekAsync(DateTime weekOf)
    {
        var response = await _client.From<DbAttendance>()
            .Where(a => a.WeekOf == weekOf.Date)
            .Get();
        return response.Models;
    }

    public async Task UpsertAttendanceAsync(int playerId, DateTime weekOf, string status)
    {
        var attendance = new DbAttendance
        {
            PlayerId = playerId,
            WeekOf = weekOf.Date,
            Status = status,
            UpdatedAt = DateTime.UtcNow
        };

        await _client.From<DbAttendance>().Upsert(attendance);
    }

    public async Task<List<DbGame>> GetUpcomingGamesAsync()
    {
        // Include today's games - they should show until the next day
        var today = DateTime.Today;
        var response = await _client.From<DbGame>()
            .Filter("game_date", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, today.ToString("yyyy-MM-dd"))
            .Order("game_date", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Limit(10)
            .Get();
        return response.Models;
    }

    public async Task<List<DbGame>> GetPastGamesAsync()
    {
        var response = await _client.From<DbGame>()
            .Filter("game_date", Supabase.Postgrest.Constants.Operator.LessThan, DateTime.Today.ToString("yyyy-MM-dd"))
            .Not("home_score", Supabase.Postgrest.Constants.Operator.Is, "null")
            .Order("game_date", Supabase.Postgrest.Constants.Ordering.Descending)
            .Limit(4)
            .Get();
        return response.Models;
    }

    public async Task<List<DbDuty>> GetDutiesForGameAsync(int gameId)
    {
        try
        {
            var response = await _client.From<DbDuty>()
                .Filter("game_id", Supabase.Postgrest.Constants.Operator.Equals, gameId.ToString())
                .Get();
            return response.Models;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting duties for game {GameId}", gameId);
            return [];
        }
    }

    public async Task AssignDutiesAsync(int gameId, Dictionary<string, int> dutyAssignments)
    {
        try
        {
            // Delete existing duties for this game
            await _client.From<DbDuty>()
                .Filter("game_id", Supabase.Postgrest.Constants.Operator.Equals, gameId.ToString())
                .Delete();

            // Insert new duties
            var duties = dutyAssignments.Select(kv => new DbDuty
            {
                GameId = gameId,
                PlayerId = kv.Value,
                DutyType = kv.Key,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            foreach (var duty in duties)
            {
                await _client.From<DbDuty>().Insert(duty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning duties for game {GameId}", gameId);
            throw;
        }
    }

    public async Task SubscribeToChatMessagesAsync(Action<DbChatMessage> onNewMessage, CancellationToken cancellationToken = default)
    {
        UnsubscribeFromChat(); // Cancel any existing polling

        _pollingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _onNewMessage = onNewMessage;

        // Use polling for chat updates (Realtime requires Alpha access or Read Replica)
        var messages = await GetChatMessagesAsync(1);
        var lastMessageId = messages.LastOrDefault()?.Id ?? 0;
        var token = _pollingCts.Token;

        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested && _onNewMessage != null)
            {
                try
                {
                    await Task.Delay(3000, token);
                    if (token.IsCancellationRequested) break;

                    var response = await _client.From<DbChatMessage>()
                        .Filter("id", Supabase.Postgrest.Constants.Operator.GreaterThan, lastMessageId.ToString())
                        .Order("id", Supabase.Postgrest.Constants.Ordering.Ascending)
                        .Get();

                    foreach (var msg in response.Models)
                    {
                        if (token.IsCancellationRequested) break;
                        lastMessageId = msg.Id;
                        _onNewMessage?.Invoke(msg);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Chat polling error, retrying...");
                }
            }
        }, token);
    }

    public void UnsubscribeFromChat()
    {
        _onNewMessage = null;
        _pollingCts?.Cancel();
        _pollingCts?.Dispose();
        _pollingCts = null;
    }

    public void Dispose()
    {
        UnsubscribeFromChat();
    }
}
