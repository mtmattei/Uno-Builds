namespace QuoteCraft.Presentation;

public partial record DashboardModel
{
    private readonly INavigator _navigator;
    private readonly IQuoteRepository _quoteRepo;
    private readonly IClientRepository _clientRepo;
    private readonly IBusinessProfileRepository _profileRepo;
    private readonly ICatalogItemRepository _catalogRepo;
    private readonly QuoteNumberGenerator _quoteNumberGen;
    private readonly Services.IShareService _shareService;
    private readonly Services.IFeatureGateService _featureGate;

    public DashboardModel(
        INavigator navigator,
        IQuoteRepository quoteRepo,
        IClientRepository clientRepo,
        IBusinessProfileRepository profileRepo,
        ICatalogItemRepository catalogRepo,
        QuoteNumberGenerator quoteNumberGen,
        Services.IShareService shareService,
        Services.IFeatureGateService featureGate)
    {
        _navigator = navigator;
        _quoteRepo = quoteRepo;
        _clientRepo = clientRepo;
        _profileRepo = profileRepo;
        _catalogRepo = catalogRepo;
        _quoteNumberGen = quoteNumberGen;
        _shareService = shareService;
        _featureGate = featureGate;
    }

    public IState<string> SelectedFilter => State<string>.Value(this, () => "All");
    public IState<string> SearchText => State<string>.Value(this, () => string.Empty);
    public IState<QuoteEntity> SelectedQuote => State<QuoteEntity>.Empty(this);
    public IState<bool> IsPreviewMode => State<bool>.Value(this, () => false);
    public IState<int> DetailVersion => State<int>.Value(this, () => 0);

    // Feature gate state: shown when a limit is hit
    public IState<string> UpgradeMessage => State<string>.Value(this, () => string.Empty);

    public IListFeed<QuoteEntity> Quotes =>
        Feed.Combine(SelectedFilter, SearchText, DetailVersion)
            .SelectAsync(async (inputs, ct) =>
            {
                var (filter, search, _) = inputs;
                var all = await _quoteRepo.GetAllAsync();

                IEnumerable<QuoteEntity> filtered = all;

                if (!string.IsNullOrEmpty(filter) && filter != "All")
                    filtered = filtered.Where(q => Enum.TryParse<QuoteStatus>(filter, out var s) && q.Status == s);

                if (!string.IsNullOrWhiteSpace(search))
                    filtered = filtered.Where(q =>
                        q.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (q.ClientName ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        q.QuoteNumber.Contains(search, StringComparison.OrdinalIgnoreCase));

                return (IImmutableList<QuoteEntity>)filtered.ToImmutableList();
            })
            .AsListFeed();

    public IFeed<int> TotalCount => Feed.Async(async ct => (await _quoteRepo.GetAllAsync()).Count);

    // Analytics: computed from all quotes
    public IFeed<DashboardAnalytics> Analytics => DetailVersion
        .SelectAsync(async (_, ct) =>
        {
            var all = await _quoteRepo.GetAllAsync();
            var now = DateTimeOffset.UtcNow;
            var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

            var thisMonth = all.Where(q => q.CreatedAt >= startOfMonth).ToList();

            var totalQuoted = thisMonth.Sum(q => q.Total);
            var sent = thisMonth.Count(q => q.Status != QuoteStatus.Draft);

            var resolved = all.Count(q =>
                q.Status == QuoteStatus.Accepted || q.Status == QuoteStatus.Declined);
            var accepted = all.Count(q => q.Status == QuoteStatus.Accepted);
            var rate = resolved > 0 ? (double)accepted / resolved * 100 : 0;

            return new DashboardAnalytics(totalQuoted, sent, rate);
        });

    // Combined detail feed: re-fetches from DB on version bump for inline edits
    public IFeed<QuoteDetail> SelectedQuoteDetail =>
        Feed.Combine(SelectedQuote, DetailVersion)
            .SelectAsync(async (inputs, ct) =>
            {
                var (quote, _) = inputs;
                var freshQuote = await _quoteRepo.GetByIdAsync(quote.Id);
                if (freshQuote is null)
                    return new QuoteDetail(quote, ImmutableList<LineItemEntity>.Empty);
                ClientEntity? client = null;
                if (!string.IsNullOrEmpty(freshQuote.ClientId))
                    client = await _clientRepo.GetByIdAsync(freshQuote.ClientId);
                return new QuoteDetail(freshQuote, freshQuote.LineItems.ToImmutableList(), client);
            });

    // Preview feed: quote + line items + business profile + client
    public IFeed<QuotePreviewData> PreviewData =>
        Feed.Combine(SelectedQuote, DetailVersion)
            .SelectAsync(async (inputs, ct) =>
            {
                var (quote, _) = inputs;
                var freshQuote = await _quoteRepo.GetByIdAsync(quote.Id) ?? quote;
                var profile = await _profileRepo.GetAsync();
                ClientEntity? client = null;
                if (!string.IsNullOrEmpty(freshQuote.ClientId))
                    client = await _clientRepo.GetByIdAsync(freshQuote.ClientId);
                return new QuotePreviewData(freshQuote, freshQuote.LineItems.ToImmutableList(), profile, client);
            });

    public async ValueTask RefreshDetail(CancellationToken ct)
    {
        await DetailVersion.UpdateAsync(v => v + 1, ct);
    }

    public async ValueTask ExpireOverdueQuotes(CancellationToken ct)
    {
        var all = await _quoteRepo.GetAllAsync();
        var now = DateTimeOffset.UtcNow;
        var expirable = all.Where(q =>
            (q.Status == QuoteStatus.Draft || q.Status == QuoteStatus.Sent) &&
            q.ValidUntil.HasValue &&
            q.ValidUntil.Value < now).ToList();

        foreach (var q in expirable)
        {
            q.Status = QuoteStatus.Expired;
            q.UpdatedAt = DateTimeOffset.UtcNow;
            await _quoteRepo.SaveAsync(q);
        }

        if (expirable.Count > 0)
            await DetailVersion.UpdateAsync(v => v + 1, ct);
    }

    public async ValueTask ReopenQuote(CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is not null && quote.Status == QuoteStatus.Expired)
        {
            var fresh = await _quoteRepo.GetByIdAsync(quote.Id);
            if (fresh is not null)
            {
                fresh.Status = QuoteStatus.Draft;
                fresh.ValidUntil = DateTimeOffset.UtcNow.AddDays(
                    (await _profileRepo.GetAsync()).QuoteValidDays);
                fresh.UpdatedAt = DateTimeOffset.UtcNow;
                await _quoteRepo.SaveAsync(fresh);
                await SelectedQuote.UpdateAsync(_ => fresh, ct);
                await DetailVersion.UpdateAsync(v => v + 1, ct);
            }
        }
    }

