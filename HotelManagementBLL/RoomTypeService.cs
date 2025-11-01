using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _repo;
    public RoomTypeService(IRoomTypeRepository repo) { _repo = repo; }
    public RoomTypeService() { _repo = new RoomTypeRepository(); }
    public Task<IReadOnlyList<RoomType>> GetAllAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);
    public Task<RoomType?> GetByIdAsync(string connectionString, int roomTypeId, CancellationToken ct = default)
        => _repo.GetByIdAsync(connectionString, roomTypeId, ct);
    public Task<int> AddAsync(string connectionString, RoomType roomType, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.AddAsync(connectionString, roomType, ct);
    }
    public Task<bool> UpdateAsync(string connectionString, RoomType roomType, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.UpdateAsync(connectionString, roomType, ct);
    }
    public Task<bool> DeleteAsync(string connectionString, int roomTypeId, CancellationToken ct = default)
    {
        Authorization.EnsureCanDeleteEntity();
        return _repo.DeleteAsync(connectionString, roomTypeId, ct);
    }
}
