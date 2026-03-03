namespace settingspage.Models;

public record SettingsConfig
{
	public bool? EnableNotifications { get; init; }
	public bool? DarkMode { get; init; }
	public bool? AutoSave { get; init; }
}
