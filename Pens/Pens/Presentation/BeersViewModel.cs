using Microsoft.Extensions.Logging;
using Pens.Services;

namespace Pens.Presentation;

public partial class BeersViewModel : ObservableObject
{
    private readonly ISupabaseService _supabase;
    private readonly ILogger<BeersViewModel> _logger;
    private const int TotalCases = 52;
    private const int BeersPerCase = 30;
    private ImmutableList<CaseBlock>? _cachedCaseBlocks;
    private int _cachedConsumedCases = -1;

    public BeersViewModel(ISupabaseService supabase, ILogger<BeersViewModel> logger)
    {
        _supabase = supabase;
        _logger = logger;
        _ = LoadBeerCountAsync();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalBeers))]
    [NotifyPropertyChangedFor(nameof(CaseBlocks))]
    private int _consumedCases = 0;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public int TotalBeers => ConsumedCases * BeersPerCase;
    public int TotalCasesCount => TotalCases;

    public ImmutableList<CaseBlock> CaseBlocks
    {
        get
        {
            if (_cachedCaseBlocks == null || _cachedConsumedCases != ConsumedCases)
            {
                _cachedConsumedCases = ConsumedCases;
                _cachedCaseBlocks = Enumerable.Range(0, TotalCases)
                    .Select(i => new CaseBlock(i, i < ConsumedCases))
                    .ToImmutableList();
            }
            return _cachedCaseBlocks;
        }
    }

    private async Task LoadBeerCountAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var tracker = await _supabase.GetBeerTrackerAsync();
            if (tracker != null)
            {
                ConsumedCases = tracker.ConsumedCases;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading beer count");
            ErrorMessage = "Failed to load beer data";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ToggleCaseAsync(CaseBlock caseBlock)
    {
        var newCount = caseBlock.Index < ConsumedCases
            ? caseBlock.Index
            : caseBlock.Index + 1;

        var previousCount = ConsumedCases;
        ConsumedCases = newCount;
        ErrorMessage = null;

        try
        {
            await _supabase.UpdateBeerCountAsync(newCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating beer count");
            ConsumedCases = previousCount; // Rollback on error
            ErrorMessage = "Failed to save";
        }
    }
}
