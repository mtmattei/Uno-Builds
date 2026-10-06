using FreewriteUno.InlineAi.Models;
using FreewriteUno.InlineAi.Services;

namespace FreewriteUno.Tests;

public sealed class FakeAiServiceTests
{
    // Zero per-chunk delay keeps the suite fast; the streaming contract is unaffected.
    private static FakeAiService NewService() => new(TimeSpan.Zero);

    private static AiRequest Request(AiIntent intent, string selection = "The quick brown fox.", string? prompt = null, ToneOption? tone = null)
        => new(intent, new SelectionContext(selection, 0, selection.Length), prompt, tone);

    private static async Task<string> CollectAsync(IAiService svc, AiRequest request)
    {
        var sb = new System.Text.StringBuilder();
        await foreach (var chunk in svc.StreamAsync(request))
        {
            sb.Append(chunk);
        }
        return sb.ToString();
    }

    [Fact]
    public async Task StreamAsync_ChunksReassembleToNonEmptyResponse()
    {
        var result = await CollectAsync(NewService(), Request(AiIntent.Rewrite));
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task StreamAsync_PromptContainingFail_Throws()
    {
        var svc = NewService();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CollectAsync(svc, Request(AiIntent.Ask, prompt: "please fail this turn")));
    }

    [Theory]
    [InlineData(AiIntent.Rewrite)]
    [InlineData(AiIntent.Shorten)]
    [InlineData(AiIntent.ChangeTone)]
    [InlineData(AiIntent.Summarize)]
    [InlineData(AiIntent.Explain)]
    [InlineData(AiIntent.Ask)]
    public async Task StreamAsync_EveryIntent_ProducesText(AiIntent intent)
    {
        var result = await CollectAsync(NewService(), Request(intent));
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public async Task StreamAsync_Cancellation_StopsStreaming()
    {
        // Non-zero delay so cancellation lands mid-stream rather than after completion.
        var svc = new FakeAiService(TimeSpan.FromMilliseconds(20));
        using var cts = new CancellationTokenSource();
        var request = Request(AiIntent.Explain, selection: new string('a', 400));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in svc.StreamAsync(request, cts.Token))
            {
                cts.Cancel();
            }
        });
    }

    [Fact]
    public async Task StreamAsync_ChangeTone_RespectsToneOption()
    {
        var formal = await CollectAsync(NewService(), Request(AiIntent.ChangeTone, tone: ToneOption.Formal));
        var casual = await CollectAsync(NewService(), Request(AiIntent.ChangeTone, tone: ToneOption.Casual));
        Assert.NotEqual(formal, casual);
    }

    [Fact]
    public void ProducesRewrite_OnlyRewriteStyleIntents()
    {
        Assert.True(AiIntent.Rewrite.ProducesRewrite());
        Assert.True(AiIntent.Shorten.ProducesRewrite());
        Assert.True(AiIntent.ChangeTone.ProducesRewrite());
        Assert.False(AiIntent.Summarize.ProducesRewrite());
        Assert.False(AiIntent.Explain.ProducesRewrite());
        Assert.False(AiIntent.Ask.ProducesRewrite());
    }
}
