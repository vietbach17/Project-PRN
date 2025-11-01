using HotelManagementModels;

namespace HotelManagementBLL;

public interface IAuthService
{
    Task<User?> LoginAsync(string connectionString, string username, string password, CancellationToken ct = default);
}
