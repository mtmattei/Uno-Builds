using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using FriendSonar.Models;
using FriendSonar.Services;

namespace FriendSonar;

public sealed partial class MainPage : Page
{
    private DispatcherTimer? _statusTimer;
    private TextBlock? _pingAngle;
    private TextBlock? _contactCount;
    private TextBlock? _rangeText;
    private Controls.RadarDisplay? _radarDisplay;
    private Controls.ContactList? _contactListControl;
    private Controls.FriendDetailPanel? _friendDetailPanel;
    private TextBlock? _shareCodeText;

    // Range toggle buttons
    private ToggleButton? _range1Button;
    private ToggleButton? _range3Button;
    private ToggleButton? _range5Button;
    private ToggleButton? _range10Button;

    // Services
    private readonly LocationService _locationService;
    private readonly SupabaseService _supabaseService;

    // Last scan tracking
    private DateTime _lastScanTime;
    private int _currentRange = 3;

    // Set to true to populate with fake data for recording
    private const bool DemoMode = true;

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
        // Get references to named elements
        _pingAngle = this.FindName("PingAngle") as TextBlock;
        _contactCount = this.FindName("ContactCount") as TextBlock;
        _rangeText = this.FindName("RangeText") as TextBlock;
        _radarDisplay = this.FindName("RadarDisplay") as Controls.RadarDisplay;
        _contactListControl = this.FindName("ContactListControl") as Controls.ContactList;
        _friendDetailPanel = this.FindName("FriendDetailPanel") as Controls.FriendDetailPanel;
        _shareCodeText = this.FindName("ShareCodeText") as TextBlock;

        // Wire up blip tap event
        if (_radarDisplay != null)
        {
            _radarDisplay.BlipTapped += RadarDisplay_BlipTapped;
        }

        // Get range button references
        _range1Button = this.FindName("Range1Button") as ToggleButton;
        _range3Button = this.FindName("Range3Button") as ToggleButton;
        _range5Button = this.FindName("Range5Button") as ToggleButton;
        _range10Button = this.FindName("Range10Button") as ToggleButton;

        // Initialize last scan time
        _lastScanTime = DateTime.Now;

        // Initialize services
        await InitializeServicesAsync();

