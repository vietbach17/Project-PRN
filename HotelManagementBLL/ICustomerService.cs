using HotelManagementModels;

namespace HotelManagementBLL;

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync(string connectionString, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(string connectionString, int customerId, CancellationToken ct = default);
    Task<int> AddAsync(string connectionString, Customer customer, CancellationToken ct = default);
    Task<int> AddForRegistrationAsync(string connectionString, Customer customer, CancellationToken ct = default);
    Task<bool> UpdateAsync(string connectionString, Customer customer, CancellationToken ct = default);
    Task<bool> DeleteAsync(string connectionString, int customerId, CancellationToken ct = default);
}
