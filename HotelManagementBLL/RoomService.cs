using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repo;

    public RoomService(IRoomRepository repo)
    {
        _repo = repo;
    }

    public RoomService()
    {
        _repo = new RoomRepository();
    }

    public Task<IReadOnlyList<Room>> GetRoomsAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);

    public Task<Room?> GetByIdAsync(string connectionString, int roomId, CancellationToken ct = default)
        => _repo.GetByIdAsync(connectionString, roomId, ct);

    public Task<int> AddAsync(string connectionString, Room room, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.AddAsync(connectionString, room, ct);
    }

    public Task<bool> UpdateAsync(string connectionString, Room room, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddOrUpdateEntity();
        return _repo.UpdateAsync(connectionString, room, ct);
    }

    public Task<bool> DeleteAsync(string connectionString, int roomId, CancellationToken ct = default)
    {
        Authorization.EnsureCanDeleteEntity();
        return _repo.DeleteAsync(connectionString, roomId, ct);
    }
}
