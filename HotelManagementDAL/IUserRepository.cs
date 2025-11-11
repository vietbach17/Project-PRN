using HotelManagementModels;

namespace HotelManagementDAL;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string connectionString, string username, CancellationToken ct = default);
    Task<User?> GetByCustomerIdNumberAsync(string connectionString, string idNumber, CancellationToken ct = default);
    Task<bool> UpdateUsernameAsync(string connectionString, int userId, string newUsername, CancellationToken ct = default);
    Task<bool> UpdatePasswordHashAsync(string connectionString, int userId, string newPasswordHash, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string connectionString, string username, CancellationToken ct = default);
    Task<int> AddAsync(string connectionString, User user, CancellationToken ct = default);
}
