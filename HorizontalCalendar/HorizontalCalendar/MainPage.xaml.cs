using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using HorizontalCalendar.Models;

namespace HorizontalCalendar;

public sealed partial class MainPage : Page, INotifyPropertyChanged
{
    private readonly List<CalendarEvent> _sampleEvents;
    private ObservableCollection<CalendarEvent> _selectedEvents = new();
    private DateTime _selectedDate = DateTime.Today;
    private bool _hasEvents = false;

    // Event categories with their colors (Design Brief accent colors)
    private static readonly Dictionary<string, string> EventCategories = new()
    {
        { "Meeting", "#6366F1" },      // Indigo (Accent Primary)
        { "Standup", "#F59E0B" },      // Amber
        { "External", "#10B981" },     // Emerald
        { "Focus", "#8B5CF6" },        // Violet
        { "Personal", "#EC4899" },     // Pink
        { "Social", "#F59E0B" },       // Amber
        { "Deadline", "#EF4444" },     // Red
        { "Other", "#64748B" }         // Slate
    };

    /// <summary>
    /// Gets the collection of events for the selected date.
    /// </summary>
    public ObservableCollection<CalendarEvent> SelectedEvents
    {
        get => _selectedEvents;
        private set
        {
            _selectedEvents = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets whether there are events to display.
    /// </summary>
    public Visibility HasEvents
    {
        get => _hasEvents ? Visibility.Visible : Visibility.Collapsed;
    }

    public MainPage()
    {
        this.InitializeComponent();

        // Initialize sample events
        _sampleEvents = GenerateSampleEvents();

        // Set up the events provider
        MyCalendar.EventsProvider = GetEventsForDateRange;

        // Subscribe to add event request
        MyCalendar.AddEventRequested += OnAddEventRequested;
        MyCalendar.AddEventForDateRequested += OnAddEventForDateRequested;

        // Refresh the calendar to load events
        MyCalendar.RefreshEvents();
    }

    #region Static Helper Methods for x:Bind

    /// <summary>
    /// Checks if a string has a non-empty value (for visibility binding).
    /// </summary>
    public static Visibility HasValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Formats event time display.
    /// </summary>
    public static string FormatEventTime(bool isAllDay, DateTime startTime, DateTime endTime)
        => isAllDay ? "All Day" : $"{startTime:h:mm tt} - {endTime:h:mm tt}";

    /// <summary>
    /// Formats just the start time for the event card.
    /// </summary>
    public static string FormatStartTime(bool isAllDay, DateTime startTime)
        => isAllDay ? "All Day" : startTime.ToString("h:mm tt");

    /// <summary>
    /// Formats the duration between start and end time.
    /// </summary>
    public static string FormatDuration(DateTime startTime, DateTime endTime)
    {
        var duration = endTime - startTime;
        if (duration.TotalHours >= 1)
        {
            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
        return $"{(int)duration.TotalMinutes}m";
    }

    /// <summary>
    /// Shows duration chip only for non-all-day events.
    /// </summary>
    public static Visibility ShowDuration(bool isAllDay)
        => isAllDay ? Visibility.Collapsed : Visibility.Visible;

    #endregion

    /// <summary>
    /// Generates sample events for demonstration.
    /// </summary>
    private List<CalendarEvent> GenerateSampleEvents()
    {
        var events = new List<CalendarEvent>();
        var today = DateTime.Today;

        // Add events for the current month to ensure visibility
        var currentMonth = new DateTime(today.Year, today.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

        // Team meeting - every Monday (Indigo)
        for (int i = -7; i < 30; i++)
        {
            var date = today.AddDays(i);
            if (date.DayOfWeek == DayOfWeek.Monday)
            {
                events.Add(new CalendarEvent
                {
                    Title = "Team Meeting",
                    Description = "Weekly team sync-up meeting",
                    StartTime = date.AddHours(9),
                    EndTime = date.AddHours(10),
                    Category = "Meeting",
                    ColorHex = "#6366F1", // Indigo
                    Location = "Conference Room A"
                });
            }
        }

        // Add multiple events on today for a rich demo
        events.Add(new CalendarEvent
        {
            Title = "Morning Standup",
            Description = "Daily team standup - review blockers and priorities",
            StartTime = today.AddHours(9),
            EndTime = today.AddHours(9.5),
            Category = "Standup",
            ColorHex = "#F59E0B", // Amber
            Location = "Teams"
        });

        events.Add(new CalendarEvent
        {
            Title = "Client Strategy Call",
            Description = "Discuss Q1 roadmap and deliverables",
            StartTime = today.AddHours(11),
            EndTime = today.AddHours(12),
            Category = "External",
            ColorHex = "#10B981", // Emerald
            Location = "Zoom"
        });

        events.Add(new CalendarEvent
        {
            Title = "Lunch Break",
            Description = "Team lunch at the new place",
            StartTime = today.AddHours(12.5),
            EndTime = today.AddHours(13.5),
            Category = "Social",
            ColorHex = "#F59E0B", // Amber
            Location = "Downtown Restaurant"
        });

        events.Add(new CalendarEvent
        {
            Title = "Code Review Session",
            Description = "Review pull requests for the auth module",
            StartTime = today.AddHours(14),
            EndTime = today.AddHours(15.5),
            Category = "Focus",
            ColorHex = "#8B5CF6", // Violet
            Location = "Online"
        });

        events.Add(new CalendarEvent
        {
            Title = "1:1 with Manager",
            Description = "Weekly sync and career discussion",
            StartTime = today.AddHours(16),
            EndTime = today.AddHours(16.5),
            Category = "Meeting",
            ColorHex = "#6366F1", // Indigo
            Location = "Office"
        });

        // Tomorrow events
        var tomorrow = today.AddDays(1);
        events.Add(new CalendarEvent
        {
            Title = "Project Deadline",
            Description = "Submit final deliverables for Phase 1",
            StartTime = tomorrow.AddHours(17),
            EndTime = tomorrow.AddHours(18),
            Category = "Deadline",
            ColorHex = "#EF4444", // Red
            IsAllDay = false
        });

        events.Add(new CalendarEvent
        {
            Title = "Sprint Planning",
            Description = "Plan next sprint goals and tasks",
            StartTime = tomorrow.AddHours(10),
            EndTime = tomorrow.AddHours(11.5),
            Category = "Meeting",
            ColorHex = "#6366F1", // Indigo
            Location = "Main Conference Room"
        });

        // Day +2 events
        events.Add(new CalendarEvent
        {
            Title = "Design Review",
            Description = "Review new dashboard mockups",
            StartTime = today.AddDays(2).AddHours(14),
            EndTime = today.AddDays(2).AddHours(15),
            Category = "Focus",
            ColorHex = "#8B5CF6", // Violet
            Location = "Design Lab"
        });

        // Day +3 events
        events.Add(new CalendarEvent
        {
            Title = "Dentist Appointment",
            Description = "Regular checkup",
            StartTime = today.AddDays(3).AddHours(14),
            EndTime = today.AddDays(3).AddHours(15),
            Category = "Personal",
            ColorHex = "#EC4899", // Pink
            Location = "Downtown Dental Clinic"
        });

        // Day +5 events
        events.Add(new CalendarEvent
        {
            Title = "Product Demo",
            Description = "Demo new features to stakeholders",
            StartTime = today.AddDays(5).AddHours(15),
            EndTime = today.AddDays(5).AddHours(16),
            Category = "External",
            ColorHex = "#10B981", // Emerald
            Location = "Main Stage"
        });

        // Birthday - Day +7
        events.Add(new CalendarEvent
        {
            Title = "Sarah's Birthday",
            Description = "Don't forget to wish Sarah!",
            StartTime = today.AddDays(7),
            EndTime = today.AddDays(7),
            Category = "Social",
            ColorHex = "#EC4899", // Pink
            IsAllDay = true
        });

        return events;
    }

    /// <summary>
    /// Provides events for a given date range.
    /// </summary>
    private IEnumerable<CalendarEvent> GetEventsForDateRange(DateTime startDate, DateTime endDate)
    {
        var results = _sampleEvents.Where(e =>
            e.StartTime.Date >= startDate.Date &&
            e.StartTime.Date <= endDate.Date).ToList();

        System.Diagnostics.Debug.WriteLine(
            $"GetEventsForDateRange: {startDate:MM/dd} to {endDate:MM/dd} - Found {results.Count} events");
        foreach (var evt in results)
        {
            System.Diagnostics.Debug.WriteLine($"  - {evt.StartTime:MM/dd}: {evt.Title}");
        }

        return results;
    }

    /// <summary>
    /// Handles date selection in the calendar.
    /// </summary>
    private void OnDateSelected(object? sender, CalendarDate selectedDate)
    {
        _selectedDate = selectedDate.Date;

        // Update events collection - ItemsRepeater handles the rendering
        SelectedEvents.Clear();
        foreach (var evt in selectedDate.Events.OrderBy(e => e.StartTime))
        {
            SelectedEvents.Add(evt);
        }

        // Update has events flag
        _hasEvents = SelectedEvents.Count > 0;
        OnPropertyChanged(nameof(HasEvents));

        // Toggle empty state visibility
        EmptyStatePanel.Visibility = SelectedEvents.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Handles the Add Event button click.
    /// </summary>
    private void OnAddEventRequested(object? sender, EventArgs e)
    {
        _ = ShowAddEventDialog(_selectedDate);
    }

    /// <summary>
    /// Handles add event request with a specific date.
    /// </summary>
    private void OnAddEventForDateRequested(object? sender, DateTime date)
    {
        _selectedDate = date;
    }

    /// <summary>
    /// Handles delete event button click.
    /// </summary>
    private void OnDeleteEventClicked(object sender, RoutedEventArgs e)
    {
        // Get the event from the MenuFlyoutItem's DataContext or walk up tree
        if (sender is MenuFlyoutItem menuItem)
        {
            // Find the parent button to get the Tag
            var parent = menuItem.Parent;
            while (parent != null)
            {
                if (parent is MenuFlyout flyout && flyout.Target is Button button && button.Tag is CalendarEvent evt)
                {
                    DeleteEvent(evt);
                    return;
                }
                parent = (parent as FrameworkElement)?.Parent;
            }

            // Alternative: try DataContext
            if (menuItem.DataContext is CalendarEvent eventFromContext)
            {
                DeleteEvent(eventFromContext);
            }
        }
    }

    /// <summary>
    /// Deletes an event from the calendar.
    /// </summary>
    private void DeleteEvent(CalendarEvent eventToDelete)
    {
        // Remove from sample events
        _sampleEvents.Remove(eventToDelete);

        // Remove from selected events list
        SelectedEvents.Remove(eventToDelete);

        // Refresh the calendar to update badges
        MyCalendar.RefreshEvents();

        // Update has events flag
        _hasEvents = SelectedEvents.Count > 0;
        OnPropertyChanged(nameof(HasEvents));

        // Toggle empty state visibility
        EmptyStatePanel.Visibility = SelectedEvents.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async System.Threading.Tasks.Task ShowAddEventDialog(DateTime forDate)
    {
        // Theme colors from design system (matches CalendarResources.xaml)
        var surfaceBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 20, 20, 31)); // #14141F - slightly lighter for dialog
        var borderColor = new SolidColorBrush(ColorHelper.FromArgb(26, 255, 255, 255)); // #1AFFFFFF
        var textPrimary = new SolidColorBrush(ColorHelper.FromArgb(255, 255, 255, 255));
        var textSecondary = new SolidColorBrush(ColorHelper.FromArgb(217, 255, 255, 255));
        var textMuted = new SolidColorBrush(ColorHelper.FromArgb(102, 255, 255, 255));

        // Font families matching design system
        var displayFont = new FontFamily("Georgia, serif");
        var uiFont = new FontFamily("DM Sans, Segoe UI, -apple-system, BlinkMacSystemFont, sans-serif");

        // Create the form content - set RequestedTheme on the panel for child inheritance
        var formPanel = new StackPanel
        {
            Spacing = 16,
            MinWidth = 320,
            RequestedTheme = ElementTheme.Dark
        };

        // Event Title
        var titleLabel = new TextBlock
        {
            Text = "TITLE",
            Foreground = textMuted,
            FontFamily = uiFont,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            FontSize = 11,
            CharacterSpacing = 60,
            Margin = new Thickness(0, 0, 0, 4)
        };
        // Create a style that forces light foreground for TextBox on dark background
        var darkTextBoxStyle = new Style(typeof(TextBox));
        darkTextBoxStyle.Setters.Add(new Setter(TextBox.ForegroundProperty, textPrimary));
        darkTextBoxStyle.Setters.Add(new Setter(TextBox.BackgroundProperty, new SolidColorBrush(ColorHelper.FromArgb(40, 255, 255, 255))));
        darkTextBoxStyle.Setters.Add(new Setter(TextBox.BorderBrushProperty, borderColor));
        darkTextBoxStyle.Setters.Add(new Setter(TextBox.FontFamilyProperty, uiFont));
        darkTextBoxStyle.Setters.Add(new Setter(TextBox.RequestedThemeProperty, ElementTheme.Dark));

        var titleBox = new TextBox
        {
            PlaceholderText = "Enter event title",
            Style = darkTextBoxStyle
        };
        var titleGroup = new StackPanel { Spacing = 4 };
        titleGroup.Children.Add(titleLabel);
        titleGroup.Children.Add(titleBox);
        formPanel.Children.Add(titleGroup);

        // Description
        var descLabel = new TextBlock
        {
            Text = "DESCRIPTION",
            Foreground = textMuted,
            FontFamily = uiFont,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            FontSize = 11,
            CharacterSpacing = 60,
            Margin = new Thickness(0, 0, 0, 4)
        };
        var descriptionBox = new TextBox
        {
            PlaceholderText = "Add a description (optional)",
            Style = darkTextBoxStyle,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 56,
            MaxHeight = 80
        };
        var descGroup = new StackPanel { Spacing = 4 };
        descGroup.Children.Add(descLabel);
        descGroup.Children.Add(descriptionBox);
        formPanel.Children.Add(descGroup);

        // Date display (read-only for now) - using display font (Georgia)
        var dateText = new TextBlock
        {
            Text = $"{forDate:dddd, MMMM d, yyyy}",
            Foreground = textSecondary,
            FontFamily = displayFont,
            FontSize = 14,
            FontStyle = Windows.UI.Text.FontStyle.Italic,
            Margin = new Thickness(0, 4, 0, 0)
        };
        formPanel.Children.Add(dateText);

        // All Day toggle
        var allDayToggle = new ToggleSwitch
        {
            Header = "All-day event",
            IsOn = false
        };
        formPanel.Children.Add(allDayToggle);

        // Time pickers container - horizontal layout
        var timePanel = new Grid { Margin = new Thickness(0, 0, 0, 0) };
        timePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        timePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16, GridUnitType.Pixel) });
        timePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Start Time
        var startTimeLabel = new TextBlock
        {
            Text = "START",
            Foreground = textMuted,
            FontFamily = uiFont,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            FontSize = 11,
            CharacterSpacing = 60,
            Margin = new Thickness(0, 0, 0, 4)
        };
        var startTimePicker = new TimePicker
        {
            ClockIdentifier = "12HourClock",
            Time = new TimeSpan(9, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var startGroup = new StackPanel { Spacing = 4 };
        startGroup.Children.Add(startTimeLabel);
        startGroup.Children.Add(startTimePicker);
        Grid.SetColumn(startGroup, 0);
        timePanel.Children.Add(startGroup);

        // End Time
        var endTimeLabel = new TextBlock
        {
            Text = "END",
            Foreground = textMuted,
            FontFamily = uiFont,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            FontSize = 11,
            CharacterSpacing = 60,
            Margin = new Thickness(0, 0, 0, 4)
        };
        var endTimePicker = new TimePicker
        {
            ClockIdentifier = "12HourClock",
            Time = new TimeSpan(10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var endGroup = new StackPanel { Spacing = 4 };
        endGroup.Children.Add(endTimeLabel);
        endGroup.Children.Add(endTimePicker);
        Grid.SetColumn(endGroup, 2);
        timePanel.Children.Add(endGroup);

        formPanel.Children.Add(timePanel);

        // Toggle time pickers visibility based on all-day
        allDayToggle.Toggled += (s, e) =>
        {
            timePanel.Visibility = allDayToggle.IsOn ? Visibility.Collapsed : Visibility.Visible;
        };

        // Location
        var locationLabel = new TextBlock
        {
            Text = "LOCATION",
            Foreground = textMuted,
            FontFamily = uiFont,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            FontSize = 11,
            CharacterSpacing = 60,
            Margin = new Thickness(0, 0, 0, 4)
        };
        var locationBox = new TextBox
        {
            PlaceholderText = "Add a location (optional)",
            Style = darkTextBoxStyle
        };
        var locationGroup = new StackPanel { Spacing = 4 };
        locationGroup.Children.Add(locationLabel);
        locationGroup.Children.Add(locationBox);
        formPanel.Children.Add(locationGroup);

        // Category selector
        var categoryLabel = new TextBlock
        {
            Text = "CATEGORY",
            Foreground = textMuted,
            FontFamily = uiFont,
            FontWeight = Microsoft.UI.Text.FontWeights.Medium,
            FontSize = 11,
            CharacterSpacing = 60,
            Margin = new Thickness(0, 0, 0, 4)
        };
        var categoryCombo = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            FontFamily = uiFont,
            ItemsSource = EventCategories.Keys.ToList(),
            SelectedIndex = 0
        };
        var categoryGroup = new StackPanel { Spacing = 4 };
        categoryGroup.Children.Add(categoryLabel);
        categoryGroup.Children.Add(categoryCombo);
        formPanel.Children.Add(categoryGroup);

        // Wrap in ScrollViewer for scrollable content
        var scrollViewer = new ScrollViewer
        {
            Content = formPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = 400,
            Padding = new Thickness(0, 0, 8, 0)
        };

        // Create styled title matching app design
        var titleTextBlock = new TextBlock
        {
            Text = "New Event",
            FontFamily = displayFont,
            FontSize = 24,
            Foreground = textPrimary
        };

        var dialog = new ContentDialog
        {
            Title = titleTextBlock,
            Content = scrollViewer,
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot,
            RequestedTheme = ElementTheme.Dark,
            Background = surfaceBackground,
            BorderBrush = borderColor,
            CornerRadius = new CornerRadius(16)
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Validate title
            if (string.IsNullOrWhiteSpace(titleBox.Text))
            {
                await ShowErrorDialog("Please enter an event title.");
                return;
            }

            // Create the event
            var category = categoryCombo.SelectedItem?.ToString() ?? "Other";
            var colorHex = EventCategories.GetValueOrDefault(category, "#64748B");

            var newEvent = new CalendarEvent
            {
                Title = titleBox.Text.Trim(),
                Description = descriptionBox.Text?.Trim(),
                Location = locationBox.Text?.Trim(),
                Category = category,
                ColorHex = colorHex,
                IsAllDay = allDayToggle.IsOn,
                StartTime = allDayToggle.IsOn
                    ? forDate.Date
                    : forDate.Date.Add(startTimePicker.Time),
                EndTime = allDayToggle.IsOn
                    ? forDate.Date
                    : forDate.Date.Add(endTimePicker.Time)
            };

            // Add to sample events
            _sampleEvents.Add(newEvent);

            // Refresh the calendar
            MyCalendar.RefreshEvents();

            // If this is for the currently selected date, update the events list
            if (forDate.Date == _selectedDate.Date)
            {
                SelectedEvents.Add(newEvent);
                _hasEvents = true;
                OnPropertyChanged(nameof(HasEvents));
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
        }
    }

    private async System.Threading.Tasks.Task ShowErrorDialog(string message)
    {
        var errorDialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };

        await errorDialog.ShowAsync();
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
