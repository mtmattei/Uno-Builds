using UnoEnterpriseApp.Models;

namespace UnoEnterpriseApp.Contracts;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(string id);
    Task<IEnumerable<Customer>> SearchAsync(string searchTerm);
    Task<Customer> SaveAsync(Customer customer);
    Task DeleteAsync(string id);
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
}