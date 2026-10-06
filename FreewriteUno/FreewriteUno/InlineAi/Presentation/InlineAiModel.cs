using System.Text;
using FreewriteUno.InlineAi.Models;
using FreewriteUno.InlineAi.Services;
using Uno.Extensions.Reactive;

namespace FreewriteUno.InlineAi.Presentation;

/// <summary>
/// MVUX model for the inline AI chat. Owns the held selection, the conversation, the composer draft,
/// and the streaming lifecycle. Public async methods auto-generate the commands the view binds to;
/// the document-mutating seams go through <see cref="IEditorBridge"/>.
/// <para>
/// This is an MVUX island inside an otherwise MVVM app: the model is self-contained, so the patterns
/// coexist without mixing. The generator emits <c>InlineAiViewModel</c> from this <c>InlineAiModel</c>.
/// </para>
/// </summary>
public partial record InlineAiModel
{
    private readonly IAiService _ai;
    private readonly IClipboardService _clipboard;
    private readonly IEditorBridge _editor;
    private bool _wasOpen;

    public InlineAiModel(IAiService ai, IClipboardService clipboard, IEditorBridge editor)
    {
        _ai = ai;
        _clipboard = clipboard;
        _editor = editor;

        // Route every close path (button, Esc, light-dismiss) through one reset.
        IsChatOpen.ForEach(OnChatOpenChanged);
    }

    // ----- reactive surface (bound in XAML via {Binding}) -----

    /// <summary>The held selection text + source range, set when the chat opens.</summary>
    public IState<SelectionContext> Selection => State.Value(this, () => SelectionContext.Empty);

    /// <summary>Card popup visibility; two-way bound so light-dismiss flows back into the model.</summary>
    public IState<bool> IsChatOpen => State.Value(this, () => false);

    /// <summary>The conversation (user + assistant turns).</summary>
    public IListState<ChatMessage> Messages => ListState<ChatMessage>.Empty(this);

    /// <summary>Composer text, two-way bound to the input.</summary>
    public IState<string> Draft => State<string>.Value(this, () => string.Empty);

    /// <summary>True while a turn streams; gates input so requests can't overlap.</summary>
    public IState<bool> IsResponding => State.Value(this, () => false);

    /// <summary>Whether the "Change tone" sub-options are revealed.</summary>
    public IState<bool> IsToneExpanded => State.Value(this, () => false);

    /// <summary>Latest transient feedback ("Copied", "Applied", "Couldn't apply"); empty string means no toast.</summary>
    public IState<string> Toast => State<string>.Value(this, () => string.Empty);

    /// <summary>Quick actions and composer are enabled only when idle.</summary>
    public IFeed<bool> IsIdle => IsResponding.Select(responding => !responding);

    /// <summary>Send is enabled only when the draft has content (overlap is also guarded internally).</summary>
    public IFeed<bool> CanSend => Draft.Select(draft => !string.IsNullOrWhiteSpace(draft));

    /// <summary>Selected text projected for the context chip (avoids a nested binding path through the state).</summary>
    public IFeed<string> SelectionText => Selection.Select(s => s?.Text ?? string.Empty);

    // ----- commands (auto-generated from public async methods) -----

    /// <summary>Pill action: open the chat against the selection the host captured.</summary>
    public async ValueTask OpenChat(CancellationToken ct = default)
    {
        await ResetConversation(ct);
        await Selection.UpdateAsync(_ => _editor.CurrentSelection, ct);
        await IsChatOpen.SetAsync(true, ct);
    }

    /// <summary>Close button / Esc. Light-dismiss reaches the same place via the two-way IsChatOpen binding.</summary>
    public async ValueTask Close(CancellationToken ct = default)
    {
        await IsChatOpen.SetAsync(false, ct);
    }

    public async ValueTask SendAsync(CancellationToken ct = default)
    {
        var draft = (await Draft)?.Trim();
        if (string.IsNullOrEmpty(draft))
        {
            return;
        }

        await Draft.SetAsync(string.Empty, ct);
        var request = new AiRequest(AiIntent.Ask, await Selection ?? SelectionContext.Empty, Prompt: draft);
        await RunTurn(request, draft, ct);
    }

    public ValueTask Rewrite(CancellationToken ct = default) => RunQuickAction(AiIntent.Rewrite, ct);
    public ValueTask Summarize(CancellationToken ct = default) => RunQuickAction(AiIntent.Summarize, ct);
    public ValueTask Explain(CancellationToken ct = default) => RunQuickAction(AiIntent.Explain, ct);
    public ValueTask Shorten(CancellationToken ct = default) => RunQuickAction(AiIntent.Shorten, ct);

    public async ValueTask ToggleTone(CancellationToken ct = default)
    {
        var expanded = await IsToneExpanded;
        await IsToneExpanded.SetAsync(!expanded, ct);
    }

    /// <summary>Tone is passed as a string from the chip's CommandParameter, then parsed to the enum.</summary>
    public async ValueTask ChangeTone(string tone, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ToneOption>(tone, ignoreCase: true, out var parsed))
        {
            return;
        }

