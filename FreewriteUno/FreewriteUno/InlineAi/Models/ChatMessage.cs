namespace FreewriteUno.InlineAi.Models;

public enum ChatRole { User, Assistant }

/// <summary>Lifecycle of an assistant turn, used by the view to branch on loading / error rendering.</summary>
public enum MessageStatus { Pending, Streaming, Complete, Error }

/// <summary>
/// A proposed rewrite carried by an assistant turn. <see cref="CanApply"/> gates the Apply action;
/// the target range is captured at request time so Apply can write back even if the live selection moved.
/// </summary>
public record RewriteResult(string Text, int TargetStart, int TargetLength)
{
    public bool CanApply => !string.IsNullOrEmpty(Text) && TargetLength > 0;

    /// <summary>Set true once the rewrite has been written into the document, to dim the result card.</summary>
    public bool IsApplied { get; init; }

    /// <summary>Set true when Discard is chosen, to dim actions in place.</summary>
    public bool IsDiscarded { get; init; }
}

/// <summary>
/// One conversation turn (user or assistant). Immutable; the model replaces the in-flight assistant
/// message with <c>with</c>-updates as tokens stream in. Convenience flags keep the item template lean.
/// </summary>
public partial record ChatMessage
{
    /// <summary>Stable identity so streaming updates and Apply/Discard target the right turn. Preserved across <c>with</c>-copies.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("n");

    public ChatRole Role { get; init; }
    public string Text { get; init; } = string.Empty;
    public MessageStatus Status { get; init; } = MessageStatus.Complete;
    public RewriteResult? Result { get; init; }

    /// <summary>The request that produced this assistant turn, kept so Retry can re-run the same intent.</summary>
    public AiRequest? Request { get; init; }

    public bool IsUser => Role == ChatRole.User;
    public bool IsAssistant => Role == ChatRole.Assistant;
    public bool IsPending => Status == MessageStatus.Pending;
    public bool IsStreaming => Status == MessageStatus.Streaming;
    public bool IsError => Status == MessageStatus.Error;
    public bool HasResult => Result is { CanApply: true, IsDiscarded: false };

    // ----- view-facing flags (keep the item template free of converters) -----

    /// <summary>True for rewrite-style turns: text streams into the result card rather than a prose bubble.</summary>
    public bool WillProduceRewrite => Request?.Intent.ProducesRewrite() == true;

    public bool ShowError => IsAssistant && IsError;
    public bool ShowResultCard => IsAssistant && !IsError && WillProduceRewrite;
    public bool ShowProse => IsAssistant && !IsError && !WillProduceRewrite;
    public bool ShowTypingDots => IsPending;
    public bool ShowCaret => IsStreaming;

    /// <summary>Result actions are live only before the result is applied or discarded.</summary>
    public bool ResultActionsEnabled => Result is { CanApply: true, IsApplied: false, IsDiscarded: false };
    public bool IsResultSettled => Result is { IsApplied: true } or { IsDiscarded: true };

    /// <summary>Text to render (prose or result body); during streaming the result body mirrors the in-flight text.</summary>
    public string DisplayText => Result?.Text ?? Text;

    public static ChatMessage UserTurn(string text) => new() { Role = ChatRole.User, Text = text };

    public static ChatMessage PendingAssistant(AiRequest request) => new()
    {
        Role = ChatRole.Assistant,
        Status = MessageStatus.Pending,
        Request = request,
    };
}
