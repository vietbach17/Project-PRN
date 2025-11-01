using HotelManagementModels;

namespace HotelManagementBLL;

public interface IRoomService
{
    Task<IReadOnlyList<Room>> GetRoomsAsync(string connectionString, CancellationToken ct = default);
    Task<Room?> GetByIdAsync(string connectionString, int roomId, CancellationToken ct = default);
    Task<int> AddAsync(string connectionString, Room room, CancellationToken ct = default);
    Task<bool> UpdateAsync(string connectionString, Room room, CancellationToken ct = default);
    Task<bool> DeleteAsync(string connectionString, int roomId, CancellationToken ct = default);
}
