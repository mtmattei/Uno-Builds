using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace HorizontalCalendar.Models;

/// <summary>
/// Represents a single date in the calendar control.
/// </summary>
public class CalendarDate : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isToday;
    private ObservableCollection<CalendarEvent>? _events;

    /// <summary>
    /// Gets or sets the date value.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the collection of events for this date.
    /// </summary>
    public ObservableCollection<CalendarEvent> Events
    {
        get => _events ??= new ObservableCollection<CalendarEvent>();
        set
        {
            if (_events == value) return;
            
            // Unsubscribe from old collection
            if (_events != null)
                _events.CollectionChanged -= OnEventsCollectionChanged;

            _events = value;

            // Subscribe to new collection
            if (_events != null)
                _events.CollectionChanged += OnEventsCollectionChanged;

            OnPropertyChanged();
            UpdateEventProperties();
        }
    }

    /// <summary>
    /// Gets the badge count based on the number of events.
    /// </summary>
    public int BadgeCount => _events?.Count ?? 0;

    /// <summary>
    /// Gets whether to show the badge (when there are events).
    /// </summary>
    public bool ShowBadge => BadgeCount > 0;

    /// <summary>
    /// Gets or sets a value indicating whether this date is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            UpdateVisualProperties();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this date is today.
    /// </summary>
    public bool IsToday
    {
        get => _isToday;
        set
        {
            if (_isToday == value) return;
            _isToday = value;
            OnPropertyChanged();
            UpdateVisualProperties();
        }
    }

    /// <summary>
    /// Gets the border brush state as a string.
    /// </summary>
    public string BorderBrush => IsSelected ? "Selected" : "Default";

    /// <summary>
    /// Gets the selection background state for filled chip effect.
    /// </summary>
    public string SelectionBackground => IsSelected ? "SelectedFill" : (IsToday ? "TodayBackground" : "Default");

    /// <summary>
    /// Gets the opacity for selection background.
    /// </summary>
    public double SelectionOpacity => IsSelected ? 0.2 : (IsToday ? 1.0 : 0.0);

    /// <summary>
    /// Gets the border thickness based on state.
    /// </summary>
    public Thickness BorderThickness => IsSelected ? new Thickness(2) : new Thickness(0);

    /// <summary>
    /// Gets the font weight for the date number.
    /// </summary>
    public string DateFontWeight => IsSelected ? "Bold" : "SemiBold";

    /// <summary>
    /// Gets the foreground color state for the date text.
    /// </summary>
    public string DateForeground => IsSelected ? "SelectedText" : "DefaultText";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarDate"/> class.
    /// </summary>
    public CalendarDate()
    {
        _events = new ObservableCollection<CalendarEvent>();
        _events.CollectionChanged += OnEventsCollectionChanged;
    }

    /// <summary>
    /// Handles changes to the events collection.
    /// </summary>
    private void OnEventsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateEventProperties();
    }

    /// <summary>
    /// Updates properties that depend on the events collection.
    /// </summary>
    private void UpdateEventProperties()
    {
        OnPropertyChanged(nameof(BadgeCount));
        OnPropertyChanged(nameof(ShowBadge));
    }

    /// <summary>
    /// Updates all visual-related properties.
    /// </summary>
    private void UpdateVisualProperties()
    {
        OnPropertyChanged(nameof(BorderBrush));
        OnPropertyChanged(nameof(SelectionBackground));
        OnPropertyChanged(nameof(SelectionOpacity));
        OnPropertyChanged(nameof(BorderThickness));
        OnPropertyChanged(nameof(DateFontWeight));
        OnPropertyChanged(nameof(DateForeground));
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
