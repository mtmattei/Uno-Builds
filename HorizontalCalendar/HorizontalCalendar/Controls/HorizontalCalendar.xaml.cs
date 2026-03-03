using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Text;
using HorizontalCalendar.Models;
using HorizontalCalendar.Extensions;

namespace HorizontalCalendar.Controls;

/// <summary>
/// Represents a month item for the month picker.
/// </summary>
public class MonthItem
{
    public int MonthNumber { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsCurrentMonth { get; set; }
    public Windows.UI.Text.FontWeight FontWeight => IsCurrentMonth
        ? Microsoft.UI.Text.FontWeights.SemiBold
        : Microsoft.UI.Text.FontWeights.Normal;
    public Brush Foreground => IsCurrentMonth
        ? new SolidColorBrush(ColorHelper.FromArgb(255, 99, 102, 241)) // Accent #6366F1
        : new SolidColorBrush(ColorHelper.FromArgb(102, 255, 255, 255)); // TextMuted
}

/// <summary>
/// Represents a year item for the year picker.
/// </summary>
public class YearItem
{
    public int Year { get; set; }
    public bool IsCurrentYear { get; set; }
    public Windows.UI.Text.FontWeight FontWeight => IsCurrentYear
        ? Microsoft.UI.Text.FontWeights.SemiBold
        : Microsoft.UI.Text.FontWeights.Normal;
    public Brush Foreground => IsCurrentYear
        ? new SolidColorBrush(ColorHelper.FromArgb(255, 99, 102, 241)) // Accent #6366F1
        : new SolidColorBrush(ColorHelper.FromArgb(102, 255, 255, 255)); // TextMuted
}

public sealed partial class HorizontalCalendar : UserControl, INotifyPropertyChanged
{
    #region Static Brushes for x:Bind (Design Brief Colors)

    // Text Colors per Design Brief
    private static readonly Brush TextPrimaryBrush = new SolidColorBrush(Colors.White); // #FFFFFF
    private static readonly Brush TextSecondaryBrush = new SolidColorBrush(ColorHelper.FromArgb(217, 255, 255, 255)); // 85% white
    private static readonly Brush TextSubtleBrush = new SolidColorBrush(ColorHelper.FromArgb(89, 255, 255, 255)); // 35% white
    private static readonly Brush TextWeekendBrush = new SolidColorBrush(ColorHelper.FromArgb(128, 255, 182, 147)); // Weekend color
    private static readonly Brush AccentSoftBrush = new SolidColorBrush(ColorHelper.FromArgb(255, 165, 180, 252)); // #A5B4FC
    private static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);

    // Today gradient brush (subtle)
    private static readonly LinearGradientBrush TodayGradient = new()
    {
        StartPoint = new Windows.Foundation.Point(0, 0),
        EndPoint = new Windows.Foundation.Point(0.6, 1),
        GradientStops =
        {
            new GradientStop { Color = ColorHelper.FromArgb(38, 99, 102, 241), Offset = 0 }, // 15% #6366F1
            new GradientStop { Color = ColorHelper.FromArgb(20, 99, 102, 241), Offset = 1 }  // 8% #6366F1
        }
    };

    // Today border brush
    private static readonly Brush TodayBorderBrush = new SolidColorBrush(ColorHelper.FromArgb(77, 99, 102, 241)); // 30% #6366F1

    // Selected gradient brush (solid accent)
    private static readonly LinearGradientBrush SelectedGradient = new()
    {
        StartPoint = new Windows.Foundation.Point(0, 0),
        EndPoint = new Windows.Foundation.Point(0.6, 1),
        GradientStops =
        {
            new GradientStop { Color = ColorHelper.FromArgb(255, 99, 102, 241), Offset = 0 }, // #6366F1
            new GradientStop { Color = ColorHelper.FromArgb(255, 79, 70, 229), Offset = 1 }   // #4F46E5
        }
    };

    // Selected border brush
    private static readonly Brush SelectedBorderBrush = new SolidColorBrush(ColorHelper.FromArgb(51, 255, 255, 255)); // 20% white

    #endregion

    #region Static Helper Methods for x:Bind

    /// <summary>
    /// Gets the brush for day labels (weekends use warm accent color).
    /// </summary>
    public static Brush GetDayLabelBrush(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
            ? TextWeekendBrush
            : TextSubtleBrush;
    }

    /// <summary>
    /// Gets the background for today indicator (subtle gradient).
    /// </summary>
    public static Brush GetTodayBackground(bool isToday, bool isSelected)
    {
        if (isToday && !isSelected) return TodayGradient;
        return TransparentBrush;
    }

