using HotelManagementDAL;
using HotelManagementModels;
using System.Security.Cryptography;
using System.Text;

namespace HotelManagementBLL;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ICustomerRepository _customers;
    public AuthService(IUserRepository users, ICustomerRepository customers)
    {
        _users = users;
        _customers = customers;
    }
    public AuthService()
    {
        _users = new UserRepository();
        _customers = new CustomerRepository();
    }

    public async Task<User?> LoginAsync(string connectionString, string username, string password, CancellationToken ct = default)
    {
        var u = await _users.GetByUsernameAsync(connectionString, username, ct);
        if (u == null) return null;
        if (!u.IsActive) return null;
        if (!VerifyPassword(u.PasswordHash, password)) return null;
        return u;
    }

    public async Task<bool> ChangePasswordAsync(string connectionString, string username, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            return false;
        var user = await _users.GetByUsernameAsync(connectionString, username.Trim(), ct);
        if (user == null || !user.IsActive) return false;
        if (!VerifyPassword(user.PasswordHash, currentPassword)) return false;
        var hash = ComputeSha256Hex(newPassword);
        return await _users.UpdatePasswordHashAsync(connectionString, user.UserId, hash, ct);
    }

    public async Task<bool> ResetPasswordAsync(string connectionString, string username, string idNumber, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(idNumber) || string.IsNullOrEmpty(newPassword))
            return false;

        var user = await _users.GetByUsernameAsync(connectionString, username.Trim(), ct);
        if (user == null || !user.IsActive || user.CustomerId is null)
            return false;

        var customer = await _customers.GetByIdAsync(connectionString, user.CustomerId.Value, ct);
        if (customer == null)
            return false;

        bool match = string.Equals(customer.IDNumber?.Trim(), idNumber.Trim(), StringComparison.OrdinalIgnoreCase);
        if (!match)
            return false;

        var hash = ComputeSha256Hex(newPassword);
        return await _users.UpdatePasswordHashAsync(connectionString, user.UserId, hash, ct);
    }

    public async Task<string?> FindUsernameByIdNumberAsync(string connectionString, string idNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idNumber)) return null;
        var user = await _users.GetByCustomerIdNumberAsync(connectionString, idNumber.Trim(), ct);
        if (user == null || !user.IsActive) return null;
        return user.Username;
    }

    private static bool VerifyPassword(string stored, string input)
    {
        if (LooksLikeSha256(stored?.Trim() ?? string.Empty))
        {
            string hash = ComputeSha256Hex(input ?? string.Empty);
            return string.Equals((stored ?? string.Empty).Trim(), hash, StringComparison.OrdinalIgnoreCase);
        }
        return string.Equals((stored ?? string.Empty).Trim(), (input ?? string.Empty).Trim());
    }

    private static bool LooksLikeSha256(string s)
        => s is not null && s.Length == 64 && IsHex(s);

    private static bool IsHex(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            bool ok = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
            if (!ok) return false;
        }
        return true;
    }

    private static string ComputeSha256Hex(string input)
    {
        using var sha = SHA256.Create();
        byte[] data = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(data.Length * 2);
        foreach (var b in data) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
