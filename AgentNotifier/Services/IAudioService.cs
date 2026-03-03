using AgentNotifier.Models;

namespace AgentNotifier.Services;

public interface IAudioService
{
    bool IsEnabled { get; set; }
    double Volume { get; set; }
    Task PlayStatusChangeAsync(AgentStatus status);
}