        StartStatusTimer();
    }

    private async System.Threading.Tasks.Task InitializeServicesAsync()
    {
        if (DemoMode)
        {
            await LoadDemoDataAsync();
            return;
        }

        // Wire up events first (before any async calls that might fire errors)
        _locationService.LocationUpdated += LocationService_LocationUpdated;
        _locationService.Error += Service_Error;
        _supabaseService.FriendLocationUpdated += SupabaseService_FriendLocationUpdated;

        // Request location permission
        var hasPermission = await _locationService.RequestPermissionAsync();
        if (!hasPermission)
        {
            await ShowErrorAsync("Location permission is required to use FriendSonar.");
            return;
        }

        // Initialize Supabase (REST only, no realtime yet)
        var supabaseReady = await _supabaseService.InitializeAsync();

        if (!supabaseReady)
        {
            await ShowErrorAsync("Could not connect to the server. Check your internet connection and restart the app.");
            return;
        }

        // Check if user exists, if not prompt for name
        if (_supabaseService.CurrentUserId == null)
        {
            var userName = await PromptForNameAsync();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                await _supabaseService.CreateOrGetUserAsync(userName, "\U0001F464");
            }
        }

        // Display share code (or generate local one as fallback)
        if (_shareCodeText != null)
        {
            _shareCodeText.Text = _supabaseService.ShareCode ?? GenerateLocalCode();
        }

        // Start location tracking (30 second intervals)
        _locationService.StartTracking(30);

        // Initial load of friend locations
        await RefreshFriendsAsync();

        // Connect realtime in the background (non-blocking)
        _ = _supabaseService.ConnectRealtimeAsync().ContinueWith(async _ =>
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                await _supabaseService.SubscribeToFriendLocationsAsync();
            });
        });
    }

    private async System.Threading.Tasks.Task LoadDemoDataAsync()
    {
        // Show a share code
        if (_shareCodeText != null)
        {
            _shareCodeText.Text = "X7K9M2";
        }

        // Demo friends with varied distances, angles, and statuses
        var demoFriends = new[]
        {
            new Friend { Id = Guid.NewGuid(), Name = "Alex Chen",      Emoji = "\U0001F3C4", DistanceMilesValue = 0.4, Angle = 45,  LastUpdated = DateTime.UtcNow.AddSeconds(-30) },
            new Friend { Id = Guid.NewGuid(), Name = "Maya Johnson",   Emoji = "\U0001F3A8", DistanceMilesValue = 1.2, Angle = 120, LastUpdated = DateTime.UtcNow.AddSeconds(-15) },
            new Friend { Id = Guid.NewGuid(), Name = "Jordan Lee",     Emoji = "\U0001F3B5", DistanceMilesValue = 0.8, Angle = 210, LastUpdated = DateTime.UtcNow.AddSeconds(-45) },
            new Friend { Id = Guid.NewGuid(), Name = "Sam Rivera",     Emoji = "\u2615",     DistanceMilesValue = 2.1, Angle = 330, LastUpdated = DateTime.UtcNow.AddMinutes(-1) },
            new Friend { Id = Guid.NewGuid(), Name = "Taylor Kim",     Emoji = "\U0001F4BB", DistanceMilesValue = 1.7, Angle = 75,  LastUpdated = DateTime.UtcNow.AddSeconds(-20) },
            new Friend { Id = Guid.NewGuid(), Name = "Casey Brooks",   Emoji = "\U0001F6B2", DistanceMilesValue = 2.8, Angle = 165, LastUpdated = DateTime.UtcNow.AddMinutes(-3) },
            new Friend { Id = Guid.NewGuid(), Name = "Riley Patel",    Emoji = "\U0001F30E", DistanceMilesValue = 0.3, Angle = 280, LastUpdated = DateTime.UtcNow.AddSeconds(-10) },
        };

        foreach (var friend in demoFriends)
        {
            Friends.Add(friend);
        }

        // Small delay to let RadarDisplay finish loading
        await System.Threading.Tasks.Task.Delay(200);

        RefreshRadarDisplay();

        // Update contact count
        if (_contactCount != null)
        {
            _contactCount.Text = demoFriends.Length.ToString();
        }

        // Start a timer that slowly drifts friend positions for a lively radar
        var demoTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
        demoTimer.Tick += (s, e) =>
        {
            var rng = new Random();
            foreach (var friend in Friends)
            {
                // Small random drift in distance and angle
                friend.DistanceMilesValue = Math.Max(0.1, friend.DistanceMilesValue + (rng.NextDouble() - 0.5) * 0.15);
                friend.Angle = (friend.Angle + rng.Next(-5, 6) + 360) % 360;

                // Keep "last updated" fresh so they stay visible
                friend.LastUpdated = DateTime.UtcNow.AddSeconds(-rng.Next(0, 60));
            }
            RefreshRadarDisplay();
        };
        demoTimer.Start();
    }

    private async System.Threading.Tasks.Task<string> PromptForNameAsync()
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
        // Update our location in Supabase
        await _supabaseService.UpdateLocationAsync(e.Latitude, e.Longitude);

        // Recalculate all friend distances/bearings
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
                // Update existing friend
                existingFriend.Latitude = update.Latitude;
                existingFriend.Longitude = update.Longitude;
                existingFriend.LastUpdated = update.UpdatedAt;

                // Recalculate position if we have user location
                if (_locationService.CurrentLatitude.HasValue && _locationService.CurrentLongitude.HasValue)
                {
                    existingFriend.UpdateFromUserLocation(
                        _locationService.CurrentLatitude.Value,
                        _locationService.CurrentLongitude.Value);
                }
            }
            else
            {
                // Add new friend
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

    private bool _isShowingError;

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

    private async System.Threading.Tasks.Task ShowErrorAsync(string message)
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

    private async System.Threading.Tasks.Task RefreshFriendsAsync()
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
        if (_radarDisplay == null || _contactListControl == null) return;

        _radarDisplay.ClearFriends();

        // Only show visible friends (updated within 5 minutes)
        var visibleFriends = Friends.Where(f => f.IsVisible).ToList();

        foreach (var friend in visibleFriends)
        {
            _radarDisplay.AddFriend(
                friend.Id.GetHashCode(),
                friend.Name,
                friend.DistanceMilesValue,
                friend.Angle,
                friend.Status);
        }

        // Pass range to contact list so it filters consistently with the radar
        _contactListControl.SetFriends(new ObservableCollection<Friend>(visibleFriends), _currentRange);

        // Count only friends within range
        var inRangeCount = visibleFriends.Count(f => f.DistanceMilesValue <= _currentRange);
        if (_contactCount != null)
        {
            _contactCount.Text = inRangeCount.ToString();
        }
    }

    private void RangeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton clickedButton) return;

        // Uncheck all buttons except the clicked one
        if (_range1Button != null) _range1Button.IsChecked = _range1Button == clickedButton;
        if (_range3Button != null) _range3Button.IsChecked = _range3Button == clickedButton;
        if (_range5Button != null) _range5Button.IsChecked = _range5Button == clickedButton;
        if (_range10Button != null) _range10Button.IsChecked = _range10Button == clickedButton;

        // Parse the range from the Tag property
        if (clickedButton.Tag is string tagStr && int.TryParse(tagStr, out var range))
        {
            _currentRange = range;
            _radarDisplay?.SetRange(range);
            UpdateRangeText();
            RefreshRadarDisplay();
        }
    }

    private void UpdateRangeText()
    {
        if (_rangeText != null)
        {
            _rangeText.Text = $"RANGE: {_currentRange} MI";
        }
    }

    private void RadarDisplay_BlipTapped(object? sender, Friend friend)
    {
        _friendDetailPanel?.ShowFriend(friend);
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        StopStatusTimer();
        _locationService.StopTracking();
        _locationService.Dispose();
        _supabaseService.Dispose();
    }


    private void StartStatusTimer()
    {
        _statusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
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
        // Update ping angle display
        if (_radarDisplay != null && _pingAngle != null)
        {
            var angle = _radarDisplay.CurrentSweepAngle;
            _pingAngle.Text = $"PING: {angle}°";
        }
    }

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        var deferral = args.GetDeferral();

        try
        {
            if (_radarDisplay != null)
            {
                await _radarDisplay.TriggerFullScanAsync();
            }

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
        if (_radarDisplay != null)
        {
            await _radarDisplay.TriggerFullScanAsync();
        }

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

    private async System.Threading.Tasks.Task ShowSuccessAsync(string message)
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

    private string GenerateLocalCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