    /// <summary>
    /// Gets the border for today indicator.
    /// </summary>
    public static Brush GetTodayBorder(bool isToday, bool isSelected)
    {
        if (isToday && !isSelected) return TodayBorderBrush;
        return TransparentBrush;
    }

    /// <summary>
    /// Gets the selection background (solid accent gradient).
    /// </summary>
    public static Brush GetSelectionBackground(bool isSelected, bool isToday)
    {
        if (isSelected) return SelectedGradient;
        return TransparentBrush;
    }

    /// <summary>
    /// Gets the selection border.
    /// </summary>
    public static Brush GetSelectionBorder(bool isSelected)
    {
        if (isSelected) return SelectedBorderBrush;
        return TransparentBrush;
    }

    /// <summary>
    /// Gets the foreground color for date numbers.
    /// </summary>
    public static Brush GetDateForeground(bool isSelected, bool isToday)
    {
        if (isSelected) return TextPrimaryBrush; // White on selected
        if (isToday) return AccentSoftBrush; // Soft accent for today
        return TextSecondaryBrush; // Secondary for normal days
    }

    /// <summary>
    /// Shows the today indicator dot (only when today and not selected).
    /// </summary>
    public static bool ShowTodayDot(bool isToday, bool isSelected)
    {
        return isToday && !isSelected;
    }

    /// <summary>
    /// Shows event dots (when there are events and not showing today dot).
    /// </summary>
    public static bool ShowEventDots(bool showBadge, bool isToday, bool isSelected)
    {
        // Don't show event dots if we're showing the today dot
        if (isToday && !isSelected) return false;
        return showBadge;
    }

    /// <summary>
    /// Determines if a dot should be shown for the given event count and position.
    /// </summary>
    public static bool ShowDot(int badgeCount, int position)
    {
        return badgeCount >= position && badgeCount < 5;
    }

    /// <summary>
    /// Determines if a count badge should be shown (5+ events).
    /// </summary>
    public static bool ShowCountBadge(int badgeCount)
    {
        return badgeCount >= 5;
    }

    /// <summary>
    /// Gets the background brush for the entire day item (fills the shape when selected).
    /// </summary>
    public static Brush GetItemBackground(bool isSelected, bool isToday)
    {
        if (isSelected) return SelectedGradient;
        if (isToday) return TodayGradient;
        return TransparentBrush;
    }

    /// <summary>
    /// Gets the border brush for the entire day item.
    /// </summary>
    public static Brush GetItemBorder(bool isSelected, bool isToday)
    {
        if (isSelected) return SelectedBorderBrush;
        if (isToday) return TodayBorderBrush;
        return TransparentBrush;
    }

    /// <summary>
    /// Gets the Translation vector for elevation effect when selected.
    /// </summary>
    public static System.Numerics.Vector3 GetItemTranslation(bool isSelected)
    {
        return isSelected ? new System.Numerics.Vector3(0, 0, 8) : new System.Numerics.Vector3(0, 0, 0);
    }

    /// <summary>
    /// Gets the foreground brush for the day label (weekday abbreviation).
    /// </summary>
    public static Brush GetDayLabelForeground(bool isSelected, DateTime date)
    {
        if (isSelected) return TextPrimaryBrush;
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
            ? TextWeekendBrush
            : TextSubtleBrush;
    }

    /// <summary>
    /// Gets the indicator dot color based on state.
    /// </summary>
    public static Brush GetIndicatorColor(bool isSelected, bool isToday, bool hasEvents)
    {
        if (isSelected) return TextPrimaryBrush;
        if (isToday) return new SolidColorBrush(ColorHelper.FromArgb(255, 99, 102, 241)); // Accent
        return new SolidColorBrush(ColorHelper.FromArgb(255, 99, 102, 241)); // Accent for events
    }

