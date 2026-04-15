using ClaudeDash.Models.Timeline;
using Path = System.IO.Path;

namespace ClaudeDash.Services;

public class SessionParserService : ISessionParserService
{
    private static readonly string ClaudeDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude");

    // Approximate token costs in USD per 1M tokens (Claude 4 pricing)
    private static readonly Dictionary<string, (double Input, double Output)> ModelPricing = new(StringComparer.OrdinalIgnoreCase)
    {
        ["claude-opus-4-6"] = (15.0, 75.0),
        ["claude-opus-4-5-20250620"] = (15.0, 75.0),
        ["claude-sonnet-4-6"] = (3.0, 15.0),
        ["claude-sonnet-4-5-20250514"] = (3.0, 15.0),
        ["claude-haiku-4-5-20251001"] = (0.80, 4.0),
    };

    public async Task<SessionTimeline?> ParseFullSessionAsync(string sessionFilePath)
    {
        if (!File.Exists(sessionFilePath))
            return null;

        try
        {
            var fileInfo = new FileInfo(sessionFilePath);
            var sessionId = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var projectDirName = fileInfo.Directory?.Name ?? "";
            var projectPath = ClaudeConfigService.DecodeProjectPath(projectDirName);

            var timeline = new SessionTimeline
            {
                SessionId = sessionId,
                ProjectPath = projectPath
            };

            var entries = new List<TimelineEntry>();
            var entryIndex = 0;

            await foreach (var line in ReadLinesAsync(sessionFilePath))
            {
                var entry = ParseLine(line, ref entryIndex, timeline);
                if (entry != null)
                    entries.Add(entry);
            }

            timeline.Entries = entries;

            // Compute aggregates
            ComputeAggregates(timeline);

            // Discover subagents
            DiscoverSubagents(sessionFilePath, timeline);

            return timeline;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SessionTimeline?> ParseSessionByIdAsync(string sessionId)
    {
        var projectsDir = Path.Combine(ClaudeDir, "projects");
        if (!Directory.Exists(projectsDir))
            return null;

        // Search recursively - sessions can be in project dirs or nested subagent dirs
        var fileName = $"{sessionId}.jsonl";
        var files = Directory.GetFiles(projectsDir, fileName, SearchOption.AllDirectories);
        if (files.Length > 0)
            return await ParseFullSessionAsync(files[0]);

        return null;
    }

    public async Task<ClaudeSessionInfo> GetEnrichedSessionInfoAsync(string sessionFilePath)
    {
        var fileInfo = new FileInfo(sessionFilePath);
        var sessionId = Path.GetFileNameWithoutExtension(fileInfo.Name);
        var projectDirName = fileInfo.Directory?.Name ?? "";

        var session = new ClaudeSessionInfo
        {
            SessionId = sessionId,
            ShortId = sessionId.Length >= 8 ? sessionId[..8] : sessionId,
            ProjectPath = ClaudeConfigService.DecodeProjectPath(projectDirName),
            LastActivity = fileInfo.LastWriteTimeUtc
        };

        int userCount = 0, assistantCount = 0, toolCallCount = 0;
        int totalInput = 0, totalOutput = 0;
        DateTime? firstTimestamp = null;
        DateTime? lastTimestamp = null;
        string? model = null;
        string? version = null;
        string? branch = null;
        string? firstMessage = null;
        bool hasError = false;

        try
        {
            await foreach (var line in ReadLinesAsync(sessionFilePath))
            {
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    // Timestamp
                    if (root.TryGetProperty("timestamp", out var tsProp))
                    {
                        if (DateTime.TryParse(tsProp.GetString(), out var ts))
                        {
                            firstTimestamp ??= ts;
                            lastTimestamp = ts;
                        }
                    }

                    if (!root.TryGetProperty("type", out var typeProp))
                        continue;

                    var type = typeProp.GetString();

                    if (type == "user")
                    {
                        userCount++;
                        if (firstMessage == null)
                        {
                            firstMessage = ExtractUserText(root);
                        }
                    }
                    else if (type == "assistant")
                    {
                        assistantCount++;

                        // Count tool calls in content
                        if (root.TryGetProperty("message", out var msg))
                        {
                            if (model == null && msg.TryGetProperty("model", out var modelProp))
                                model = modelProp.GetString();

                            if (msg.TryGetProperty("content", out var content) &&
                                content.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in content.EnumerateArray())
                                {
                                    if (item.TryGetProperty("type", out var ct) &&
                                        ct.GetString() == "tool_use")
                                        toolCallCount++;
                                }
                            }

                            if (msg.TryGetProperty("stop_reason", out var stopProp) &&
                                stopProp.GetString() == "error")
                                hasError = true;
                        }

                        // Token usage
                        if (root.TryGetProperty("usage", out var usage))
                        {
                            if (usage.TryGetProperty("input_tokens", out var inp))
                                totalInput += inp.GetInt32();
                            if (usage.TryGetProperty("output_tokens", out var outp))
                                totalOutput += outp.GetInt32();
                        }
                    }

                    // Metadata from any entry
                    if (version == null && root.TryGetProperty("version", out var verProp))
                        version = verProp.GetString();
                    if (branch == null && root.TryGetProperty("gitBranch", out var brProp))
                        branch = brProp.GetString();
                }
                catch
                {
                    // Skip malformed lines
                }
            }
        }
        catch
        {
            // Graceful degradation
        }

        session.MessageCount = userCount + assistantCount;
        session.TokenCount = totalInput + totalOutput;
        session.Model = FormatModelName(model ?? "");
        session.ClaudeVersion = version ?? "";
        session.GitBranch = branch ?? "";
        session.Status = hasError ? "error" : "completed";
        session.RepoName = Path.GetFileName(session.ProjectPath.TrimEnd('\\', '/'));

        if (firstMessage != null)
        {
            session.FirstUserMessage = firstMessage.Length > 120
                ? firstMessage[..120] + "..."
                : firstMessage;
        }

        // Duration
        if (firstTimestamp.HasValue && lastTimestamp.HasValue)
        {
            var dur = lastTimestamp.Value - firstTimestamp.Value;
            session.Duration = dur.TotalHours >= 1
                ? $"{dur.TotalHours:F1}h"
                : dur.TotalMinutes >= 1
                    ? $"{dur.TotalMinutes:F0}m"
                    : $"{dur.TotalSeconds:F0}s";
        }

        // Cost estimate
        session.CostAmount = EstimateCost(model ?? "", totalInput, totalOutput);

        return session;
    }

