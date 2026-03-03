using System.Collections.ObjectModel;
using AgentNotifier.Models;
using AgentNotifier.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace AgentNotifier.ViewModels;

public partial class AgentViewModel : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _agentId = "AGT-001";
    [ObservableProperty] private string _model = string.Empty;
    [ObservableProperty] private AgentStatus _status = AgentStatus.Idle;
    [ObservableProperty] private string _statusLabel = "IDLE";
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private string _currentTask = string.Empty;
    [ObservableProperty] private int _progress;
    [ObservableProperty] private bool _hasProgress;
    [ObservableProperty] private int _tokensUsed;
    [ObservableProperty] private string _tokensDisplay = "0";
    [ObservableProperty] private double _cost;
    [ObservableProperty] private string _costDisplay = "$0.00";
    [ObservableProperty] private double _rate;
    [ObservableProperty] private string _rateDisplay = "0 t/s";
    [ObservableProperty] private int _queuePosition;
    [ObservableProperty] private string _queueDisplay = "#001";
    [ObservableProperty] private bool _isWaitingForInput;
    [ObservableProperty] private string _elapsedTime = "00:00";
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isBlinkVisible = true;
    [ObservableProperty] private bool _isWorking;

    private string _dataHash = "";
    public DateTime LastDataChange { get; private set; } = DateTime.UtcNow;

    public void UpdateFrom(AgentInfo info)
    {
        // Track if actual data changed for staleness detection
        var hash = $"{info.Status}|{info.TokensUsed}|{info.Progress}|{info.Session?.ElapsedMs}";
        if (hash != _dataHash)
        {
            _dataHash = hash;
            LastDataChange = DateTime.UtcNow;
        }

        Id = info.Id;
        Name = info.Name;
        AgentId = FormatAgentId(info.Id);
        Model = info.Model;
        Status = info.Status;
        StatusLabel = GetStatusLabel(info.Status);
        IsWorking = info.Status == AgentStatus.Working;
        Message = info.Message;
        CurrentTask = info.CurrentTask;
        HasProgress = info.Progress.HasValue;
        Progress = info.Progress ?? 0;
        TokensUsed = info.TokensUsed;
        TokensDisplay = FormatTokenCount(info.TokensUsed);
        Cost = info.Cost;
        CostDisplay = FormatCost(info.Cost);
        Rate = info.Rate;
        RateDisplay = $"{info.Rate:F0} t/s";
        QueuePosition = info.QueuePosition;
        QueueDisplay = $"#{info.QueuePosition:D3}";
        IsWaitingForInput = info.IsWaitingForInput;

        if (info.Session != null)
        {
            var elapsed = TimeSpan.FromMilliseconds(info.Session.ElapsedMs);
            ElapsedTime = elapsed.TotalHours >= 1
                ? elapsed.ToString(@"hh\:mm")
                : elapsed.ToString(@"mm\:ss");
        }
    }

    private static string FormatAgentId(string id)
    {
        if (string.IsNullOrEmpty(id)) return "AGT-???";
        var shortId = id.Length > 3 ? id[^3..] : id;
        return $"AGT-{shortId.ToUpper()}";
    }

    private static string GetStatusLabel(AgentStatus status) => status switch
    {
        AgentStatus.Working => "PROC",
        AgentStatus.Waiting => "WAIT",
        AgentStatus.Finished => "DONE",
        AgentStatus.Error => "ERR!",
        _ => "IDLE"
    };

    private static string FormatTokenCount(int tokens)
    {
        if (tokens >= 1000000) return $"{tokens / 1000000.0:F1}M";
        if (tokens >= 1000) return $"{tokens / 1000.0:F1}K";
        return tokens.ToString("N0");
    }

    private static string FormatCost(double cost)
    {
        if (cost >= 1) return $"${cost:F2}";
        if (cost >= 0.01) return $"${cost:F3}";
        return $"${cost:F4}";
    }

    [RelayCommand]
    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IAgentStatusService _statusService;
    private readonly IAudioService _audioService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly DispatcherQueueTimer _clockTimer;
    private readonly DispatcherQueueTimer _blinkTimer;
    private readonly DispatcherQueueTimer _staleTimer;
    private bool _disposed;
    private readonly Dictionary<string, AgentStatus> _previousStatuses = new();

    // Summary Stats
    [ObservableProperty] private int _totalAgents;
    [ObservableProperty] private int _awaitingInputCount;
    [ObservableProperty] private string _totalTokensDisplay = "0";
    [ObservableProperty] private string _totalElapsedDisplay = "00:00";
    [ObservableProperty] private string _totalCostDisplay = "$0.00";
    [ObservableProperty] private bool _audioEnabled = true;
    [ObservableProperty] private bool _isLive = true;

    public ObservableCollection<AgentViewModel> Agents { get; } = new();

    public MainViewModel(
        IAgentStatusService statusService,
        IAudioService audioService,
        ILogger<MainViewModel> logger)
    {
        _statusService = statusService;
        _audioService = audioService;
        _logger = logger;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        _clockTimer = _dispatcherQueue.CreateTimer();
        _clockTimer.Interval = TimeSpan.FromSeconds(1);
        _clockTimer.Tick += (s, e) => UpdateElapsedTimes();
        _clockTimer.Start();

        _blinkTimer = _dispatcherQueue.CreateTimer();
        _blinkTimer.Interval = TimeSpan.FromMilliseconds(750);
        _blinkTimer.Tick += OnBlinkTick;

        _staleTimer = _dispatcherQueue.CreateTimer();
        _staleTimer.Interval = TimeSpan.FromSeconds(10);
        _staleTimer.Tick += (s, e) => RemoveStaleAgents();
        _staleTimer.Start();

        _statusService.AgentsChanged += OnAgentsChanged;
        _statusService.Start();
    }

    private void UpdateElapsedTimes()
    {
        // Update live indicator blink
        IsLive = !IsLive || Agents.Any(a => a.Status == AgentStatus.Working);
    }

    private void OnBlinkTick(DispatcherQueueTimer sender, object args)
    {
        foreach (var agent in Agents)
        {
            if (agent.Status == AgentStatus.Working || agent.IsWaitingForInput)
            {
                agent.IsBlinkVisible = !agent.IsBlinkVisible;
            }
        }
    }

    private void OnAgentsChanged(object? sender, MultiAgentPayload payload)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdateAgents(payload.Agents);
            UpdateSummary(payload);
            CheckForAudioNotifications(payload.Agents);
            UpdateBlinkTimer();
        });
    }

    private void UpdateAgents(List<AgentInfo> agentInfos)
    {
        var existingIds = Agents.Select(a => a.Id).ToHashSet();
        var newIds = agentInfos.Select(a => a.Id).ToHashSet();

        var toRemove = Agents.Where(a => !newIds.Contains(a.Id)).ToList();
        foreach (var agent in toRemove)
        {
            Agents.Remove(agent);
        }

        foreach (var info in agentInfos)
        {
            var existing = Agents.FirstOrDefault(a => a.Id == info.Id);
            if (existing != null)
            {
                existing.UpdateFrom(info);
            }
            else
            {
                var newAgent = new AgentViewModel();
                newAgent.UpdateFrom(info);
                Agents.Add(newAgent);
            }
        }
    }

    private void UpdateSummary(MultiAgentPayload payload)
    {
        TotalAgents = Agents.Count;
        AwaitingInputCount = Agents.Count(a => a.IsWaitingForInput || a.Status == AgentStatus.Waiting);

        // Format total tokens
        var totalTokens = payload.TotalTokens > 0 ? payload.TotalTokens : Agents.Sum(a => a.TokensUsed);
        TotalTokensDisplay = totalTokens >= 1000 ? $"{totalTokens:N0}" : totalTokens.ToString();

        // Format total elapsed
        var totalElapsed = payload.TotalElapsed.TotalSeconds > 0
            ? payload.TotalElapsed
            : TimeSpan.FromMilliseconds(Agents.Sum(a =>
                a.ElapsedTime.Contains(':')
                    ? (int.Parse(a.ElapsedTime.Split(':')[0]) * 60 + int.Parse(a.ElapsedTime.Split(':')[1])) * 1000
                    : 0));
        TotalElapsedDisplay = totalElapsed.TotalHours >= 1
            ? totalElapsed.ToString(@"hh\:mm")
            : totalElapsed.ToString(@"mm\:ss");

        // Format total cost
        var totalCost = payload.TotalCost > 0 ? payload.TotalCost : Agents.Sum(a => a.Cost);
        TotalCostDisplay = totalCost >= 1 ? $"${totalCost:F2}" : $"${totalCost:F3}";
    }

    private void CheckForAudioNotifications(List<AgentInfo> agents)
    {
        if (!AudioEnabled) return;

        foreach (var agent in agents)
        {
            var previousStatus = _previousStatuses.GetValueOrDefault(agent.Id, AgentStatus.Idle);

            if (previousStatus != agent.Status)
            {
                _previousStatuses[agent.Id] = agent.Status;

                if (agent.Status == AgentStatus.Waiting ||
                    agent.Status == AgentStatus.Finished ||
                    agent.Status == AgentStatus.Error ||
                    agent.IsWaitingForInput)
                {
                    _ = _audioService.PlayStatusChangeAsync(agent.Status);
                }
            }
        }
    }

    private void UpdateBlinkTimer()
    {
        var hasBlinkingAgents = Agents.Any(a => a.Status == AgentStatus.Working || a.IsWaitingForInput);

        if (hasBlinkingAgents && !_blinkTimer.IsRunning)
        {
            _blinkTimer.Start();
        }
        else if (!hasBlinkingAgents && _blinkTimer.IsRunning)
        {
            _blinkTimer.Stop();
            foreach (var agent in Agents)
            {
                agent.IsBlinkVisible = true;
            }
        }
    }

    private void RemoveStaleAgents()
    {
        var now = DateTime.UtcNow;
        var toRemove = Agents.Where(a =>
            (a.Status == AgentStatus.Working && (now - a.LastDataChange).TotalSeconds > 30) ||
            (a.Status == AgentStatus.Finished && (now - a.LastDataChange).TotalSeconds > 60) ||
            (a.Status == AgentStatus.Error && (now - a.LastDataChange).TotalSeconds > 60)
        ).ToList();

        foreach (var agent in toRemove)
        {
            _previousStatuses.Remove(agent.Id);
            Agents.Remove(agent);
        }
    }

    [RelayCommand]
    private void ToggleAudio()
    {
        AudioEnabled = !AudioEnabled;
        _audioService.IsEnabled = AudioEnabled;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _clockTimer.Stop();
        _blinkTimer.Stop();
        _staleTimer.Stop();
        _statusService.AgentsChanged -= OnAgentsChanged;

        if (_statusService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