    /// <summary>
    /// Determines if the indicator dot should be visible.
    /// </summary>
    public static Visibility ShowIndicatorDot(bool isToday, bool hasEvents)
    {
        return (isToday || hasEvents) ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    private const int DefaultBadgeDayIndex1 = 0;
    private const int DefaultBadgeDayIndex2 = 4;
    private const int YearRangeBefore = 10;
    private const int YearRangeAfter = 10;

    private DateTime _currentMonth;
    private ObservableCollection<CalendarDate> _dates = new();
    private ObservableCollection<MonthItem> _months = new();
    private ObservableCollection<YearItem> _years = new();
    private CalendarDate? _selectedDate;
    private DispatcherTimer? _midnightTimer;
    private ScrollViewer? _cachedScrollViewer;

    public ObservableCollection<CalendarDate> Dates
    {
        get => _dates;
        set { _dates = value; OnPropertyChanged(); }
    }

    public ObservableCollection<MonthItem> Months
    {
        get => _months;
        set { _months = value; OnPropertyChanged(); }
    }

    public ObservableCollection<YearItem> Years
    {
        get => _years;
        set { _years = value; OnPropertyChanged(); }
    }

    public string DisplayMonth => CurrentMonth.ToString("MMMM yyyy");
    public string DisplayMonthName => CurrentMonth.ToString("MMMM");
    public string DisplayYear => CurrentMonth.ToString("yyyy");

    /// <summary>
    /// Gets the selected date text for the info bar display.
    /// </summary>
    public string SelectedDateText
    {
        get
        {
            if (_selectedDate == null)
                return DateTime.Today.ToString("ddd, MMMM d");
            return _selectedDate.Date.ToString("ddd, MMMM d");
        }
    }

    public event EventHandler<CalendarDate>? DateSelected;
    public event EventHandler? AddEventRequested;
    public event EventHandler<DateTime>? AddEventForDateRequested;

    private void OnAddEventClicked(object sender, RoutedEventArgs e)
    {
        AddEventRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnTodayClicked(object sender, RoutedEventArgs e)
    {
        GoToToday();
    }

    /// <summary>
    /// Navigates to today's date and selects it.
    /// </summary>
    public void GoToToday()
    {
        var today = DateTime.Today;

        // If we're not in the current month, navigate to it
        if (CurrentMonth.Year != today.Year || CurrentMonth.Month != today.Month)
        {
            CurrentMonth = new DateTime(today.Year, today.Month, 1);
            LoadDates();
        }

        // Select today's date
        SelectDate(today);
    }

    /// <summary>
    /// Selects the specified date.
    /// </summary>
    public void SelectDate(DateTime date)
    {
        var targetDate = Dates.FirstOrDefault(d => d.Date.Date == date.Date);
        if (targetDate == null) return;

        // Deselect previous
        if (_selectedDate != null) _selectedDate.IsSelected = false;

        // Select new
        targetDate.IsSelected = true;
        _selectedDate = targetDate;

        // Update UI
        OnPropertyChanged(nameof(SelectedDateText));
        RefreshTodayHighlight();

        // Scroll to the date
        ScrollToDate(targetDate, animate: true);

        // Raise event
        DateSelected?.Invoke(this, targetDate);
    }

    public Func<DateTime, DateTime, IEnumerable<CalendarEvent>>? EventsProvider { get; set; }

    private DateTime CurrentMonth
    {
        get => _currentMonth;
        set
        {
            _currentMonth = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayMonth));
            OnPropertyChanged(nameof(DisplayMonthName));
            OnPropertyChanged(nameof(DisplayYear));
        }
    }

    public HorizontalCalendar()
    {
        this.InitializeComponent();
        CurrentMonth = DateTime.Today;
        LoadMonths();
        LoadYears();
        LoadDates();
        SetupMidnightTimer();

        // Select today by default
        SelectTodayOnLoad();

        Unloaded += (s, e) => _midnightTimer?.Stop();
    }

    private void SelectTodayOnLoad()
    {
        var today = Dates.FirstOrDefault(d => d.IsToday);
        if (today != null)
        {
            today.IsSelected = true;
            _selectedDate = today;
            OnPropertyChanged(nameof(SelectedDateText));
        }
    }

    private void SetupMidnightTimer()
    {
        var now = DateTime.Now;
        var tomorrow = now.Date.AddDays(1);
        var timeUntilMidnight = tomorrow - now;

        _midnightTimer = new DispatcherTimer();
        _midnightTimer.Interval = timeUntilMidnight;
        _midnightTimer.Tick += OnMidnightTimerTick;
        _midnightTimer.Start();
    }

    private void OnMidnightTimerTick(object? sender, object e)
    {
        RefreshTodayHighlight();
        _midnightTimer?.Stop();
        SetupMidnightTimer();
    }

    #region Month/Year Picker

    private void LoadMonths()
    {
        var months = new ObservableCollection<MonthItem>();
        var currentMonthNum = CurrentMonth.Month;

        for (int i = 1; i <= 12; i++)
        {
            months.Add(new MonthItem
            {
                MonthNumber = i,
                ShortName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i),
                FullName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                IsCurrentMonth = i == currentMonthNum
            });
        }

