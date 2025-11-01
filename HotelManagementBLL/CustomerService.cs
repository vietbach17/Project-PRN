using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;
    public CustomerService(ICustomerRepository repo) { _repo = repo; }
    public CustomerService() { _repo = new CustomerRepository(); }
    public Task<IReadOnlyList<Customer>> GetAllAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);
    public Task<Customer?> GetByIdAsync(string connectionString, int customerId, CancellationToken ct = default)
        => _repo.GetByIdAsync(connectionString, customerId, ct);
    public Task<int> AddAsync(string connectionString, Customer customer, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.AddAsync(connectionString, customer, ct);
    }
    public Task<int> AddForRegistrationAsync(string connectionString, Customer customer, CancellationToken ct = default)
    {
        // No guard: registration occurs before login
        return _repo.AddAsync(connectionString, customer, ct);
    }
    public Task<bool> UpdateAsync(string connectionString, Customer customer, CancellationToken ct = default)
    {
        Authorization.EnsureCanUpdateCustomer(customer.CustomerId);
        return _repo.UpdateAsync(connectionString, customer, ct);
    }
    public Task<bool> DeleteAsync(string connectionString, int customerId, CancellationToken ct = default)
    {
        Authorization.EnsureCanDeleteEntity();
        return _repo.DeleteAsync(connectionString, customerId, ct);
    }
}
