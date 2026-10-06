using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FriendSonar.Models;
using FriendSonar.Services;

namespace FriendSonar;

public sealed partial class MainPage : Page
{
    private const bool DemoMode = true;
    private const string ShareCodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly LocationService _locationService;
    private readonly SupabaseService _supabaseService;

    private DispatcherTimer? _statusTimer;
    private DispatcherTimer? _demoTimer;
    private DateTime _lastScanTime;
    private int _currentRange = 3;
    private bool _isShowingError;
    private bool _servicesWired;

    public ObservableCollection<Friend> Friends { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();

        _locationService = new LocationService();
        _supabaseService = new SupabaseService();

        this.Loaded += MainPage_Loaded;
        this.Unloaded += MainPage_Unloaded;
    }

    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        _lastScanTime = DateTime.Now;
        RadarDisplay.BlipTapped += RadarDisplay_BlipTapped;

        await InitializeServicesAsync();
        StartStatusTimer();
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        StopStatusTimer();
        StopDemoTimer();

        RadarDisplay.BlipTapped -= RadarDisplay_BlipTapped;

        if (_servicesWired)
        {
            _locationService.LocationUpdated -= LocationService_LocationUpdated;
            _locationService.Error -= Service_Error;
            _supabaseService.FriendLocationUpdated -= SupabaseService_FriendLocationUpdated;
            _servicesWired = false;
        }

        _locationService.StopTracking();
        _locationService.Dispose();
        _supabaseService.Dispose();
    }

    private async Task InitializeServicesAsync()
    {
#pragma warning disable CS0162 // Unreachable code is by design when DemoMode = true
        if (DemoMode)
        {
            await LoadDemoDataAsync();
            return;
        }

        _locationService.LocationUpdated += LocationService_LocationUpdated;
        _locationService.Error += Service_Error;
        _supabaseService.FriendLocationUpdated += SupabaseService_FriendLocationUpdated;
        _servicesWired = true;

        var hasPermission = await _locationService.RequestPermissionAsync();
        if (!hasPermission)
        {
            await ShowErrorAsync("Location permission is required to use FriendSonar.");
            return;
        }

        var supabaseReady = await _supabaseService.InitializeAsync();
        if (!supabaseReady)
        {
            await ShowErrorAsync("Could not connect to the server. Check your internet connection and restart the app.");
            return;
        }

        if (_supabaseService.CurrentUserId == null)
        {
            var userName = await PromptForNameAsync();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                await _supabaseService.CreateOrGetUserAsync(userName, "\U0001F464");
            }
        }

        ShareCodeText.Text = _supabaseService.ShareCode ?? GenerateLocalCode();

        _locationService.StartTracking(30);

        await RefreshFriendsAsync();

        _ = _supabaseService.ConnectRealtimeAsync().ContinueWith(_ =>
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await _supabaseService.SubscribeToFriendLocationsAsync();
            });
        });
