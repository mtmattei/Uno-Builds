namespace FluxTransit.Presentation;

public partial record ProfileModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _localizer;

    public ProfileModel(INavigator navigator, IStringLocalizer localizer)
    {
        _navigator = navigator;
        _localizer = localizer;
    }

    // User info
    public string UserName => "Commuter";
    public string CardNumber => "**** 4521";

    // OPUS Balance state
    public IState<decimal> OpusBalance => State<decimal>.Value(this, () => 18.50m);

    // Gemini API Key state
    public IState<string> GeminiApiKey => State<string>.Value(this, () => string.Empty);

    // Language selection (EN or FR)
    public IState<string> SelectedLanguage => State<string>.Value(this, () => "EN");

    // Is refreshing balance
    public IState<bool> IsRefreshing => State<bool>.Value(this, () => false);

    // Navigate back command
    public async Task GoBack()
    {
        await _navigator.GoBack(this);
    }

    // Update balance command - simulates a refresh
    public async Task UpdateBalance(CancellationToken ct)
    {
        await IsRefreshing.Set(true, ct);
        try
        {
            // Simulate API call
            await Task.Delay(1000, ct);
            // Update with a slightly random balance
            var random = new Random();
            var newBalance = 15.00m + (decimal)random.NextDouble() * 10;
            await OpusBalance.Set(Math.Round(newBalance, 2), ct);
        }
        finally
        {
            await IsRefreshing.Set(false, ct);
        }
    }

    // Save settings command
    public async Task SaveSettings(CancellationToken ct)
    {
        var apiKey = await GeminiApiKey;
        var language = await SelectedLanguage;

        // In a real app, this would save to localStorage or preferences
        // For now, just log that settings were saved
        await Task.Delay(100, ct);
    }
}
