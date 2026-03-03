using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnoEnterpriseApp.Contracts;
using UnoEnterpriseApp.Models;
using UnoEnterpriseApp.Services;

namespace UnoEnterpriseApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidationService _validationService;
    
    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();

    [ObservableProperty]
    private Customer? selectedCustomer;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isAllCustomersChecked = true;

    [ObservableProperty]
    private bool isActiveAccountsChecked;

    [ObservableProperty]
    private bool isPendingApprovalsChecked;

    [ObservableProperty]
    private string selectedTab = "CustomerList";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private string validationErrors = string.Empty;

    public MainViewModel(ICustomerRepository customerRepository, IValidationService validationService)
    {
        _customerRepository = customerRepository;
        _validationService = validationService;
        _ = InitializeCustomersAsync();
    }

    private async Task InitializeCustomersAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading customers...";
            
            var customers = await _customerRepository.GetAllAsync();
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
            
            // Select first customer by default so the details panel always shows data
            SelectedCustomer = Customers.FirstOrDefault();
            
            // Set default notes if the selected customer exists
            if (SelectedCustomer != null && string.IsNullOrEmpty(SelectedCustomer.Notes))
            {
                SelectedCustomer.Notes = "VIP customer. Requires approval for orders over $50k. Preferred payment terms: Net 30.";
            }
            
            StatusMessage = "Ready";
            OnPropertyChanged(nameof(StatusBarInfo));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load customers: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Searching customers...";
            
            // Simulate async operation
            await Task.Delay(500);
            
            // Use repository for search
            var searchResults = await _customerRepository.SearchAsync(SearchText);
            
            Customers.Clear();
            foreach (var customer in searchResults)
            {
                Customers.Add(customer);
            }
            
            StatusMessage = $"Found {searchResults.Count()} customers";
            OnPropertyChanged(nameof(StatusBarInfo));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AdvancedFilterAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Applying advanced filters...";
            
            // Simulate async operation
            await Task.Delay(300);
            
            // TODO: Implement advanced filter dialog
            StatusMessage = "Advanced filter not implemented yet";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Saving changes...";
            
            if (SelectedCustomer != null)
            {
                // Validate before saving
                if (!_validationService.IsValid(SelectedCustomer))
                {
                    var validationResult = _validationService.ValidateObject(SelectedCustomer);
                    ValidationErrors = validationResult.ErrorMessage ?? "Validation failed";
                    StatusMessage = "Please correct validation errors before saving";
                    return;
                }
                
                ValidationErrors = string.Empty;
                
                // Use repository to save
                await _customerRepository.SaveAsync(SelectedCustomer);
                
                // Update the collection
                var index = Customers.IndexOf(SelectedCustomer);
                if (index >= 0)
                {
                    Customers[index] = SelectedCustomer;
                }
                
                StatusMessage = "Changes saved successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Cancelling changes...";
            
            // Simulate async operation
            await Task.Delay(200);
            
            // Reload customer from repository
            if (SelectedCustomer != null)
            {
                var originalCustomer = await _customerRepository.GetByIdAsync(SelectedCustomer.Id);
                if (originalCustomer != null)
                {
                    SelectedCustomer = originalCustomer;
                }
            }
            
            StatusMessage = "Changes cancelled";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        // Update related properties when selection changes
        OnPropertyChanged(nameof(SelectedCustomer));
    }

    public string StatusBarInfo
    {
        get
        {
            var totalRevenue = Customers.Sum(c => c.Revenue);
            var activeCount = Customers.Count(c => c.Status == CustomerStatus.Active);
            return $"{Customers.Count} customers • {activeCount} active • ${totalRevenue:N0} total revenue";
        }
    }
}
