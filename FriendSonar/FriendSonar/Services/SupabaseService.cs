using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Supabase.Realtime;
using FriendSonar.Models;
using Postgrest.Models;
using Postgrest.Attributes;
using Windows.Storage;

namespace FriendSonar.Services;

public class SupabaseService : IDisposable
{
    private const string SupabaseUrl = "https://idpnizzbvtpdxluziqlp.supabase.co";
    private const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImlkcG5penpidnRwZHhsdXppcWxwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjQzNjU1NDgsImV4cCI6MjA3OTk0MTU0OH0.LQ4VtRwhgDPWyk21ObIYEtvgwU5NynEA2VQHHggBte4";

    private const string UserIdKey = "FriendSonar_UserId";
    private const string ShareCodeKey = "FriendSonar_ShareCode";

    private Supabase.Client? _client;
    private Guid? _currentUserId;
    private string? _shareCode;

    public event EventHandler<FriendLocationUpdate>? FriendLocationUpdated;
    public event EventHandler<string>? Error;

    public Guid? CurrentUserId => _currentUserId;
    public string? ShareCode => _shareCode;
    public bool IsInitialized => _client != null;

    public SupabaseService()
    {
        // Load saved user info on construction
        LoadSavedUserInfo();
    }

    private void LoadSavedUserInfo()
    {
        try
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (localSettings.Values.TryGetValue(UserIdKey, out var userIdObj) && userIdObj is string userIdStr)
            {
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    _currentUserId = userId;
                }
            }

