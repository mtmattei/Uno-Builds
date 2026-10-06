using System.Collections.ObjectModel;
using FreewriteUno.Models;
using FreewriteUno.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FreewriteUno.ViewModels;

[Bindable]
public sealed partial class MainViewModel : ObservableObject
{
    private const int SaveDebounceMs = 150;

    private readonly IEntryStore _entryStore;
    private readonly ISettingsStore _settingsStore;

    private CancellationTokenSource? _saveCts;
    private Task? _pendingSave;
    private bool _suppressSave;

    public MainViewModel(IEntryStore entryStore, ISettingsStore settingsStore)
    {
        _entryStore = entryStore;
        _settingsStore = settingsStore;
        _theme = settingsStore.Theme;
    }

    public ObservableCollection<Entry> Entries { get; } = new();

    [ObservableProperty] private Entry? _selectedEntry;
    [ObservableProperty] private string _text = string.Empty;
    [ObservableProperty] private int _timeRemaining = 900;
    [ObservableProperty] private bool _timerIsRunning;
    [ObservableProperty] private bool _backspaceDisabled;
    [ObservableProperty] private double _fontSize = 18;
    [ObservableProperty] private string _selectedFont = "Lato";
    [ObservableProperty] private ElementTheme _theme = ElementTheme.Light;
    [ObservableProperty] private bool _isSidebarOpen;
    [ObservableProperty] private double _chromeOpacity = 1.0;

    // Post-session edit mode: the inline AI pill is suppressed while writing and enabled only in
    // review mode (set true when the countdown completes; also togglable for untimed sessions).
    [ObservableProperty] private bool _isReviewMode;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var existing = await _entryStore.LoadAllAsync(ct).ConfigureAwait(true);
        Entries.Clear();
        foreach (var e in existing) Entries.Add(e);

        var today = DateTimeOffset.Now.Date;
        var todaysEmpty = existing.FirstOrDefault(e => e.CreatedAt.Date == today && e.IsEmpty);