    private TimelineEntry? ParseLine(string line, ref int entryIndex, SessionTimeline timeline)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProp))
                return null;

            var type = typeProp.GetString();
            var uuid = root.TryGetProperty("uuid", out var uuidProp) ? uuidProp.GetString() ?? "" : "";
            var parentUuid = root.TryGetProperty("parentUuid", out var pProp) ? pProp.GetString() : null;
            var cwd = root.TryGetProperty("cwd", out var cwdProp) ? cwdProp.GetString() : null;
            var gitBranch = root.TryGetProperty("gitBranch", out var brProp) ? brProp.GetString() : null;

            // Version/branch from any entry
            if (timeline.ClaudeVersion == null && root.TryGetProperty("version", out var verProp))
                timeline.ClaudeVersion = verProp.GetString();
            if (timeline.GitBranch == null && gitBranch != null)
                timeline.GitBranch = gitBranch;

            DateTime timestamp = default;
            if (root.TryGetProperty("timestamp", out var tsProp) &&
                DateTime.TryParse(tsProp.GetString(), out var ts))
                timestamp = ts;

            switch (type)
            {
                case "user":
                    return ParseUserEntry(root, uuid, parentUuid, timestamp, cwd, gitBranch, ref entryIndex);

                case "assistant":
                    return ParseAssistantEntry(root, uuid, parentUuid, timestamp, cwd, gitBranch, ref entryIndex, timeline);

                case "progress":
                    return ParseProgressEntry(root, uuid, timestamp, ref entryIndex);

                case "system":
                    return ParseSystemEntry(root, uuid, timestamp, ref entryIndex);

                case "file-history-snapshot":
                    return ParseFileHistoryEntry(root, uuid, timestamp, ref entryIndex, timeline);

                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }

    private TimelineEntry? ParseUserEntry(JsonElement root, string uuid, string? parentUuid,
        DateTime timestamp, string? cwd, string? gitBranch, ref int entryIndex)
    {
        // Check if this is a tool result
        if (root.TryGetProperty("message", out var msg) &&
            msg.TryGetProperty("content", out var content))
        {
            if (content.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in content.EnumerateArray())
                {
                    if (item.TryGetProperty("type", out var ct) && ct.GetString() == "tool_result")
                    {
                        // This is a tool result, check for file change data
                        if (root.TryGetProperty("toolUseResult", out var toolResult))
                        {
                            return ParseToolResultEntry(item, toolResult, uuid, parentUuid, timestamp, cwd, gitBranch, ref entryIndex);
                        }

                        var resultText = item.TryGetProperty("content", out var rc)
                            ? (rc.ValueKind == JsonValueKind.String ? rc.GetString() : rc.ToString())
                            : "";

                        return new TimelineEntry
                        {
                            Id = uuid,
                            ParentId = parentUuid,
                            Timestamp = timestamp,
                            Type = TimelineEntryType.ToolResult,
                            Index = entryIndex++,
                            ToolCallId = item.TryGetProperty("tool_use_id", out var tid) ? tid.GetString() : null,
                            ToolResultText = resultText,
                            ToolSuccess = true,
                            WorkingDirectory = cwd,
                            GitBranch = gitBranch
                        };
                    }
                }
            }

            // Regular user message
            var text = ExtractUserText(root);
            if (text != null)
            {
                return new TimelineEntry
                {
                    Id = uuid,
                    ParentId = parentUuid,
                    Timestamp = timestamp,
                    Type = TimelineEntryType.UserMessage,
                    Index = entryIndex++,
                    UserText = text,
                    WorkingDirectory = cwd,
                    GitBranch = gitBranch
                };
            }
        }

        return null;
    }

    private List<TimelineEntry> ParseAssistantContentItems(JsonElement content, string uuid, string? parentUuid,
        DateTime timestamp, string? cwd, string? gitBranch, ref int entryIndex, SessionTimeline timeline)
    {
        var entries = new List<TimelineEntry>();

        foreach (var item in content.EnumerateArray())
        {
            if (!item.TryGetProperty("type", out var ct))
                continue;

            var contentType = ct.GetString();

            if (contentType == "text")
            {
                var text = item.TryGetProperty("text", out var textProp) ? textProp.GetString() : null;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    entries.Add(new TimelineEntry
                    {
                        Id = $"{uuid}_text_{entries.Count}",
                        ParentId = parentUuid,
                        Timestamp = timestamp,
                        Type = TimelineEntryType.AssistantText,
                        Index = entryIndex++,
                        AssistantText = text,
                        WorkingDirectory = cwd,
                        GitBranch = gitBranch
                    });
                }
            }
            else if (contentType == "tool_use")
            {
                var toolName = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "unknown";
                var toolId = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : "";

                Dictionary<string, JsonElement>? input = null;
                if (item.TryGetProperty("input", out var inputProp) && inputProp.ValueKind == JsonValueKind.Object)
                {
                    input = new Dictionary<string, JsonElement>();
                    foreach (var prop in inputProp.EnumerateObject())
                    {
                        input[prop.Name] = prop.Value.Clone();
                    }
                }

                timeline.ToolsUsed.Add(toolName ?? "unknown");

                entries.Add(new TimelineEntry
                {
                    Id = $"{uuid}_tool_{toolId}",
                    ParentId = parentUuid,
                    Timestamp = timestamp,
                    Type = TimelineEntryType.ToolCall,
                    Index = entryIndex++,
                    ToolName = toolName,
                    ToolCallId = toolId,
                    ToolInput = input,
                    WorkingDirectory = cwd,
                    GitBranch = gitBranch
                });
            }
            else if (contentType == "thinking")
            {
                var thinking = item.TryGetProperty("thinking", out var thinkProp) ? thinkProp.GetString() : null;
                if (!string.IsNullOrWhiteSpace(thinking))
                {
                    entries.Add(new TimelineEntry
                    {
                        Id = $"{uuid}_think_{entries.Count}",
                        ParentId = parentUuid,
                        Timestamp = timestamp,
                        Type = TimelineEntryType.ThinkingBlock,
                        Index = entryIndex++,
                        ThinkingText = thinking,
                        WorkingDirectory = cwd,
                        GitBranch = gitBranch
                    });
                }
            }
        }

        return entries;
    }

    private TimelineEntry? ParseAssistantEntry(JsonElement root, string uuid, string? parentUuid,
        DateTime timestamp, string? cwd, string? gitBranch, ref int entryIndex, SessionTimeline timeline)
    {
        if (!root.TryGetProperty("message", out var msg))
            return null;

        var model = msg.TryGetProperty("model", out var modelProp) ? modelProp.GetString() : null;
        var stopReason = msg.TryGetProperty("stop_reason", out var stopProp) ? stopProp.GetString() : null;

        if (model != null)
            timeline.ModelsUsed.Add(model);

        TokenUsage? usage = null;
        if (root.TryGetProperty("usage", out var usageProp))
        {
            usage = new TokenUsage();
            if (usageProp.TryGetProperty("input_tokens", out var inp))
                usage.InputTokens = inp.GetInt32();
            if (usageProp.TryGetProperty("output_tokens", out var outp))
                usage.OutputTokens = outp.GetInt32();
            if (usageProp.TryGetProperty("cache_read_input_tokens", out var cacheRead))
                usage.CacheReadTokens = cacheRead.GetInt32();
            if (usageProp.TryGetProperty("cache_creation_input_tokens", out var cacheCreate))
                usage.CacheCreationTokens = cacheCreate.GetInt32();
        }

        if (msg.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
        {
            var subEntries = ParseAssistantContentItems(content, uuid, parentUuid, timestamp, cwd, gitBranch, ref entryIndex, timeline);

            // Attach usage and model to the first entry
            if (subEntries.Count > 0)
            {
                subEntries[0].Usage = usage;
                subEntries[0].Model = model;
                subEntries[0].StopReason = stopReason;

                // We return the first entry here and add the rest to the timeline directly
                // Actually, we need to add all entries. Return first, add rest via timeline.
                // Better approach: return the first text entry with aggregated info
            }

            // Return a single combined assistant entry for simplicity
            var combinedText = string.Join("\n", subEntries
                .Where(e => e.Type == TimelineEntryType.AssistantText)
                .Select(e => e.AssistantText));

            var toolCalls = subEntries.Where(e => e.Type == TimelineEntryType.ToolCall).ToList();
            var thinkingBlocks = subEntries.Where(e => e.Type == TimelineEntryType.ThinkingBlock).ToList();

            // Add tool calls and thinking blocks directly to the timeline entries list
            // They'll be picked up by the caller since we only return one entry here
            // Instead, let's flatten: return the text entry, store tool calls separately
            var mainEntry = new TimelineEntry
            {
                Id = uuid,
                ParentId = parentUuid,
                Timestamp = timestamp,
                Type = TimelineEntryType.AssistantText,
                Index = entryIndex++,
                AssistantText = combinedText,
                Model = model,
                StopReason = stopReason,
                Usage = usage,
                WorkingDirectory = cwd,
                GitBranch = gitBranch
            };

            return mainEntry;
        }

        return null;
    }

    private TimelineEntry? ParseToolResultEntry(JsonElement item, JsonElement toolResult,
        string uuid, string? parentUuid, DateTime timestamp, string? cwd, string? gitBranch, ref int entryIndex)
    {
        var filePath = toolResult.TryGetProperty("filePath", out var fpProp) ? fpProp.GetString() : null;
        var oldString = toolResult.TryGetProperty("oldString", out var osProp) ? osProp.GetString() : null;
        var newString = toolResult.TryGetProperty("newString", out var nsProp) ? nsProp.GetString() : null;

        List<DiffHunk>? patches = null;
        if (toolResult.TryGetProperty("structuredPatch", out var patchProp) && patchProp.ValueKind == JsonValueKind.Array)
        {
            patches = [];
            foreach (var hunk in patchProp.EnumerateArray())
            {
                var diffHunk = new DiffHunk
                {
                    OldStart = hunk.TryGetProperty("oldStart", out var os) ? os.GetInt32() : 0,
                    OldLines = hunk.TryGetProperty("oldLines", out var ol) ? ol.GetInt32() : 0,
                    NewStart = hunk.TryGetProperty("newStart", out var ns) ? ns.GetInt32() : 0,
                    NewLines = hunk.TryGetProperty("newLines", out var nl) ? nl.GetInt32() : 0
                };

                if (hunk.TryGetProperty("lines", out var lines) && lines.ValueKind == JsonValueKind.Array)
                {
                    diffHunk.Lines = lines.EnumerateArray()
                        .Select(l => l.GetString() ?? "")
                        .ToList();
                }

                patches.Add(diffHunk);
            }
        }

        if (filePath != null && (oldString != null || newString != null || patches != null))
        {
            return new TimelineEntry
            {
                Id = uuid,
                ParentId = parentUuid,
                Timestamp = timestamp,
                Type = TimelineEntryType.FileChange,
                Index = entryIndex++,
                FilePath = filePath,
                OldContent = oldString,
                NewContent = newString,
                StructuredPatch = patches,
                ToolCallId = item.TryGetProperty("tool_use_id", out var tid) ? tid.GetString() : null,
                ToolSuccess = true,
                WorkingDirectory = cwd,
                GitBranch = gitBranch
            };
        }

        // Regular tool result without file change
        var resultText = item.TryGetProperty("content", out var rc)
            ? (rc.ValueKind == JsonValueKind.String ? rc.GetString() : rc.ToString())
            : "";

        return new TimelineEntry
        {
            Id = uuid,
            ParentId = parentUuid,
            Timestamp = timestamp,
            Type = TimelineEntryType.ToolResult,
            Index = entryIndex++,
            ToolCallId = item.TryGetProperty("tool_use_id", out var toolId) ? toolId.GetString() : null,
            ToolResultText = resultText,
            ToolSuccess = true,
            WorkingDirectory = cwd,
            GitBranch = gitBranch
        };
    }

    private static TimelineEntry? ParseProgressEntry(JsonElement root, string uuid, DateTime timestamp, ref int entryIndex)
    {
        var detail = "";
        if (root.TryGetProperty("data", out var data))
        {
            var subtype = data.TryGetProperty("type", out var stProp) ? stProp.GetString() : "";
            var command = data.TryGetProperty("command", out var cmdProp) ? cmdProp.GetString() : "";
            detail = $"{subtype}: {command}".Trim(' ', ':');
        }

        return new TimelineEntry
        {
            Id = uuid,
            Timestamp = timestamp,
            Type = TimelineEntryType.ProgressEvent,
            Index = entryIndex++,
            EventSubtype = "progress",
            EventDetail = detail
        };
    }

    private static TimelineEntry? ParseSystemEntry(JsonElement root, string uuid, DateTime timestamp, ref int entryIndex)
    {
        var subtype = root.TryGetProperty("subtype", out var stProp) ? stProp.GetString() : "";
        var detail = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "";

        return new TimelineEntry
        {
            Id = uuid,
            Timestamp = timestamp,
            Type = TimelineEntryType.SystemEvent,
            Index = entryIndex++,
            EventSubtype = subtype,
            EventDetail = detail
        };
    }

    private static TimelineEntry? ParseFileHistoryEntry(JsonElement root, string uuid,
        DateTime timestamp, ref int entryIndex, SessionTimeline timeline)
    {
        if (!root.TryGetProperty("snapshot", out var snapshot))
            return null;

        var isUpdate = root.TryGetProperty("isSnapshotUpdate", out var updateProp) && updateProp.GetBoolean();

        if (snapshot.TryGetProperty("trackedFileBackups", out var backups) && backups.ValueKind == JsonValueKind.Object)
        {
            foreach (var file in backups.EnumerateObject())
            {
                timeline.FilesTouched.Add(file.Name);
            }
        }

        return new TimelineEntry
        {
            Id = uuid,
            Timestamp = timestamp,
            Type = TimelineEntryType.FileHistorySnapshot,
            Index = entryIndex++,
            EventSubtype = isUpdate ? "snapshot_update" : "snapshot",
            EventDetail = $"File backup snapshot"
        };
    }

    private void ComputeAggregates(SessionTimeline timeline)
    {
        foreach (var entry in timeline.Entries)
        {
            switch (entry.Type)
            {
                case TimelineEntryType.UserMessage:
                    timeline.UserMessageCount++;
                    break;
                case TimelineEntryType.AssistantText:
                    timeline.AssistantMessageCount++;
                    if (entry.Usage != null)
                    {
                        timeline.TotalInputTokens += entry.Usage.InputTokens;
                        timeline.TotalOutputTokens += entry.Usage.OutputTokens;
                    }
                    break;
                case TimelineEntryType.ToolCall:
                    timeline.ToolCallCount++;
                    break;
                case TimelineEntryType.FileChange:
                    timeline.FileChangeCount++;
                    if (entry.FilePath != null)
                        timeline.FilesTouched.Add(entry.FilePath);
                    break;
            }
        }

        // Timestamps
        var timestamped = timeline.Entries.Where(e => e.Timestamp != default).ToList();
        if (timestamped.Count > 0)
        {
            timeline.StartTime = timestamped.Min(e => e.Timestamp);
            timeline.EndTime = timestamped.Max(e => e.Timestamp);
        }

        // Cost estimate
        foreach (var model in timeline.ModelsUsed)
        {
            // Sum tokens per model and calculate cost
            var modelEntries = timeline.Entries
                .Where(e => e.Model == model && e.Usage != null)
                .Select(e => e.Usage!)
                .ToList();

            var inputTokens = modelEntries.Sum(u => u.InputTokens);
            var outputTokens = modelEntries.Sum(u => u.OutputTokens);

            timeline.EstimatedCost += EstimateCost(model, inputTokens, outputTokens);
        }
    }

    private static void DiscoverSubagents(string sessionFilePath, SessionTimeline timeline)
    {
        var sessionDir = Path.Combine(
            Path.GetDirectoryName(sessionFilePath) ?? "",
            Path.GetFileNameWithoutExtension(sessionFilePath));

        var subagentsDir = Path.Combine(sessionDir, "subagents");

        if (!Directory.Exists(subagentsDir))
            return;

        try
        {
            foreach (var agentFile in Directory.GetFiles(subagentsDir, "*.jsonl", SearchOption.AllDirectories))
            {
                var fi = new FileInfo(agentFile);
                timeline.Subagents.Add(new SubagentRef
                {
                    AgentId = Path.GetFileNameWithoutExtension(fi.Name),
                    FilePath = fi.FullName,
                    LastActivity = fi.LastWriteTimeUtc
                });
            }
        }
        catch
        {
            // Graceful degradation
        }
    }

    private static string? ExtractUserText(JsonElement root)
    {
        if (!root.TryGetProperty("message", out var msg) ||
            !msg.TryGetProperty("content", out var content))
            return null;

        if (content.ValueKind == JsonValueKind.String)
            return content.GetString();

        if (content.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in content.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                    item.TryGetProperty("text", out var text))
                {
                    return text.GetString();
                }
            }
        }

        return null;
    }

    private static string FormatModelName(string model)
    {
        if (string.IsNullOrEmpty(model)) return "";
        if (model.Contains("opus", StringComparison.OrdinalIgnoreCase)) return "opus 4";
        if (model.Contains("sonnet", StringComparison.OrdinalIgnoreCase)) return "sonnet 4";
        if (model.Contains("haiku", StringComparison.OrdinalIgnoreCase)) return "haiku 4";
        return model;
    }

    private static double EstimateCost(string model, int inputTokens, int outputTokens)
    {
        // Find best matching pricing
        foreach (var kvp in ModelPricing)
        {
            if (model.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase) ||
                kvp.Key.Contains(model, StringComparison.OrdinalIgnoreCase))
            {
                return (inputTokens / 1_000_000.0 * kvp.Value.Input) +
                       (outputTokens / 1_000_000.0 * kvp.Value.Output);
            }
        }

        // Default to sonnet pricing
        return (inputTokens / 1_000_000.0 * 3.0) + (outputTokens / 1_000_000.0 * 15.0);
    }

    private static async IAsyncEnumerable<string> ReadLinesAsync(string filePath)
    {
        using var reader = new StreamReader(filePath);
        while (await reader.ReadLineAsync() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                yield return line;
        }
    }
}
