using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace QuoteCraft.Presentation;

public sealed partial class DashboardPage : Page
{
    private Border? _selectedQuoteCard;

    private static SolidColorBrush AlternateRowBrush =>
        (SolidColorBrush)Application.Current.Resources["AlternateRowBrush"];

    public static readonly DependencyProperty IsEditModeProperty =
        DependencyProperty.Register(
            nameof(IsEditMode), typeof(bool), typeof(DashboardPage),
            new PropertyMetadata(false));

    public bool IsEditMode
    {
        get => (bool)GetValue(IsEditModeProperty);
        set => SetValue(IsEditModeProperty, value);
    }

    public static readonly DependencyProperty EditingNotesProperty =
        DependencyProperty.Register(
            nameof(EditingNotes), typeof(string), typeof(DashboardPage),
            new PropertyMetadata(string.Empty));

    public string EditingNotes
    {
        get => (string)GetValue(EditingNotesProperty);
        set => SetValue(EditingNotesProperty, value);
    }

    public DashboardPage()
    {
        this.InitializeComponent();
        this.Loaded += DashboardPage_Loaded;
    }

    private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Auto-expire overdue quotes on dashboard load
        if (DataContext is DashboardModel model)
            await model.ExpireOverdueQuotes(CancellationToken.None);
    }

    // -- Quote Card Selection ---------------------------------------------------

    private async void QuoteCard_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Border tappedCard && tappedCard.DataContext is QuoteEntity quote)
        {
            IsEditMode = false;

            // Clear previous selection
            if (_selectedQuoteCard is not null)
            {
                _selectedQuoteCard.Background = (SolidColorBrush)Application.Current.Resources["SurfaceBrush"];
                _selectedQuoteCard.BorderBrush = (SolidColorBrush)Application.Current.Resources["OutlineVariantBrush"];
            }

            // Highlight new selection
            tappedCard.Background = (SolidColorBrush)Application.Current.Resources["PrimaryContainerBrush"];
            tappedCard.BorderBrush = (SolidColorBrush)Application.Current.Resources["PrimaryBrush"];
            _selectedQuoteCard = tappedCard;

            // Invoke model command
            if (DataContext is DashboardModel model)
                await model.OpenQuote(quote, CancellationToken.None);
        }
    }

    // -- Inline Edit ------------------------------------------------------------

    private async void EditQuote_Click(object sender, RoutedEventArgs e)
    {
        IsEditMode = true;
        if (DataContext is DashboardModel model)
            EditingNotes = await model.GetSelectedQuoteNotesAsync();
    }

    private async void DoneEditing_Click(object sender, RoutedEventArgs e)
    {
        IsEditMode = false;
        if (DataContext is DashboardModel model)
            await model.SaveInlineNotes(EditingNotes, CancellationToken.None);
    }

    private async void DeleteQuote_Click(object sender, RoutedEventArgs e)
    {
        var dialog = DialogHelper.BuildBannerDialog(
            this.XamlRoot,
            "\uE74D",
            "Delete Quote?",
            "This quote and all its line items will be permanently removed.",
            closeButtonText: "Cancel",
            primaryButtonText: "Delete");

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (DataContext is DashboardModel model)
                await model.DeleteQuote(CancellationToken.None);
        }
    }

    // -- Alternate Row Styling --------------------------------------------------

    private void LineItems_ElementPrepared(Microsoft.UI.Xaml.Controls.ItemsRepeater sender, Microsoft.UI.Xaml.Controls.ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is FrameworkElement element)
        {
            if (args.Index % 2 == 1)
            {
                if (element is Panel panel) panel.Background = AlternateRowBrush;
                else if (element is Border border) border.Background = AlternateRowBrush;
            }
            else
            {
                if (element is Panel panel) panel.Background = null;
                else if (element is Border border) border.Background = null;
            }
        }
    }

    // -- Inline Line Item Dialog ------------------------------------------------

    private async void InlineAddLineItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new LineItemEditorDialog { XamlRoot = this.XamlRoot };
        dialog.SetAddMode();
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.Result is not null)
        {
            if (DataContext is DashboardModel model)
                await model.SaveInlineLineItem(dialog.Result, CancellationToken.None);
        }
    }

    private async void InlineLineItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (!IsEditMode) return;
        if (sender is FrameworkElement { DataContext: LineItemEntity item })
        {
            var dialog = new LineItemEditorDialog { XamlRoot = this.XamlRoot };
            dialog.SetEditMode(item);
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Result is not null)
            {
                if (DataContext is DashboardModel model)
                    await model.SaveInlineLineItem(dialog.Result, CancellationToken.None);
            }
            else if (result == ContentDialogResult.Secondary && dialog.WasDeleted)
            {
                if (DataContext is DashboardModel model)
                    await model.DeleteInlineLineItem(dialog.EditingItemId ?? string.Empty, CancellationToken.None);
            }
        }
    }

    // -- Create Quote Dialog ----------------------------------------------------

    private async void CreateQuote_Click(object sender, RoutedEventArgs e)
    {
        // Feature gate check
        var featureGate = App.Services.GetRequiredService<Services.IFeatureGateService>();
        if (!await featureGate.CanCreateQuoteAsync())
        {
            var limitDialog = Helpers.DialogHelper.BuildBannerDialog(
                this.XamlRoot,
                "\uE1D0",
                "Quote Limit Reached",
                featureGate.GetUpgradeMessage("quotes"),
                closeButtonText: "OK",
                primaryButtonText: "Upgrade");
            await limitDialog.ShowAsync();
            return;
        }

        var titleBox = new TextBox
        {
            Header = "Quote Title",
            Text = "New Quote",
            PlaceholderText = "e.g. Kitchen Renovation",
        };
        var clientBox = new TextBox
        {
            Header = "Client Name (optional)",
            PlaceholderText = "Start typing a client name...",
        };

        var fieldsPanel = new StackPanel { Spacing = 16 };
        fieldsPanel.Children.Add(titleBox);
        fieldsPanel.Children.Add(clientBox);

        var dialog = Helpers.DialogHelper.BuildBannerDialogWithContent(
            this.XamlRoot,
            "\uE1D0",
            "New Quote",
            "Create a new quote for a client",
            fieldsPanel,
            primaryButtonText: "Create",
            closeButtonText: "Cancel");

        dialog.PrimaryButtonClick += (s, args) =>
        {
            var title = titleBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                args.Cancel = true;
                return;
            }
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var profileRepo = App.Services.GetRequiredService<IBusinessProfileRepository>();
            var quoteRepo = App.Services.GetRequiredService<IQuoteRepository>();
            var quoteNumGen = App.Services.GetRequiredService<QuoteNumberGenerator>();
            var clientRepo = App.Services.GetRequiredService<IClientRepository>();
            var navigator = App.Services.GetRequiredService<INavigator>();

            var profile = await profileRepo.GetAsync();
            var quoteNumber = await quoteNumGen.GenerateAsync();

            var quote = new QuoteEntity
            {
                Title = titleBox.Text?.Trim() ?? "New Quote",
                QuoteNumber = quoteNumber,
                Status = QuoteStatus.Draft,
                TaxRate = profile.DefaultTaxRate,
                ValidUntil = DateTimeOffset.UtcNow.AddDays(profile.QuoteValidDays),
            };

            // Match client if name was provided
            var clientName = clientBox.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(clientName))
            {
                quote.ClientName = clientName;
                var clients = await clientRepo.GetAllAsync();
                var match = clients.FirstOrDefault(c =>
                    c.Name.Equals(clientName, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                    quote.ClientId = match.Id;
            }

            await quoteRepo.SaveAsync(quote);
            await navigator.NavigateRouteAsync(this, "QuoteEditor", data: quote);
        }
    }

    // -- Inline Catalog Browser -------------------------------------------------

    private async void InlineCatalogBrowser_Click(object sender, RoutedEventArgs e)
    {
        List<CatalogItemEntity>? items = null;
        if (DataContext is DashboardModel model)
            items = await model.GetCatalogItemsAsync();

        if (items is null || items.Count == 0) return;

        var dialog = new CatalogBrowserDialog { XamlRoot = this.XamlRoot };
        dialog.LoadItems(items);
        dialog.ItemAdded += async catalogItem =>
        {
            if (DataContext is DashboardModel m)
                await m.AddInlineFromCatalog(catalogItem, CancellationToken.None);
        };
        await dialog.ShowAsync();
    }
}
