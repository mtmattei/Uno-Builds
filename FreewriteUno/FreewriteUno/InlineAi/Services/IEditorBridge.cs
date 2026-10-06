using FreewriteUno.InlineAi.Models;

namespace FreewriteUno.InlineAi.Services;

/// <summary>
/// Mediates the two imperative seams between the reactive <c>InlineAiModel</c> and the live document
/// surface: the host publishes the current selection here, and the model asks it to apply a rewrite or
/// release the held selection. Registered as a singleton so the model resolves it by constructor injection
/// rather than reaching for <c>App.Current</c>.
/// </summary>
public interface IEditorBridge
{
    /// <summary>Latest selection captured by the host, read by the model when the chat opens.</summary>
    SelectionContext CurrentSelection { get; set; }

    /// <summary>Host-supplied writer that replaces the target range in the document. Returns false if the range is gone.</summary>
    Func<RewriteResult, Task<bool>>? Applier { get; set; }

    /// <summary>Host-supplied hook to clear the held-selection highlight when the chat dismisses.</summary>
    Action? SelectionReleaser { get; set; }

    Task<bool> ApplyAsync(RewriteResult result);

    void ReleaseSelection();
}

public sealed class EditorBridge : IEditorBridge
{
    public SelectionContext CurrentSelection { get; set; } = SelectionContext.Empty;

    public Func<RewriteResult, Task<bool>>? Applier { get; set; }

    public Action? SelectionReleaser { get; set; }

    public Task<bool> ApplyAsync(RewriteResult result) =>
        Applier?.Invoke(result) ?? Task.FromResult(false);

    public void ReleaseSelection() => SelectionReleaser?.Invoke();
}
