namespace settingspage.Presentation;

public partial record SettingsModel
{
	private readonly IWritableOptions<SettingsConfig> _settings;

	public SettingsModel(IWritableOptions<SettingsConfig> settings)
	{
		_settings = settings;
		var current = settings.Value;
		_enableNotifications = State.Value(this, () => current?.EnableNotifications ?? false);
		_darkMode = State.Value(this, () => current?.DarkMode ?? false);
		_autoSave = State.Value(this, () => current?.AutoSave ?? false);

		EnableNotifications.ForEach(async (val, ct) =>
			await _settings.UpdateAsync(c => c with { EnableNotifications = val }));
		DarkMode.ForEach(async (val, ct) =>
			await _settings.UpdateAsync(c => c with { DarkMode = val }));
		AutoSave.ForEach(async (val, ct) =>
			await _settings.UpdateAsync(c => c with { AutoSave = val }));
	}

	private IState<bool> _enableNotifications;
	public IState<bool> EnableNotifications => _enableNotifications;

	private IState<bool> _darkMode;
	public IState<bool> DarkMode => _darkMode;

	private IState<bool> _autoSave;
	public IState<bool> AutoSave => _autoSave;
}
