using HotelManagementModels;

namespace HotelManagementBLL;

public interface IHousekeepingService
{
    Task<IReadOnlyList<HousekeepingTask>> GetAllAsync(string connectionString, CancellationToken ct = default);
    Task<HousekeepingTask?> GetByIdAsync(string connectionString, int taskId, CancellationToken ct = default);
    Task<int> AddAsync(string connectionString, HousekeepingTask task, CancellationToken ct = default);
    Task<bool> UpdateAsync(string connectionString, HousekeepingTask task, CancellationToken ct = default);
    Task<bool> DeleteAsync(string connectionString, int taskId, CancellationToken ct = default);
}
