using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class HousekeepingService : IHousekeepingService
{
    private readonly IHousekeepingRepository _repo;
    public HousekeepingService(IHousekeepingRepository repo) { _repo = repo; }
    public HousekeepingService() { _repo = new HousekeepingRepository(); }
    public Task<IReadOnlyList<HousekeepingTask>> GetAllAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);
    public Task<HousekeepingTask?> GetByIdAsync(string connectionString, int taskId, CancellationToken ct = default)
        => _repo.GetByIdAsync(connectionString, taskId, ct);
    public Task<int> AddAsync(string connectionString, HousekeepingTask task, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.AddAsync(connectionString, task, ct);
    }
    public Task<bool> UpdateAsync(string connectionString, HousekeepingTask task, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.UpdateAsync(connectionString, task, ct);
    }
    public Task<bool> DeleteAsync(string connectionString, int taskId, CancellationToken ct = default)
    {
        Authorization.EnsureCanDeleteEntity();
        return _repo.DeleteAsync(connectionString, taskId, ct);
    }
}
