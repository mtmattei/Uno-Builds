namespace Orbital.Helpers;

/// <summary>
/// Static helper for reading and writing Orbital user settings
/// persisted to %LOCALAPPDATA%/Orbital/user-settings.json.
/// </summary>
public static class SettingsService
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Orbital");
    private static readonly string SettingsFile = Path.Combine(SettingsDir, "user-settings.json");

    public static string? GetStoredUsername()
    {
        var name = ReadSetting("username");
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    public static void SaveUsername(string name)
        => WriteSetting("username", name);

    public static string? ReadSetting(string key)
    {
        try
        {
            if (!File.Exists(SettingsFile)) return null;
            var json = File.ReadAllText(SettingsFile);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out var val))
                return val.GetString();
        }
        catch { }
        return null;
    }

    public static void WriteSetting(string key, string value)
    {
        try
        {
            Dictionary<string, string> settings = new();
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var existing = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (existing is not null) settings = existing;
            }
            settings[key] = value;
            Directory.CreateDirectory(SettingsDir);
            File.WriteAllText(SettingsFile, System.Text.Json.JsonSerializer.Serialize(settings));
        }
        catch { }
    }
}
