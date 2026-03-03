using UnoEnterpriseApp.Contracts;
using UnoEnterpriseApp.Models;

namespace UnoEnterpriseApp.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(ILogger<CustomerRepository> logger)
    {
        _logger = logger;
        _customers = GenerateSampleCustomers();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all customers");
        await Task.Delay(100); // Simulate async operation
        return _customers.ToList();
    }

    public async Task<Customer?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
        await Task.Delay(50);
        return _customers.FirstOrDefault(c => c.Id == id);
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
    {
        _logger.LogInformation("Searching customers with term: {SearchTerm}", searchTerm);
        await Task.Delay(200);
        
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        return _customers.Where(c =>
            c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.Company.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.Id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }

    public async Task<Customer> SaveAsync(Customer customer)
    {
        _logger.LogInformation("Saving customer: {CustomerId}", customer.Id);
        await Task.Delay(500); // Simulate async save operation
        
        var existingIndex = _customers.FindIndex(c => c.Id == customer.Id);
        if (existingIndex >= 0)
        {
            _customers[existingIndex] = customer;
        }
        else
        {
            _customers.Add(customer);
        }
        
        return customer;
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting customer: {CustomerId}", id);
        await Task.Delay(200);
        
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        if (customer != null)
        {
            _customers.Remove(customer);
        }
    }

    public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status)
    {
        _logger.LogInformation("Retrieving customers with status: {Status}", status);
        await Task.Delay(100);
        
        return _customers.Where(c => c.Status == status).ToList();
    }

    private static List<Customer> GenerateSampleCustomers()
    {
        var random = new Random(42); // Fixed seed for consistent demo data
        var customers = new List<Customer>();

        // Sample data arrays for realistic variety
        var firstNames = new[] { "Emma", "Liam", "Olivia", "Noah", "Ava", "Ethan", "Sophia", "Mason", "Isabella", "William",
            "Mia", "James", "Charlotte", "Benjamin", "Amelia", "Lucas", "Harper", "Henry", "Evelyn", "Alexander",
            "Madison", "Sebastian", "Avery", "Jack", "Ella", "Owen", "Scarlett", "Samuel", "Grace", "Matthew",
            "Chloe", "Joseph", "Victoria", "David", "Riley", "Carter", "Aria", "Wyatt", "Lily", "John" };
        
        var lastNames = new[] { "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez",
            "Lopez", "Gonzales", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee",
            "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker",
            "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green" };
        
        var companies = new[] { 
            "Acme Corporation", "TechVision Systems", "Global Dynamics", "Pinnacle Solutions", "NextGen Industries",
            "Quantum Innovations", "Apex Manufacturing", "Stellar Logistics", "Prime Consulting Group", "Fusion Technologies",
            "Summit Financial Services", "Horizon Pharmaceuticals", "Velocity Marketing", "Atlas Engineering", "Phoenix Enterprises",
            "Titanium Software", "Omega Healthcare", "Neptune Shipping", "Eclipse Media Group", "Zenith Retail",
            "Aurora Biotech", "Cascade Energy", "Vertex Construction", "Nimbus Cloud Services", "Meridian Aerospace",
            "Orion Telecommunications", "Catalyst Chemical Co.", "Synergy Partners", "Vanguard Security", "Evergreen Foods",
            "Silverline Automotive", "BlueSky Airlines", "Crossroads Logistics", "Diamond Insurance", "Eagle Eye Analytics",
            "Fortress Banking", "Gateway Electronics", "Harmony Real Estate", "Innovate AI Labs", "Jupiter Mining Co."
        };

        var departments = new[] { "", "Sales", "IT", "Operations", "Finance", "Marketing", "HR", "R&D", "Legal", "Procurement" };
        
        var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose",
            "Austin", "Jacksonville", "Fort Worth", "Columbus", "Charlotte", "San Francisco", "Indianapolis", "Seattle", "Denver", "Boston",
            "Washington", "Nashville", "El Paso", "Detroit", "Portland", "Memphis", "Oklahoma City", "Las Vegas", "Louisville", "Baltimore" };
        
        var notes = new[] {
            "VIP customer. Requires approval for orders over $50k. Preferred payment terms: Net 30.",
            "Key account - quarterly business reviews scheduled. Primary contact prefers email communication.",
            "New customer onboarding in progress. Credit check completed successfully.",
            "Long-standing partner since 2015. Eligible for volume discounts.",
            "High-growth potential. Expanding operations in Q4 2025.",
            "Requires specialized support team. Technical documentation needed for all shipments.",
            "Strategic account with custom SLA. 24/7 support enabled.",
            "Pending contract renewal. Decision maker change in progress.",
            "Excellent payment history. Approved for extended credit terms.",
            "Pilot program participant. Providing feedback on new features.",
            "Regional distributor for West Coast. Warehouse locations in 3 states.",
            "Government contractor - special compliance requirements apply.",
            "Seasonal business with peak orders in Q4. Plan capacity accordingly.",
            "Recently merged with competitor. Integration in progress.",
            "Beta testing partner for new product line. NDA on file.",
            "International operations in 12 countries. Multi-currency billing enabled.",
            "Sustainability focus - requires carbon footprint reporting.",
            "Custom integration with ERP system completed. API access granted.",
            "Premium support tier. Dedicated account manager assigned.",
            "Referral partner - 5% commission on new business."
        };

        // Generate 50 diverse customers
        for (int i = 0; i < 50; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var company = companies[i % companies.Length];
            var dept = departments[random.Next(departments.Length)];
            var city = cities[random.Next(cities.Length)];
            
            // Generate varied financial data
            var revenue = random.Next(10, 500) * 1000;
            if (i % 7 == 0) revenue *= 10; // Some high-value customers
            
            var creditLimit = revenue * random.Next(2, 6);
            
            // Varied dates for more realistic data
            var lastOrderDays = random.Next(1, 180);
            var lastOrder = DateTime.Now.AddDays(-lastOrderDays);
            
            // Status distribution: 60% active, 15% pending, 15% inactive, 10% suspended
            var statusRoll = random.Next(100);
            var status = statusRoll < 60 ? CustomerStatus.Active :
                        statusRoll < 75 ? CustomerStatus.Pending :
                        statusRoll < 90 ? CustomerStatus.Inactive :
                        CustomerStatus.Suspended;
            
            var customer = new Customer
            {
                Id = $"C{(10000 + i):00000}",
                FirstName = firstName,
                LastName = lastName,
                Name = $"{firstName} {lastName}",
                Company = company,
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@{company.Replace(" ", "").ToLower()}.com",
                Phone = $"({300 + random.Next(700)}) {random.Next(100, 999)}-{random.Next(1000, 9999)}",
                Status = status,
                LastOrder = lastOrder,
                Revenue = revenue,
                CreditLimit = creditLimit,
                Notes = notes[random.Next(notes.Length)]
            };
            
            customers.Add(customer);
        }
        
        return customers.OrderBy(c => c.Company).ThenBy(c => c.Name).ToList();
    }
}