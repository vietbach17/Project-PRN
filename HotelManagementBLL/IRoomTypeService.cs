using HotelManagementModels;

namespace HotelManagementBLL;

public interface IRoomTypeService
{
    Task<IReadOnlyList<RoomType>> GetAllAsync(string connectionString, CancellationToken ct = default);
    Task<RoomType?> GetByIdAsync(string connectionString, int roomTypeId, CancellationToken ct = default);
    Task<int> AddAsync(string connectionString, RoomType roomType, CancellationToken ct = default);
    Task<bool> UpdateAsync(string connectionString, RoomType roomType, CancellationToken ct = default);
    Task<bool> DeleteAsync(string connectionString, int roomTypeId, CancellationToken ct = default);
}
