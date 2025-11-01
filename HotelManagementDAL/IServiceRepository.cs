using HotelManagementModels;

namespace HotelManagementDAL;

public interface IServiceRepository
{
    Task<IReadOnlyList<Service>> GetAllAsync(string connectionString, CancellationToken ct = default);
    Task<Service?> GetByIdAsync(string connectionString, int serviceId, CancellationToken ct = default);
    Task<int> AddAsync(string connectionString, Service service, CancellationToken ct = default);
    Task<bool> UpdateAsync(string connectionString, Service service, CancellationToken ct = default);
    Task<bool> DeleteAsync(string connectionString, int serviceId, CancellationToken ct = default);
}