        await IsToneExpanded.SetAsync(false, ct);
        var request = new AiRequest(AiIntent.ChangeTone, await Selection ?? SelectionContext.Empty, Tone: parsed);
        await RunTurn(request, $"Change tone — {parsed}", ct);
    }

    /// <summary>Re-run a failed turn with the same intent, dropping the errored assistant bubble first.</summary>
    public async ValueTask Retry(ChatMessage message, CancellationToken ct = default)
    {
        if (message?.Request is not { } request || await IsResponding)
        {
            return;
        }

        await Messages.Update(list => list.Count == 0 ? list : list.RemoveAt(list.Count - 1), ct);
        await StreamInto(request, ct);
    }

    public async ValueTask ApplyResult(ChatMessage message, CancellationToken ct = default)
    {
        if (message?.Result is not { CanApply: true } result)
        {
            return;
        }

        if (await _editor.ApplyAsync(result))
        {
            await ReplaceMessage(message, m => m with { Result = m.Result! with { IsApplied = true } }, ct);
            await ShowToast("Applied", ct);
            await IsChatOpen.SetAsync(false, ct);
        }
        else
        {
            await ShowToast("Couldn't apply", ct);
        }
    }

    public async ValueTask CopyResult(ChatMessage message, CancellationToken ct = default)
    {
        var text = message?.Result?.Text ?? message?.Text;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        await _clipboard.SetTextAsync(text);
        await ShowToast("Copied", ct);
    }

    public async ValueTask DiscardResult(ChatMessage message, CancellationToken ct = default)
    {
        if (message?.Result is null)
        {
            return;
        }

        await ReplaceMessage(message, m => m with { Result = m.Result! with { IsDiscarded = true } }, ct);
    }

    // ----- internals -----

    private async ValueTask RunQuickAction(AiIntent intent, CancellationToken ct)
    {
        var request = new AiRequest(intent, await Selection ?? SelectionContext.Empty);
        await RunTurn(request, LabelFor(intent), ct);
    }

    private async ValueTask RunTurn(AiRequest request, string userBubbleText, CancellationToken ct)
    {
        if (await IsResponding)
        {
            return; // no overlapping requests
        }

        await Messages.AddAsync(ChatMessage.UserTurn(userBubbleText), ct);
        await StreamInto(request, ct);
    }

    private async ValueTask StreamInto(AiRequest request, CancellationToken ct)
    {
        await ClearToast(ct);
        await IsResponding.SetAsync(true, ct);
        await Messages.AddAsync(ChatMessage.PendingAssistant(request), ct);

        var accumulated = new StringBuilder();
        try
        {
            await foreach (var chunk in _ai.StreamAsync(request, ct).WithCancellation(ct))
            {
                accumulated.Append(chunk);
                var text = accumulated.ToString();
                await ReplaceLast(m => m with { Text = text, Status = MessageStatus.Streaming }, ct);
            }

            var finalText = accumulated.ToString();
            var result = request.Intent.ProducesRewrite() && !string.IsNullOrWhiteSpace(finalText)
                ? new RewriteResult(finalText, request.Selection.Start, request.Selection.Length)
                : null;

            await ReplaceLast(m => m with { Text = finalText, Status = MessageStatus.Complete, Result = result }, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            await ReplaceLast(
                m => m with { Status = MessageStatus.Error, Text = "Something went wrong. Try again." },
                ct);
        }
        finally
        {
            await IsResponding.SetAsync(false, ct);
        }
    }

    private ValueTask ReplaceLast(Func<ChatMessage, ChatMessage> updater, CancellationToken ct) =>
        Messages.Update(list => list.Count == 0 ? list : list.SetItem(list.Count - 1, updater(list[^1])), ct);

    private ValueTask ReplaceMessage(ChatMessage target, Func<ChatMessage, ChatMessage> updater, CancellationToken ct) =>
        Messages.Update(
            list =>
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Id == target.Id)
                    {
                        return list.SetItem(i, updater(list[i]));
                    }
                }

                return list;
            },
            ct);

    private async ValueTask ResetConversation(CancellationToken ct)
    {
        await Messages.Update(_ => ImmutableList<ChatMessage>.Empty, ct);
        await Draft.SetAsync(string.Empty, ct);
        await IsToneExpanded.SetAsync(false, ct);
        await ClearToast(ct);
    }

    private async ValueTask OnChatOpenChanged(bool open, CancellationToken ct)
    {
        if (!open && _wasOpen)
        {
            await ResetConversation(ct);
            await Selection.UpdateAsync(_ => SelectionContext.Empty, ct);
            _editor.ReleaseSelection();
        }

        _wasOpen = open;
    }

    private ValueTask ShowToast(string text, CancellationToken ct) => Toast.SetAsync(text, ct);

    // Empty string is the "no toast" sentinel (Toast is IState<string>, not nullable).
    private ValueTask ClearToast(CancellationToken ct) => Toast.SetAsync(string.Empty, ct);

    private static string LabelFor(AiIntent intent) => intent switch
    {
        AiIntent.Rewrite => "Rewrite",
        AiIntent.Summarize => "Summarize",
        AiIntent.Explain => "Explain",
        AiIntent.Shorten => "Make shorter",
        AiIntent.ChangeTone => "Change tone",
        _ => "Ask",
    };
}
