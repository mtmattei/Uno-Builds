using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UnoEnterpriseApp.Models;

public partial class Customer : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Customer ID is required")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Customer ID must be between 3 and 10 characters")]
    private string id = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    private string name = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Company is required")]
    [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
    private string company = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    private string email = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    private string phone = string.Empty;

    [ObservableProperty]
    private CustomerStatus status = CustomerStatus.Active;

    [ObservableProperty]
    private DateTime lastOrder = DateTime.Now;

    [ObservableProperty]
    private decimal revenue;

    [ObservableProperty]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    private string firstName = string.Empty;

    [ObservableProperty]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    private string lastName = string.Empty;

    [ObservableProperty]
    [Range(0, 10000000, ErrorMessage = "Credit limit must be between $0 and $10,000,000")]
    private decimal creditLimit = 100000;

    [ObservableProperty]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    private string notes = "VIP customer. Requires approval for orders over $50k. Preferred payment terms: Net 30.";

    // Computed properties for display
    public string FormattedRevenue => $"${Revenue:N0}";
    public string FormattedLastOrder => LastOrder.ToString("yyyy-MM-dd");
    public string FormattedCreditLimit => $"${CreditLimit:N2}";
    
    // Get initials for avatar display
    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
            {
                return Name.Length > 0 ? Name.Substring(0, 1).ToUpper() : "?";
            }
            var firstInitial = !string.IsNullOrWhiteSpace(FirstName) ? FirstName[0].ToString().ToUpper() : "";
            var lastInitial = !string.IsNullOrWhiteSpace(LastName) ? LastName[0].ToString().ToUpper() : "";
            return $"{firstInitial}{lastInitial}";
        }
    }
    
    // Generate a color based on the customer name for avatar background
    public string AvatarColor
    {
        get
        {
            var colors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FFEAA7", "#DDA0DD", "#98D8C8", "#F7DC6F", "#85C1E2", "#F8B739" };
            var hash = Name.GetHashCode();
            return colors[Math.Abs(hash) % colors.Length];
        }
    }

    public Customer()
    {
        // Parse name into first and last when name is set
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Name) && !string.IsNullOrEmpty(Name))
            {
                var parts = Name.Split(' ', 2);
                FirstName = parts[0];
                LastName = parts.Length > 1 ? parts[1] : string.Empty;
            }
        };
    }
}

public enum CustomerStatus
{
    Active,
    Inactive,
    Pending,
    Suspended
}