#pragma warning restore CS0162
    }

    private async Task LoadDemoDataAsync()
    {
        ShareCodeText.Text = "X7K9M2";

        // Guard against re-entry on theme switch (Loaded fires again)
        Friends.Clear();
        StopDemoTimer();

        var demoFriends = new[]
        {
            new Friend { Id = Guid.NewGuid(), Name = "Alex Chen",      Emoji = "\U0001F3C4", DistanceMilesValue = 0.4, Angle = 45,  LastUpdated = DateTime.UtcNow.AddSeconds(-30) },
            new Friend { Id = Guid.NewGuid(), Name = "Maya Johnson",   Emoji = "\U0001F3A8", DistanceMilesValue = 1.2, Angle = 120, LastUpdated = DateTime.UtcNow.AddSeconds(-15) },
            new Friend { Id = Guid.NewGuid(), Name = "Jordan Lee",     Emoji = "\U0001F3B5", DistanceMilesValue = 0.8, Angle = 210, LastUpdated = DateTime.UtcNow.AddSeconds(-45) },
            new Friend { Id = Guid.NewGuid(), Name = "Sam Rivera",     Emoji = "☕",         DistanceMilesValue = 2.1, Angle = 330, LastUpdated = DateTime.UtcNow.AddMinutes(-1) },
            new Friend { Id = Guid.NewGuid(), Name = "Taylor Kim",     Emoji = "\U0001F4BB", DistanceMilesValue = 1.7, Angle = 75,  LastUpdated = DateTime.UtcNow.AddSeconds(-20) },
            new Friend { Id = Guid.NewGuid(), Name = "Casey Brooks",   Emoji = "\U0001F6B2", DistanceMilesValue = 2.8, Angle = 165, LastUpdated = DateTime.UtcNow.AddMinutes(-3) },
            new Friend { Id = Guid.NewGuid(), Name = "Riley Patel",    Emoji = "\U0001F30E", DistanceMilesValue = 0.3, Angle = 280, LastUpdated = DateTime.UtcNow.AddSeconds(-10) },
        };

        foreach (var friend in demoFriends)
        {
            Friends.Add(friend);
        }

        // Let RadarDisplay finish loading before pushing blips into it
        await Task.Delay(200);

        RefreshRadarDisplay();
        ContactCount.Text = demoFriends.Length.ToString();

        _demoTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        _demoTimer.Tick += DemoTimer_Tick;
        _demoTimer.Start();
    }

    private void DemoTimer_Tick(object? sender, object e)
    {
        var rng = new Random();
        foreach (var friend in Friends)
        {
            friend.DistanceMilesValue = Math.Max(0.1, friend.DistanceMilesValue + (rng.NextDouble() - 0.5) * 0.15);
            friend.Angle = (friend.Angle + rng.Next(-5, 6) + 360) % 360;
            friend.LastUpdated = DateTime.UtcNow.AddSeconds(-rng.Next(0, 60));
        }
        RefreshRadarDisplay();
    }

    private void StopDemoTimer()
    {
        if (_demoTimer != null)
        {
            _demoTimer.Stop();
            _demoTimer.Tick -= DemoTimer_Tick;
            _demoTimer = null;
        }
    }

    private async Task<string> PromptForNameAsync()
    {
        var inputBox = new TextBox
        {
            PlaceholderText = "Enter your name",
            MaxLength = 30
        };

        var dialog = new ContentDialog
        {
            Title = "Welcome to Friend Sonar",
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "What should your friends call you?",
                        TextWrapping = TextWrapping.Wrap,
                        Opacity = 0.8
                    },
                    inputBox
                }
            },
            PrimaryButtonText = "Continue",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(inputBox.Text))
        {
            return inputBox.Text.Trim();
        }

        return "Anonymous";
    }

    private async void LocationService_LocationUpdated(object? sender, LocationUpdatedEventArgs e)
    {
        await _supabaseService.UpdateLocationAsync(e.Latitude, e.Longitude);
        UpdateFriendPositions(e.Latitude, e.Longitude);
        _lastScanTime = DateTime.Now;
    }

    private void SupabaseService_FriendLocationUpdated(object? sender, FriendLocationUpdate update)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var existingFriend = Friends.FirstOrDefault(f => f.Id == update.FriendId);

            if (existingFriend != null)
            {
                existingFriend.Latitude = update.Latitude;
                existingFriend.Longitude = update.Longitude;
                existingFriend.LastUpdated = update.UpdatedAt;

                if (_locationService.CurrentLatitude.HasValue && _locationService.CurrentLongitude.HasValue)
                {
                    existingFriend.UpdateFromUserLocation(
                        _locationService.CurrentLatitude.Value,
                        _locationService.CurrentLongitude.Value);
                }
            }
            else
            {
                var friend = new Friend
                {
                    Id = update.FriendId,
                    Name = update.DisplayName,
                    Emoji = update.Emoji,
                    Latitude = update.Latitude,
                    Longitude = update.Longitude,
                    LastUpdated = update.UpdatedAt
                };

                if (_locationService.CurrentLatitude.HasValue && _locationService.CurrentLongitude.HasValue)
                {
                    friend.UpdateFromUserLocation(
                        _locationService.CurrentLatitude.Value,
                        _locationService.CurrentLongitude.Value);
                }

                Friends.Add(friend);
            }

            RefreshRadarDisplay();
        });
    }

    private void Service_Error(object? sender, string error)
    {
        System.Diagnostics.Debug.WriteLine($"[FriendSonar] Service error: {error}");
        DispatcherQueue.TryEnqueue(async () =>
        {
            // Prevent stacking multiple error dialogs
            if (_isShowingError) return;
            _isShowingError = true;
            try
            {
                await ShowErrorAsync(error);
            }
            finally
            {
                _isShowingError = false;
            }
        });
    }

    private async Task ShowErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task ShowSuccessAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Success",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task RefreshFriendsAsync()
    {
        var friendLocations = await _supabaseService.GetFriendLocationsAsync();

        Friends.Clear();
        foreach (var fl in friendLocations)
        {
            var friend = new Friend
            {
                Id = fl.Id,
                Name = fl.DisplayName,
                Emoji = fl.Emoji,
                Latitude = fl.Latitude,
                Longitude = fl.Longitude,
                LastUpdated = fl.UpdatedAt
            };

            if (_locationService.CurrentLatitude.HasValue && _locationService.CurrentLongitude.HasValue)
            {
                friend.UpdateFromUserLocation(
                    _locationService.CurrentLatitude.Value,
                    _locationService.CurrentLongitude.Value);
            }

            Friends.Add(friend);
        }

        RefreshRadarDisplay();
    }

    private void UpdateFriendPositions(double userLat, double userLon)
    {
        foreach (var friend in Friends)
        {
            friend.UpdateFromUserLocation(userLat, userLon);
        }
        RefreshRadarDisplay();
    }

    private void RefreshRadarDisplay()
    {
        RadarDisplay.ClearFriends();

        var visibleFriends = Friends.Where(f => f.IsVisible).ToList();

        foreach (var friend in visibleFriends)
        {
            RadarDisplay.AddFriend(
                friend.Id.GetHashCode(),
                friend.Name,
                friend.DistanceMilesValue,
                friend.Angle,
                friend.Status);
        }

        ContactListControl.SetFriends(visibleFriends, _currentRange);

        var inRangeCount = visibleFriends.Count(f => f.DistanceMilesValue <= _currentRange);
        ContactCount.Text = inRangeCount.ToString();
    }

    private void RangeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton clickedButton) return;

        Range1Button.IsChecked = Range1Button == clickedButton;
        Range3Button.IsChecked = Range3Button == clickedButton;
        Range5Button.IsChecked = Range5Button == clickedButton;
        Range10Button.IsChecked = Range10Button == clickedButton;

        if (clickedButton.Tag is string tagStr && int.TryParse(tagStr, out var range))
        {
            _currentRange = range;
            RadarDisplay.SetRange(range);
            RangeText.Text = $"RANGE: {_currentRange} MI";
            RefreshRadarDisplay();
        }
    }

    private void RadarDisplay_BlipTapped(object? sender, Friend friend)
    {
        FriendDetailPanel.ShowFriend(friend);
    }

    private void StartStatusTimer()
    {
        _statusTimer = new DispatcherTimer
        {
            // 250ms = 4Hz; visually identical to 100ms for a slowly-rotating sweep readout
            Interval = TimeSpan.FromMilliseconds(250)
        };
        _statusTimer.Tick += StatusTimer_Tick;
        _statusTimer.Start();
    }

    private void StopStatusTimer()
    {
        if (_statusTimer != null)
        {
            _statusTimer.Stop();
            _statusTimer.Tick -= StatusTimer_Tick;
            _statusTimer = null;
        }
    }

    private void StatusTimer_Tick(object? sender, object e)
    {
        PingAngle.Text = $"PING: {RadarDisplay.CurrentSweepAngle}°";
    }

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        var deferral = args.GetDeferral();

        try
        {
            await RadarDisplay.TriggerFullScanAsync();
            await RefreshFriendsAsync();
            _lastScanTime = DateTime.Now;
        }
        finally
        {
            deferral.Complete();
        }
    }

    private async void PingAllButton_Click(object sender, RoutedEventArgs e)
    {
        await RadarDisplay.TriggerFullScanAsync();
        _lastScanTime = DateTime.Now;
    }

    private async void AddFriendButton_Click(object sender, RoutedEventArgs e)
    {
        var inputBox = new TextBox
        {
            PlaceholderText = "Enter friend's code",
            MaxLength = 6,
            CharacterCasing = CharacterCasing.Upper
        };

        var dialog = new ContentDialog
        {
            Title = "Add Friend",
            Content = inputBox,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(inputBox.Text))
        {
            var success = await _supabaseService.AddFriendByCodeAsync(inputBox.Text);
            if (success)
            {
                await RefreshFriendsAsync();
                await ShowSuccessAsync("Friend added successfully!");
            }
        }
    }

    private static string GenerateLocalCode()
    {
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => ShareCodeChars[random.Next(ShareCodeChars.Length)]).ToArray());
    }
}
