using ADTest.Models;

namespace ADTest.Services;

public class ConversationService
{
    public async ValueTask<ImmutableList<DateGroupedConversations>> GetConversationsAsync(CancellationToken ct)
    {
        await Task.Delay(300, ct);

        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var olderDate = new DateTime(2026, 2, 8);

        return ImmutableList.Create(
            new DateGroupedConversations("Today", ImmutableList.Create(
                new ConversationItem(Guid.NewGuid().ToString(), "Asked for a high-protein meal plan", today.AddHours(14), "Today"),
                new ConversationItem(Guid.NewGuid().ToString(), "Worked on the b402 dashboard UX", today.AddHours(11), "Today"),
                new ConversationItem(Guid.NewGuid().ToString(), "Brainstormed side projects", today.AddHours(9), "Today")
            )),
            new DateGroupedConversations("Yesterday", ImmutableList.Create(
                new ConversationItem(Guid.NewGuid().ToString(), "Researched ETF investing strategies", yesterday.AddHours(16), "Yesterday"),
                new ConversationItem(Guid.NewGuid().ToString(), "Drafted a polite client email", yesterday.AddHours(14), "Yesterday"),
                new ConversationItem(Guid.NewGuid().ToString(), "Asked about improving sleep quality", yesterday.AddHours(11), "Yesterday"),
                new ConversationItem(Guid.NewGuid().ToString(), "Generated tagline ideas for a landing page", yesterday.AddHours(9), "Yesterday")
            )),
            new DateGroupedConversations("Feb 8, 2026", ImmutableList.Create(
                new ConversationItem(Guid.NewGuid().ToString(), "Explained a TypeScript generic error", olderDate.AddHours(15), "Feb 8, 2026"),
                new ConversationItem(Guid.NewGuid().ToString(), "Helped rewrite a LinkedIn profile summary", olderDate.AddHours(10), "Feb 8, 2026")
            ))
        );
    }
}