            if (localSettings.Values.TryGetValue(ShareCodeKey, out var shareCodeObj) && shareCodeObj is string shareCode)
            {
                _shareCode = shareCode;
            }
        }
        catch
        {
            // LocalSettings may not be available on all platforms
        }
    }

    private void SaveUserInfo()
    {
        try
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if (_currentUserId.HasValue)
            {
                localSettings.Values[UserIdKey] = _currentUserId.Value.ToString();
            }

            if (!string.IsNullOrEmpty(_shareCode))
            {
                localSettings.Values[ShareCodeKey] = _shareCode;
            }
        }
        catch
        {
            // LocalSettings may not be available on all platforms
        }
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };
            _client = new Supabase.Client(SupabaseUrl, SupabaseKey, options);
            await _client.InitializeAsync();
            return true;
        }
        catch (Exception ex)
        {
            _client = null;
            Error?.Invoke(this, $"Failed to initialize Supabase: {ex.Message}");
            return false;
        }
    }

    public async Task ConnectRealtimeAsync()
    {
        if (_client == null) return;

        try
        {
            await _client.Realtime.ConnectAsync();
        }
        catch (Exception ex)
        {
            // Realtime is optional - log but don't block the app
            System.Diagnostics.Debug.WriteLine($"Realtime connection failed (non-fatal): {ex.Message}");
        }
    }

    private string GenerateShareCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    public async Task<Guid?> CreateOrGetUserAsync(string displayName, string emoji = "👤")
    {
        if (_client == null) return null;

        try
        {
            // Generate a unique share code
            string code;
            bool codeExists;
            do
            {
                code = GenerateShareCode();
                var existing = await _client.From<UserRecord>()
                    .Filter("share_code", Postgrest.Constants.Operator.Equals, code)
                    .Get();
                codeExists = existing.Models.Count > 0;
            } while (codeExists);

            // Create a new user with share code
            var user = new UserRecord
            {
                DisplayName = displayName,
                Emoji = emoji,
                ShareCode = code
            };

            var response = await _client.From<UserRecord>()
                .Insert(user);

            if (response.Models.Count > 0)
            {
                _currentUserId = response.Models[0].Id;
                _shareCode = response.Models[0].ShareCode;
                SaveUserInfo();
                return _currentUserId;
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to create user: {ex.Message}");
        }

        return null;
    }

    public async Task<bool> AddFriendByCodeAsync(string shareCode)
    {
        if (_client == null || _currentUserId == null) return false;

        try
        {
            // Find user by share code
            var userResponse = await _client.From<UserRecord>()
                .Filter("share_code", Postgrest.Constants.Operator.Equals, shareCode.ToUpper().Trim())
                .Single();

            if (userResponse == null)
            {
                Error?.Invoke(this, "No user found with that code");
                return false;
            }

            if (userResponse.Id == _currentUserId)
            {
                Error?.Invoke(this, "You cannot add yourself as a friend");
                return false;
            }

            // Add bidirectional friendship
            return await AddFriendAsync(userResponse.Id);
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to add friend: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateLocationAsync(double latitude, double longitude)
    {
        if (_client == null || _currentUserId == null) return false;

        try
        {
            var location = new LocationRecord
            {
                UserId = _currentUserId.Value,
                Latitude = latitude,
                Longitude = longitude,
                UpdatedAt = DateTime.UtcNow
            };

            await _client.From<LocationRecord>()
                .Upsert(location);

            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to update location: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddFriendAsync(Guid friendId)
    {
        if (_client == null || _currentUserId == null) return false;

        try
        {
            // Add bidirectional friendship
            var friendship1 = new FriendshipRecord
            {
                UserId = _currentUserId.Value,
                FriendId = friendId
            };

            var friendship2 = new FriendshipRecord
            {
                UserId = friendId,
                FriendId = _currentUserId.Value
            };

            await _client.From<FriendshipRecord>().Insert(friendship1);
            await _client.From<FriendshipRecord>().Insert(friendship2);

            return true;
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to add friend: {ex.Message}");
            return false;
        }
    }

    public async Task<List<FriendWithLocation>> GetFriendLocationsAsync()
    {
        var friends = new List<FriendWithLocation>();
        if (_client == null || _currentUserId == null) return friends;

        try
        {
            // Get friend IDs
            var friendships = await _client.From<FriendshipRecord>()
                .Filter("user_id", Postgrest.Constants.Operator.Equals, _currentUserId.Value.ToString())
                .Get();

            foreach (var friendship in friendships.Models)
            {
                // Get friend user info
                var userResponse = await _client.From<UserRecord>()
                    .Filter("id", Postgrest.Constants.Operator.Equals, friendship.FriendId.ToString())
                    .Single();

                if (userResponse == null) continue;

                // Get friend location
                var locationResponse = await _client.From<LocationRecord>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, friendship.FriendId.ToString())
                    .Single();

                if (locationResponse == null) continue;

                // Check if location is stale (> 5 minutes)
                var timeSinceUpdate = DateTime.UtcNow - locationResponse.UpdatedAt;
                if (timeSinceUpdate.TotalMinutes > 5) continue; // Hide stale friends

                friends.Add(new FriendWithLocation
                {
                    Id = userResponse.Id,
                    DisplayName = userResponse.DisplayName,
                    Emoji = userResponse.Emoji,
                    Latitude = locationResponse.Latitude,
                    Longitude = locationResponse.Longitude,
                    UpdatedAt = locationResponse.UpdatedAt
                });
            }
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to get friend locations: {ex.Message}");
        }

        return friends;
    }

    public async Task SubscribeToFriendLocationsAsync()
    {
        if (_client == null) return;

        try
        {
            var channel = _client.Realtime.Channel("locations");

            channel.Register(new Supabase.Realtime.PostgresChanges.PostgresChangesOptions("public", "locations"));

            channel.AddPostgresChangeHandler(
                Supabase.Realtime.PostgresChanges.PostgresChangesOptions.ListenType.All,
                (sender, change) =>
                {
                    // Refresh friend locations when any location changes
                    _ = RefreshFriendLocationsAsync();
                });

            await channel.Subscribe();
        }
        catch (Exception ex)
        {
            Error?.Invoke(this, $"Failed to subscribe to updates: {ex.Message}");
        }
    }

    private async Task RefreshFriendLocationsAsync()
    {
        var friends = await GetFriendLocationsAsync();
        foreach (var friend in friends)
        {
            FriendLocationUpdated?.Invoke(this, new FriendLocationUpdate
            {
                FriendId = friend.Id,
                DisplayName = friend.DisplayName,
                Emoji = friend.Emoji,
                Latitude = friend.Latitude,
                Longitude = friend.Longitude,
                UpdatedAt = friend.UpdatedAt
            });
        }
    }

    public void Dispose()
    {
        // Supabase client cleanup handled internally
    }
}

// Database models
[Table("users")]
public class UserRecord : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [Column("emoji")]
    public string Emoji { get; set; } = "👤";

    [Column("share_code")]
    public string ShareCode { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

[Table("locations")]
public class LocationRecord : BaseModel
{
    [PrimaryKey("user_id")]
    public Guid UserId { get; set; }

    [Column("latitude")]
    public double Latitude { get; set; }

    [Column("longitude")]
    public double Longitude { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

[Table("friendships")]
public class FriendshipRecord : BaseModel
{
    [PrimaryKey("user_id")]
    public Guid UserId { get; set; }

    [Column("friend_id")]
    public Guid FriendId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class FriendWithLocation
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Emoji { get; set; } = "👤";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FriendLocationUpdate
{
    public Guid FriendId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Emoji { get; set; } = "👤";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime UpdatedAt { get; set; }
}
