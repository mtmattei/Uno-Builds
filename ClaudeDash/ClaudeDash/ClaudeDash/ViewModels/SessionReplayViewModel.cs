using System.Collections.Immutable;
using ClaudeDash.Models.Timeline;
using ClaudeDash.Services;

namespace ClaudeDash.ViewModels;

public partial record SessionReplayModel
{
    private readonly ISessionParserService _parserService;
    private readonly ILogger<SessionReplayModel> _logger;
    private SessionTimeline? _timeline;
    private ImmutableList<TimelineEntry> _meaningfulEntries = ImmutableList<TimelineEntry>.Empty;
    private DispatcherTimer? _playbackTimer;

    public SessionReplayModel(
        ISessionParserService parserService,
        ILogger<SessionReplayModel> logger)
    {
        _parserService = parserService;
        _logger = logger;
    }

    // Session metadata
    public IState<string> SessionId => State.Value(this, () => string.Empty);
    public IState<bool> IsLoading => State.Value(this, () => false);
    public IState<string> LoadingStatus => State.Value(this, () => "Loading session...");
    public IState<bool> IsLoaded => State.Value(this, () => false);

    // Playback state
    public IState<int> CurrentPosition => State.Value(this, () => 0);
    public IState<int> TotalEntries => State.Value(this, () => 0);
    public IState<bool> IsPlaying => State.Value(this, () => false);
    public IState<double> PlaybackSpeed => State.Value(this, () => 1.0);
    public IState<string> PlaybackSpeedLabel => State.Value(this, () => "1x");

    // Session metadata displays
    public IState<string> ProjectName => State.Value(this, () => string.Empty);
    public IState<string> GitBranch => State.Value(this, () => string.Empty);
    public IState<string> ClaudeVersion => State.Value(this, () => string.Empty);
    public IState<string> DurationText => State.Value(this, () => string.Empty);
    public IState<string> StatsText => State.Value(this, () => string.Empty);
    public IState<int> UserMessageCount => State.Value(this, () => 0);
    public IState<int> ToolCallCount => State.Value(this, () => 0);
    public IState<int> FileChangeCount => State.Value(this, () => 0);
    public IState<string> ModelsUsed => State.Value(this, () => string.Empty);
    public IState<string> CostText => State.Value(this, () => string.Empty);
    public IState<string> PositionTimestamp => State.Value(this, () => string.Empty);

    // Selected entry for detail view
    public IState<TimelineEntry?> SelectedEntry => State<TimelineEntry?>.Value(this, () => null);

    // Timeline and file data
    public IListState<TimelineEntry> VisibleEntries => ListState.Value(this, () => ImmutableList<TimelineEntry>.Empty);
    public IListState<FileActivityRecord> ActiveFiles => ListState.Value(this, () => ImmutableList<FileActivityRecord>.Empty);

