using System;

namespace HorizontalCalendar.Models;

/// <summary>
/// Represents an event or appointment on a specific date.
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for the event.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the title of the event.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the event.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the event.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the event.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the category of the event.
    /// </summary>
    public string Category { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the color associated with this event in hex format.
    /// </summary>
    public string ColorHex { get; set; } = "#3B82F6";

    /// <summary>
    /// Gets or sets whether this is an all-day event.
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Gets or sets the location of the event.
    /// </summary>
    public string Location { get; set; } = string.Empty;
}
