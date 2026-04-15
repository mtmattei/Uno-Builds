namespace ADTest.Models;

public partial record DateGroupedConversations(
    string DateLabel,
    ImmutableList<ConversationItem> Items
);