        if (todaysEmpty is not null)
        {
            await SelectAsync(todaysEmpty, ct).ConfigureAwait(true);
        }
        else
        {
            var created = await _entryStore.CreateAsync(ct).ConfigureAwait(true);
            Entries.Insert(0, created);
            await SelectAsync(created, ct).ConfigureAwait(true);
        }
    }

    private async Task SelectAsync(Entry entry, CancellationToken ct = default)
    {
        await FlushPendingSaveAsync().ConfigureAwait(true);
        _suppressSave = true;
        try
        {
            var content = await _entryStore.ReadAsync(entry, ct).ConfigureAwait(true);
            content = content.TrimStart('\n', '\r'); // strip the legacy leading padding stored in files
            SelectedEntry = entry;
            Text = content;
        }
        finally
        {
            _suppressSave = false;
        }
    }

    partial void OnTextChanged(string value)
    {
        if (_suppressSave) return;
        if (SelectedEntry is null) return;
        ScheduleDebouncedSave(SelectedEntry, value);
    }

    partial void OnSelectedEntryChanged(Entry? value)
    {
        if (value is null || _suppressSave) return;
        IsSidebarOpen = false;
        _ = SelectAsync(value);
    }

    partial void OnThemeChanging(ElementTheme oldValue, ElementTheme newValue)
    {
        if (oldValue != newValue)
        {
            OnThemeAboutToChange?.Invoke(oldValue);
        }
    }

    partial void OnThemeChanged(ElementTheme value)
    {
        _settingsStore.Theme = value;
    }

    private void ScheduleDebouncedSave(Entry entry, string content)
    {
        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();
        var token = _saveCts.Token;
        _pendingSave = SaveAfterDelayAsync(entry, content, token);
    }

    private async Task SaveAfterDelayAsync(Entry entry, string content, CancellationToken token)
    {
        try
        {
            await Task.Delay(SaveDebounceMs, token).ConfigureAwait(false);
            try
            {
                await _entryStore.WriteAsync(entry, content, token).ConfigureAwait(false);
                OnClearPersistentToast?.Invoke();
            }
            catch when (!token.IsCancellationRequested)
            {
                await Task.Delay(500, token).ConfigureAwait(false);
                try
                {
                    await _entryStore.WriteAsync(entry, content, token).ConfigureAwait(false);
                    OnClearPersistentToast?.Invoke();
                }
                catch
                {
                    OnPersistentToast?.Invoke("Couldn't save — check disk space.");
                }
            }
            UpdatePreview(entry, content);
        }
        catch (OperationCanceledException) { /* expected */ }
    }

    public async Task FlushPendingSaveAsync()
    {
        if (_pendingSave is null && SelectedEntry is null) return;
        _saveCts?.Cancel();
        if (_pendingSave is not null)
        {
            try { await _pendingSave.ConfigureAwait(true); }
            catch { /* swallow — we're switching entries or shutting down */ }
        }

        if (SelectedEntry is { } current)
        {
            try { await _entryStore.WriteAsync(current, Text).ConfigureAwait(true); }
            catch { /* same — best effort flush */ }
            UpdatePreview(current, Text);
        }
    }

    [RelayCommand]
    private async Task NewEntryAsync(CancellationToken ct = default)
    {
        await FlushPendingSaveAsync().ConfigureAwait(true);
        var created = await _entryStore.CreateAsync(ct).ConfigureAwait(true);
        Entries.Insert(0, created);
        await SelectAsync(created, ct).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(Entry? entry, CancellationToken ct = default)
    {
        if (entry is null) return;
        var wasSelected = SelectedEntry?.Id == entry.Id;
        await _entryStore.DeleteAsync(entry, ct).ConfigureAwait(true);
        for (var i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Id == entry.Id) { Entries.RemoveAt(i); break; }
        }
        if (wasSelected)
        {
            if (Entries.Count == 0)
            {
                var created = await _entryStore.CreateAsync(ct).ConfigureAwait(true);
                Entries.Insert(0, created);
                await SelectAsync(created, ct).ConfigureAwait(true);
            }
            else
            {
                await SelectAsync(Entries[0], ct).ConfigureAwait(true);
            }
        }
    }

    private static readonly string[] FontCycle = { "Lato", "Newsreader", "JetBrains Mono" };
    private static readonly double[] SizeCycle = { 16, 18, 22, 26 };

    [RelayCommand]
    private void CycleFont()
    {
        var idx = Array.IndexOf(FontCycle, SelectedFont);
        SelectedFont = FontCycle[(idx + 1) % FontCycle.Length];
    }

    [RelayCommand]
    private void CycleSize()
    {
        var idx = Array.IndexOf(SizeCycle, FontSize);
        FontSize = SizeCycle[(idx + 1) % SizeCycle.Length];
    }

    [RelayCommand]
    private void ToggleTimer() => TimerIsRunning = !TimerIsRunning;

    [RelayCommand]
    private void ResetTimer()
    {
        TimerIsRunning = false;
        TimeRemaining = 900;
    }

    public void AdjustTimer(int deltaMinutes)
    {
        var newTotal = Math.Clamp(TimeRemaining + (deltaMinutes * 60), 5 * 60, 120 * 60);
        TimeRemaining = newTotal;
    }

    [RelayCommand]
    private void ToggleBackspace() => BackspaceDisabled = !BackspaceDisabled;

    [RelayCommand]
    private void ToggleTheme()
    {
        Theme = Theme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
    }

    [RelayCommand]
    private void ToggleSidebar() => IsSidebarOpen = !IsSidebarOpen;

    [RelayCommand]
    private void ToggleReviewMode() => IsReviewMode = !IsReviewMode;

    public Func<string, Task>? OnCopyChatPrompt { get; set; }
    public Func<Task>? OnExportPdf { get; set; }
    public Action<string>? OnPersistentToast { get; set; }
    public Action? OnClearPersistentToast { get; set; }
    public Func<string, Task>? OnRevealFolder { get; set; }
    public Action<ElementTheme>? OnThemeAboutToChange { get; set; }

    [RelayCommand]
    private async Task ShowEntriesFolderAsync()
    {
        if (OnRevealFolder is null) return;
        await OnRevealFolder(_entryStore.RootPath).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task CopyChatPromptAsync(string? target)
    {
        if (OnCopyChatPrompt is null || string.IsNullOrEmpty(target)) return;
        await OnCopyChatPrompt(target).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        if (OnExportPdf is null) return;
        await OnExportPdf().ConfigureAwait(true);
    }

    private void UpdatePreview(Entry entry, string content)
    {
        var preview = EntryStore.BuildPreview(content);
        var idx = -1;
        for (var i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].Id == entry.Id) { idx = i; break; }
        }
        if (idx < 0) return;
        Entries[idx] = entry with { Preview = preview };
        if (SelectedEntry?.Id == entry.Id)
        {
            _suppressSave = true;
            try { SelectedEntry = Entries[idx]; }
            finally { _suppressSave = false; }
        }
    }
}
