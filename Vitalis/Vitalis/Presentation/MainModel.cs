namespace Vitalis.Presentation;

public partial record MainModel
{
    private readonly IGeminiService _geminiService;

    public MainModel(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public IState<Organ> SelectedOrgan => State<Organ>.Value(this, () => OrganData.Heart);

    public IState<bool> IsAnalyzing => State<bool>.Value(this, () => false);

    public IState<AIInsight?> CurrentInsight => State<AIInsight?>.Value(this, () => null);

    public async ValueTask SelectOrgan(Organ organ)
    {
        await SelectedOrgan.UpdateAsync(_ => organ);
        await CurrentInsight.UpdateAsync(_ => null);
    }

    public async ValueTask AnalyzeVitals(CancellationToken ct)
    {
        await IsAnalyzing.UpdateAsync(_ => true);
        try
        {
            var organ = await SelectedOrgan;
            if (organ is not null)
            {
                var insight = await _geminiService.AnalyzeOrganAsync(organ, ct);
                await CurrentInsight.UpdateAsync(_ => insight);
            }
        }
        finally
        {
            await IsAnalyzing.UpdateAsync(_ => false);
        }
    }
}