    public async ValueTask OpenQuote(QuoteEntity quote, CancellationToken ct)
    {
        await IsPreviewMode.UpdateAsync(_ => false, ct);
        await SelectedQuote.UpdateAsync(_ => quote, ct);
    }

    public async ValueTask EditQuote(CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is not null)
            await _navigator.NavigateRouteAsync(this, "QuoteEditor", data: quote!);
    }

    public async ValueTask DeleteQuote(CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is not null)
        {
            await _quoteRepo.DeleteAsync(quote.Id);
            await SelectedQuote.UpdateAsync(_ => null!, ct);
            await DetailVersion.UpdateAsync(v => v + 1, ct);
        }
    }

    public async ValueTask PreviewQuote(CancellationToken ct)
    {
        await IsPreviewMode.UpdateAsync(_ => true, ct);
    }

    public async ValueTask BackToDetail(CancellationToken ct)
    {
        await IsPreviewMode.UpdateAsync(_ => false, ct);
    }

    public async ValueTask DownloadPdf(CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is not null)
        {
            var freshQuote = await _quoteRepo.GetByIdAsync(quote.Id);
            if (freshQuote is not null)
            {
                await _shareService.ShareQuotePdfAsync(freshQuote);
                await DetailVersion.UpdateAsync(v => v + 1, ct);
            }
        }
    }

    public async ValueTask SendQuote(CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is not null)
        {
            var freshQuote = await _quoteRepo.GetByIdAsync(quote.Id);
            if (freshQuote is not null)
            {
                await _shareService.ShareQuotePdfAsync(freshQuote);
                await DetailVersion.UpdateAsync(v => v + 1, ct);
            }
        }
    }

    public async ValueTask CreateQuote(CancellationToken ct)
    {
        // Feature gate: check quote limit
        if (!await _featureGate.CanCreateQuoteAsync())
        {
            await UpgradeMessage.UpdateAsync(_ => _featureGate.GetUpgradeMessage("quotes"), ct);
            return;
        }

        var profile = await _profileRepo.GetAsync();
        var quoteNumber = await _quoteNumberGen.GenerateAsync();
        var quote = new QuoteEntity
        {
            Title = "New Quote",
            QuoteNumber = quoteNumber,
            Status = QuoteStatus.Draft,
            TaxRate = profile.DefaultTaxRate,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(profile.QuoteValidDays),
        };
        await _quoteRepo.SaveAsync(quote);
        await _navigator.NavigateRouteAsync(this, "QuoteEditor", data: quote);
    }

    public async ValueTask DuplicateQuote(CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is null) return;

        // Feature gate: check quote limit
        if (!await _featureGate.CanCreateQuoteAsync())
        {
            await UpgradeMessage.UpdateAsync(_ => _featureGate.GetUpgradeMessage("quotes"), ct);
            return;
        }

        var freshQuote = await _quoteRepo.GetByIdAsync(quote.Id);
        if (freshQuote is null) return;

        var quoteNumber = await _quoteNumberGen.GenerateAsync();
        var newQuote = new QuoteEntity
        {
            Title = freshQuote.Title + " (Copy)",
            QuoteNumber = quoteNumber,
            Status = QuoteStatus.Draft,
            ClientId = freshQuote.ClientId,
            ClientName = freshQuote.ClientName,
            Notes = freshQuote.Notes,
            TaxRate = freshQuote.TaxRate,
            ValidUntil = DateTimeOffset.UtcNow.AddDays(
                (await _profileRepo.GetAsync()).QuoteValidDays),
        };
        await _quoteRepo.SaveAsync(newQuote);

        // Clone line items
        foreach (var item in freshQuote.LineItems)
        {
            var newItem = new LineItemEntity
            {
                QuoteId = newQuote.Id,
                Description = item.Description,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                SortOrder = item.SortOrder,
            };
            await _quoteRepo.SaveLineItemAsync(newItem);
        }

        // Open the duplicated quote in the editor
        await _navigator.NavigateRouteAsync(this, "QuoteEditor", data: newQuote);
    }

    public async ValueTask DismissUpgrade(CancellationToken ct)
    {
        await UpgradeMessage.UpdateAsync(_ => string.Empty, ct);
    }

    // ── Inline Edit Support ──────────────────────────────────────────────

    public async ValueTask SaveInlineNotes(string notes, CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is null) return;

        var fresh = await _quoteRepo.GetByIdAsync(quote.Id);
        if (fresh is not null && fresh.Notes != notes)
        {
            fresh.Notes = notes;
            await _quoteRepo.SaveAsync(fresh);
            await DetailVersion.UpdateAsync(v => v + 1, ct);
        }
    }

    public async ValueTask SaveInlineLineItem(LineItemEntity item, CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is null || string.IsNullOrWhiteSpace(item.Description)) return;

        item.QuoteId = quote.Id;
        if (string.IsNullOrEmpty(item.Id))
            item.SortOrder = (await _quoteRepo.GetLineItemsAsync(quote.Id)).Count;

        await _quoteRepo.SaveLineItemAsync(item);
        await DetailVersion.UpdateAsync(v => v + 1, ct);
    }

    public async ValueTask DeleteInlineLineItem(string lineItemId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(lineItemId)) return;
        await _quoteRepo.DeleteLineItemAsync(lineItemId);
        await DetailVersion.UpdateAsync(v => v + 1, ct);
    }

    public async ValueTask AddInlineFromCatalog(CatalogItemEntity catalogItem, CancellationToken ct)
    {
        var quote = await SelectedQuote;
        if (quote is null) return;

        var item = new LineItemEntity
        {
            QuoteId = quote.Id,
            Description = catalogItem.Description,
            UnitPrice = catalogItem.UnitPrice,
            Quantity = 1,
            SortOrder = (await _quoteRepo.GetLineItemsAsync(quote.Id)).Count,
        };
        await _quoteRepo.SaveLineItemAsync(item);
        await DetailVersion.UpdateAsync(v => v + 1, ct);
    }

    public Task<List<CatalogItemEntity>> GetCatalogItemsAsync() => _catalogRepo.GetAllAsync();

    public async Task<string> GetSelectedQuoteNotesAsync()
    {
        var quote = await SelectedQuote;
        if (quote is not null)
        {
            var fresh = await _quoteRepo.GetByIdAsync(quote.Id);
            return fresh?.Notes ?? string.Empty;
        }
        return string.Empty;
    }
}
