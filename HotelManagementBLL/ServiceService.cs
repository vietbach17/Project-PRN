using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repo;
    public ServiceService(IServiceRepository repo) { _repo = repo; }
    public ServiceService() { _repo = new ServiceRepository(); }
    public Task<IReadOnlyList<Service>> GetAllAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);
    public Task<Service?> GetByIdAsync(string connectionString, int serviceId, CancellationToken ct = default)
        => _repo.GetByIdAsync(connectionString, serviceId, ct);
    public Task<int> AddAsync(string connectionString, Service service, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.AddAsync(connectionString, service, ct);
    }
    public Task<bool> UpdateAsync(string connectionString, Service service, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.UpdateAsync(connectionString, service, ct);
    }
    public Task<bool> DeleteAsync(string connectionString, int serviceId, CancellationToken ct = default)
    {
        Authorization.EnsureCanDeleteEntity();
        return _repo.DeleteAsync(connectionString, serviceId, ct);
    }
}
