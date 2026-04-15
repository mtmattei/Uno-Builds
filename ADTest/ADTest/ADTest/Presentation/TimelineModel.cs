using ADTest.Models;
using ADTest.Services;

namespace ADTest.Presentation;

public partial record TimelineModel
{
    private readonly ConversationService _service = new();

    public IListFeed<DateGroupedConversations> Groups => ListFeed.Async(async ct => await _service.GetConversationsAsync(ct));

    public IState<int> SelectedFilterIndex => State.Value(this, () => 0);
}
