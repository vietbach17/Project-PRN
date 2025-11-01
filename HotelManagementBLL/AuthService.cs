using HotelManagementDAL;
using HotelManagementModels;
using System.Security.Cryptography;
using System.Text;

namespace HotelManagementBLL;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    public AuthService(IUserRepository users) { _users = users; }
    public AuthService() { _users = new UserRepository(); }

    public async Task<User?> LoginAsync(string connectionString, string username, string password, CancellationToken ct = default)
    {
        var u = await _users.GetByUsernameAsync(connectionString, username, ct);
        if (u == null) return null;
        if (!u.IsActive) return null;
        if (!VerifyPassword(u.PasswordHash, password)) return null;
        return u;
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
