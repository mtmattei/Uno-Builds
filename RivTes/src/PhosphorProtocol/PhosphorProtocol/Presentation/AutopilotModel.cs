using PhosphorProtocol.Services;

namespace PhosphorProtocol.Presentation;

public partial record AutopilotModel(IAutopilotService AutopilotService)
{
    public IFeed<AutopilotState> Autopilot => Feed.Async(async ct => await AutopilotService.GetCurrentState(ct));
}
