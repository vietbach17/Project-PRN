using HotelManagementModels;

namespace HotelManagementBLL;

public interface IAuthService
{
    Task<User?> LoginAsync(string connectionString, string username, string password, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(string connectionString, string username, string currentPassword, string newPassword, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(string connectionString, string username, string idNumber, string newPassword, CancellationToken ct = default);
    Task<string?> FindUsernameByIdNumberAsync(string connectionString, string idNumber, CancellationToken ct = default);
}