    public async ValueTask LoadSession(string sessionId, CancellationToken ct)
    {
        await SessionId.Set(sessionId, ct);
        await IsLoading.Set(true, ct);
        await IsLoaded.Set(false, ct);
        await LoadingStatus.Set("Parsing session timeline...", ct);

        try
        {
            _timeline = await _parserService.ParseSessionByIdAsync(sessionId);

            if (_timeline == null)
            {
                await LoadingStatus.Set("Session not found", ct);
                return;
            }

            // Set metadata
            await ProjectName.Set(System.IO.Path.GetFileName(_timeline.ProjectPath.TrimEnd('\\', '/')), ct);
            await GitBranch.Set(_timeline.GitBranch ?? "", ct);
            await ClaudeVersion.Set(_timeline.ClaudeVersion ?? "", ct);
            await UserMessageCount.Set(_timeline.UserMessageCount, ct);
            await ToolCallCount.Set(_timeline.ToolCallCount, ct);
            await FileChangeCount.Set(_timeline.FileChangeCount, ct);
            await ModelsUsed.Set(string.Join(", ", _timeline.ModelsUsed.Select(FormatModel)), ct);
            await CostText.Set($"${_timeline.EstimatedCost:F3}", ct);

            var dur = _timeline.Duration;
            var durText = dur.TotalHours >= 1
                ? $"{dur.TotalHours:F1}h"
                : dur.TotalMinutes >= 1
                    ? $"{dur.TotalMinutes:F0}m"
                    : $"{dur.TotalSeconds:F0}s";
            await DurationText.Set(durText, ct);

            // Filter to meaningful entries
            _meaningfulEntries = _timeline.Entries
                .Where(e => e.Type is TimelineEntryType.UserMessage
                    or TimelineEntryType.AssistantText
                    or TimelineEntryType.ToolCall
                    or TimelineEntryType.ToolResult
                    or TimelineEntryType.FileChange)
                .ToImmutableList();

            await TotalEntries.Set(_meaningfulEntries.Count, ct);
            await CurrentPosition.Set(0, ct);

            // Show all entries initially
            await VisibleEntries.UpdateAsync(_ => _meaningfulEntries, ct);

            // Build file list
            await RefreshFileActivity(_meaningfulEntries.Count, ct);

            await StatsText.Set(
                $"{_timeline.UserMessageCount} prompts  {_timeline.ToolCallCount} tools  {_timeline.FileChangeCount} edits  {_timeline.FilesTouched.Count} files",
                ct);

            await IsLoaded.Set(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load session {SessionId}", sessionId);
            await LoadingStatus.Set($"Error: {ex.Message}", ct);
        }
        finally
        {
            await IsLoading.Set(false, ct);
        }
    }

    public async ValueTask SeekToPosition(int value, CancellationToken ct)
    {
        if (_timeline == null || !(await IsLoaded)) return;

        await CurrentPosition.Set(value, ct);

        var count = Math.Min(value, _meaningfulEntries.Count);
        await VisibleEntries.UpdateAsync(_ => _meaningfulEntries.Take(count).ToImmutableList(), ct);

        // Update position timestamp
        if (count > 0 && count <= _meaningfulEntries.Count)
        {
            var entry = _meaningfulEntries[count - 1];
            if (entry.Timestamp != default)
                await PositionTimestamp.Set(entry.Timestamp.ToString("HH:mm:ss"), ct);
        }

        await RefreshFileActivity(count, ct);
    }

    private async Task RefreshFileActivity(int upToPosition, CancellationToken ct)
    {
        if (_timeline == null) return;

        var fileStats = new Dictionary<string, FileActivityRecord>(StringComparer.OrdinalIgnoreCase);
        var recentThreshold = Math.Max(0, upToPosition - 3);

        for (var i = 0; i < Math.Min(upToPosition, _meaningfulEntries.Count); i++)
        {
            var entry = _meaningfulEntries[i];
            if (entry.FilePath == null) continue;

            var fileName = System.IO.Path.GetFileName(entry.FilePath);
            if (!fileStats.TryGetValue(entry.FilePath, out var activity))
            {
                activity = new FileActivityRecord(
                    FullPath: entry.FilePath,
                    FileName: fileName,
                    Directory: System.IO.Path.GetDirectoryName(entry.FilePath) ?? "",
                    EditCount: 0,
                    LastEditIndex: 0,
                    IsHot: false);
                fileStats[entry.FilePath] = activity;
            }

            fileStats[entry.FilePath] = activity with
            {
                EditCount = activity.EditCount + 1,
                LastEditIndex = i,
                IsHot = i >= recentThreshold
            };
        }

        var sortedFiles = fileStats.Values
            .OrderByDescending(f => f.LastEditIndex)
            .ToImmutableList();

        await ActiveFiles.UpdateAsync(_ => sortedFiles, ct);
    }

    public async ValueTask TogglePlayback(CancellationToken ct)
    {
        if (await IsPlaying)
            await Pause(ct);
        else
            await Play(ct);
    }

    public async ValueTask Play(CancellationToken ct)
    {
        if (_timeline == null) return;
        var pos = await CurrentPosition;
        var total = await TotalEntries;
        if (pos >= total) return;

        await IsPlaying.Set(true, ct);
        var speed = await PlaybackSpeed;
        _playbackTimer ??= new DispatcherTimer();
        _playbackTimer.Interval = TimeSpan.FromMilliseconds(800 / speed);
        _playbackTimer.Tick -= PlaybackTick;
        _playbackTimer.Tick += PlaybackTick;
        _playbackTimer.Start();
    }

    public async ValueTask Pause(CancellationToken ct)
    {
        await IsPlaying.Set(false, ct);
        _playbackTimer?.Stop();
    }

    public async ValueTask StepForward(CancellationToken ct)
    {
        var pos = await CurrentPosition;
        var total = await TotalEntries;
        if (pos < total)
            await SeekToPosition(pos + 1, ct);
    }

    public async ValueTask StepBack(CancellationToken ct)
    {
        var pos = await CurrentPosition;
        if (pos > 0)
            await SeekToPosition(pos - 1, ct);
    }

    public async ValueTask JumpToStart(CancellationToken ct)
    {
        await Pause(ct);
        await SeekToPosition(0, ct);
    }

    public async ValueTask JumpToEnd(CancellationToken ct)
    {
        await Pause(ct);
        var total = await TotalEntries;
        await SeekToPosition(total, ct);
    }

    public async ValueTask CycleSpeed(CancellationToken ct)
    {
        var speed = await PlaybackSpeed;
        var newSpeed = speed switch
        {
            1.0 => 2.0,
            2.0 => 4.0,
            4.0 => 0.5,
            _ => 1.0
        };

        await PlaybackSpeed.Set(newSpeed, ct);

        var label = newSpeed switch
        {
            0.5 => "0.5x",
            1.0 => "1x",
            2.0 => "2x",
            4.0 => "4x",
            _ => "1x"
        };
        await PlaybackSpeedLabel.Set(label, ct);

        // Update timer interval if playing
        if (await IsPlaying && _playbackTimer != null)
        {
            _playbackTimer.Interval = TimeSpan.FromMilliseconds(800 / newSpeed);
        }
    }

    private async void PlaybackTick(object? sender, object e)
    {
        try
        {
            var pos = await CurrentPosition;
            var total = await TotalEntries;
            if (pos >= total)
            {
                await Pause(CancellationToken.None);
                return;
            }
            await SeekToPosition(pos + 1, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Playback tick error");
        }
    }

    private static string FormatModel(string model)
    {
        if (model.Contains("opus", StringComparison.OrdinalIgnoreCase)) return "opus";
        if (model.Contains("sonnet", StringComparison.OrdinalIgnoreCase)) return "sonnet";
        if (model.Contains("haiku", StringComparison.OrdinalIgnoreCase)) return "haiku";
        return model;
    }
}

/// <summary>
/// Immutable record replacing the mutable FileActivity class for MVUX compatibility.
/// </summary>
public record FileActivityRecord(
    string FullPath = "",
    string FileName = "",
    string Directory = "",
    int EditCount = 0,
    int LastEditIndex = 0,
    bool IsHot = false);
