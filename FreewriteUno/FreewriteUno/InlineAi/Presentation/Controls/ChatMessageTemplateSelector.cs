using FreewriteUno.InlineAi.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FreewriteUno.InlineAi.Presentation.Controls;

/// <summary>Branches the message stream on role: a right-aligned user bubble vs. the assistant surface.</summary>
public partial class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserTemplate { get; set; }
    public DataTemplate? AssistantTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item) =>
        item is ChatMessage { IsUser: true } ? UserTemplate : AssistantTemplate;

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) =>
        SelectTemplateCore(item);
}