        Months = months;
    }

    private void LoadYears()
    {
        var years = new ObservableCollection<YearItem>();
        var currentYear = CurrentMonth.Year;
        var startYear = currentYear - YearRangeBefore;
        var endYear = currentYear + YearRangeAfter;

        for (int y = startYear; y <= endYear; y++)
        {
            years.Add(new YearItem
            {
                Year = y,
                IsCurrentYear = y == currentYear
            });
        }

        Years = years;
    }

    #endregion

    #region Scrolling

    private const double ScrollAmount = 280; // Pixels to scroll per arrow click (roughly 4 days)

    private void OnScrollViewerPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            var properties = e.GetCurrentPoint(scrollViewer).Properties;
            var delta = properties.MouseWheelDelta;

            // Convert vertical scroll to horizontal
            var currentOffset = scrollViewer.HorizontalOffset;
            var newOffset = currentOffset - delta;

            scrollViewer.ChangeView(newOffset, null, null, true);
            e.Handled = true;
        }
    }

    private void OnScrollLeft(object sender, RoutedEventArgs e)
    {
        var scrollViewer = GetCachedScrollViewer();
        if (scrollViewer == null) return;

        var newOffset = Math.Max(0, scrollViewer.HorizontalOffset - ScrollAmount);
        scrollViewer.ChangeView(newOffset, null, null, false);
    }

    private void OnScrollRight(object sender, RoutedEventArgs e)
    {
        var scrollViewer = GetCachedScrollViewer();
        if (scrollViewer == null) return;

        var maxOffset = scrollViewer.ScrollableWidth;
        var newOffset = Math.Min(maxOffset, scrollViewer.HorizontalOffset + ScrollAmount);
        scrollViewer.ChangeView(newOffset, null, null, false);
    }

    private void OnScrollViewerViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        UpdateScrollArrowVisibility();
    }

    private void UpdateScrollArrowVisibility()
    {
        var scrollViewer = GetCachedScrollViewer();
        if (scrollViewer == null) return;

        // Show/hide arrows based on scroll position
        var atStart = scrollViewer.HorizontalOffset <= 1;
        var atEnd = scrollViewer.HorizontalOffset >= scrollViewer.ScrollableWidth - 1;

        ScrollLeftButton.IsEnabled = !atStart;
        ScrollRightButton.IsEnabled = !atEnd;
    }

    #endregion

    private void LoadDates()
    {
        var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
        var lastDay = firstDay.AddDays(daysInMonth - 1);

        // Safe event loading with error handling
        IEnumerable<CalendarEvent> monthEvents;
        try
        {
            monthEvents = EventsProvider?.Invoke(firstDay, lastDay) ?? Enumerable.Empty<CalendarEvent>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading events: {ex.Message}");
            monthEvents = Enumerable.Empty<CalendarEvent>();
        }

        var eventsByDate = monthEvents.GroupBy(e => e.StartTime.Date).ToDictionary(g => g.Key, g => g.ToList());

        var existingDates = Dates.ToList();
        var newDates = new List<CalendarDate>();

        for (int i = 0; i < daysInMonth; i++)
        {
            var date = firstDay.AddDays(i);
            var existingDate = existingDates.FirstOrDefault(d => d.Date.Date == date.Date);

            if (existingDate != null)
            {
                existingDate.IsSelected = _selectedDate?.Date.Date == date.Date;
                existingDate.IsToday = date.Date == DateTime.Today.Date;
                existingDate.Events.Clear();
                if (eventsByDate.TryGetValue(date.Date, out var events))
                    foreach (var evt in events) existingDate.Events.Add(evt);
                newDates.Add(existingDate);
            }
            else
            {
                var calendarDate = new CalendarDate
                {
                    Date = date,
                    IsSelected = _selectedDate?.Date.Date == date.Date,
                    IsToday = date.Date == DateTime.Today.Date
                };

                if (eventsByDate.TryGetValue(date.Date, out var events))
                {
                    foreach (var evt in events) calendarDate.Events.Add(evt);
                }
                else if (EventsProvider == null && (i == DefaultBadgeDayIndex1 || i == DefaultBadgeDayIndex2))
                {
                    calendarDate.Events.Add(new CalendarEvent
                    {
                        Title = "Sample Event",
                        Description = "This is a sample event",
                        StartTime = date.AddHours(10),
                        EndTime = date.AddHours(11),
                        Category = "Meeting",
                        ColorHex = "#6366F1"
                    });
                }
                newDates.Add(calendarDate);
            }
        }

        UpdateDateCollection(newDates);
        RefreshTodayHighlight();
        ScrollToToday();
    }

    private void ScrollToToday()
    {
        ScrollToDate(Dates.FirstOrDefault(d => d.IsToday), animate: false);
    }

    /// <summary>
    /// Gets the scroll item width from resources or uses default.
    /// </summary>
    private double GetScrollItemWidth()
    {
        if (Resources.TryGetValue("CalendarScrollItemWidth", out var value) && value is double width)
            return width;

        // Fallback: DayWidth (64) + ItemSpacing (6) = 70
        return 70.0;
    }

    private async void ScrollToDate(CalendarDate? targetDate, bool animate = true, bool highlight = false)
    {
        if (targetDate == null) return;

        await DispatcherQueue.EnqueueAsync(async () =>
        {
            var scrollViewer = GetCachedScrollViewer();
            if (scrollViewer == null) return;

            var index = Dates.IndexOf(targetDate);
            if (index < 0) return;

            var itemWidth = GetScrollItemWidth();
            var scrollPosition = index * itemWidth;

            if (animate)
                scrollViewer.ChangeView(scrollPosition, null, null, false);
            else
                scrollViewer.ScrollToHorizontalOffset(scrollPosition);

            if (highlight)
            {
                var originalIsToday = targetDate.IsToday;
                targetDate.IsToday = true;
                await Task.Delay(1500);
                targetDate.IsToday = originalIsToday;
            }
        });
    }

    /// <summary>
    /// Gets the cached ScrollViewer or finds and caches it.
    /// </summary>
    private ScrollViewer? GetCachedScrollViewer()
    {
        if (_cachedScrollViewer != null)
            return _cachedScrollViewer;

        _cachedScrollViewer = CalendarScrollViewer;
        return _cachedScrollViewer;
    }

    private void UpdateDateCollection(List<CalendarDate> newDates)
    {
        Dates.Clear();
        foreach (var date in newDates) Dates.Add(date);
    }

    private void OnDateClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not CalendarDate clickedDate || Dates == null)
            return;
        if (_selectedDate?.Date.Date == clickedDate.Date.Date)
            return;

        if (_selectedDate != null) _selectedDate.IsSelected = false;
        clickedDate.IsSelected = true;
        _selectedDate = clickedDate;

        // Update selected date text
        OnPropertyChanged(nameof(SelectedDateText));

        // Ensure today always stays highlighted
        RefreshTodayHighlight();

        // Run selection animation
        if (button.RenderTransform is CompositeTransform transform)
        {
            RunSelectionAnimation(transform);
        }

        // Scroll and notify
        ScrollToDate(clickedDate, animate: true);
        DateSelected?.Invoke(this, clickedDate);
    }

    /// <summary>
    /// Runs the scale animation for date selection.
    /// </summary>
    private static void RunSelectionAnimation(CompositeTransform transform)
    {
        var storyboard = new Storyboard();

        var scaleX = new DoubleAnimationUsingKeyFrames();
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 1.0 });
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(150), Value = 1.05 });
        scaleX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(300), Value = 1.02 });

        var scaleY = new DoubleAnimationUsingKeyFrames();
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.Zero, Value = 1.0 });
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(150), Value = 1.05 });
        scaleY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(300), Value = 1.02 });

        Storyboard.SetTarget(scaleX, transform);
        Storyboard.SetTargetProperty(scaleX, "ScaleX");
        Storyboard.SetTarget(scaleY, transform);
        Storyboard.SetTargetProperty(scaleY, "ScaleY");

        storyboard.Children.Add(scaleX);
        storyboard.Children.Add(scaleY);
        storyboard.Begin();
    }

    public void RefreshEvents()
    {
        LoadDates();
    }

    public void AddEvent(DateTime date, CalendarEvent calendarEvent)
    {
        var calendarDate = Dates.FirstOrDefault(d => d.Date.Date == date.Date);
        if (calendarDate != null)
        {
            calendarDate.Events.Add(calendarEvent);
        }
    }

    public void RemoveEvent(DateTime date, string eventId)
    {
        var calendarDate = Dates.FirstOrDefault(d => d.Date.Date == date.Date);
        if (calendarDate != null)
        {
            var eventToRemove = calendarDate.Events.FirstOrDefault(e => e.Id == eventId);
            if (eventToRemove != null)
            {
                calendarDate.Events.Remove(eventToRemove);
            }
        }
    }

    private void OnPrevMonth(object sender, RoutedEventArgs e)
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        LoadDates();
        RefreshTodayHighlight();
    }

    private void OnNextMonth(object sender, RoutedEventArgs e)
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        LoadDates();
        RefreshTodayHighlight();
    }

    private void RefreshTodayHighlight()
    {
        foreach (var date in Dates)
        {
            date.IsToday = date.Date.Date == DateTime.Today.Date;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
