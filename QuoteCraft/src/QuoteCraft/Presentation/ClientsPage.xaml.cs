using Microsoft.UI.Xaml.Input;

namespace QuoteCraft.Presentation;

public sealed partial class ClientsPage : Page
{
    private IClientRepository? _clientRepo;
    private ClientEntity? _selectedClientEntity;

    public ClientsPage()
    {
        this.InitializeComponent();
    }

    private IClientRepository ClientRepo =>
        _clientRepo ??= App.Services.GetRequiredService<IClientRepository>();

    private async void ClientCard_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ClientDisplayItem client } && DataContext is ClientsModel model)
        {
            _selectedClientEntity = client.Entity;
            await model.SelectClient(client, CancellationToken.None);
        }
    }

    private async void AddClient_Click(object sender, RoutedEventArgs e)
    {
        var featureGate = App.Services.GetRequiredService<IFeatureGateService>();
        if (!await featureGate.CanAddClientAsync())
        {
            var limitDialog = Helpers.DialogHelper.BuildBannerDialog(
                this.XamlRoot,
                "\uE77B",
                "Client Limit Reached",
                featureGate.GetUpgradeMessage("clients"),
                closeButtonText: "OK",
                primaryButtonText: "Upgrade");
            await limitDialog.ShowAsync();
            return;
        }

        await ShowClientEditorDialog(new ClientEntity());
    }

    private async void EditClient_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedClientEntity is null) return;

        // Re-fetch to get latest data
        var fresh = await ClientRepo.GetByIdAsync(_selectedClientEntity.Id);
        if (fresh is not null)
            await ShowClientEditorDialog(fresh);
    }

    private async Task ShowClientEditorDialog(ClientEntity entity)
    {
        var isNew = string.IsNullOrEmpty(entity.Id);

        var nameBox = new TextBox
        {
            Header = "Name",
            Text = entity.Name ?? string.Empty,
            PlaceholderText = "Client or business name",
        };
        var emailBox = new TextBox
        {
            Header = "Email",
            Text = entity.Email ?? string.Empty,
            PlaceholderText = "email@example.com",
            InputScope = new InputScope { Names = { new InputScopeName(InputScopeNameValue.EmailSmtpAddress) } },
        };
        var phoneBox = new TextBox
        {
            Header = "Phone",
            Text = entity.Phone ?? string.Empty,
            PlaceholderText = "(555) 123-4567",
            InputScope = new InputScope { Names = { new InputScopeName(InputScopeNameValue.TelephoneNumber) } },
        };
        var addressBox = new TextBox
        {
            Header = "Address",
            Text = entity.Address ?? string.Empty,
            PlaceholderText = "Street, City, State/Province",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 80,
        };

        var fieldsPanel = new StackPanel { Spacing = 16 };
        fieldsPanel.Children.Add(nameBox);
        fieldsPanel.Children.Add(emailBox);
        fieldsPanel.Children.Add(phoneBox);
        fieldsPanel.Children.Add(addressBox);

        var dialog = Helpers.DialogHelper.BuildBannerDialogWithContent(
            this.XamlRoot,
            "\uE77B",
            isNew ? "Add Client" : "Edit Client",
            isNew ? "Add a new client to your contact list" : "Update client details",
            fieldsPanel,
            primaryButtonText: "Save",
            closeButtonText: "Cancel");

        dialog.PrimaryButtonClick += async (s, args) =>
        {
            var name = nameBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                args.Cancel = true;
                return;
            }

            entity.Name = name;
            entity.Email = emailBox.Text;
            entity.Phone = phoneBox.Text;
            entity.Address = addressBox.Text;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await ClientRepo.SaveAsync(entity);
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            RefreshClients();
        }
    }

    private void RefreshClients()
    {
        if (DataContext is ClientsModel model)
            _ = model.RefreshList(CancellationToken.None);
    }

    private async void DeleteClient_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ClientsModel model) return;

        var dialog = Helpers.DialogHelper.BuildBannerDialog(
            this.XamlRoot,
            "\uE74D",
            "Delete Client?",
            "This client will be permanently removed from your contact list.",
            closeButtonText: "Cancel",
            primaryButtonText: "Delete");

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await model.DeleteClient(CancellationToken.None);
            _selectedClientEntity = null;
        }
    }

    private async void NewQuoteForClient_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClientsModel model)
            await model.CreateQuoteForClient(CancellationToken.None);
    }
}
