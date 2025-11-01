using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class UserRepository : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string connectionString, string username, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select UserId, Username, PasswordHash, Role, IsActive, CustomerId from Users where Username=@u";
        cmd.Parameters.Add(new SqlParameter("@u", SqlDbType.NVarChar, 100) { Value = username });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await rd.ReadAsync(ct))
        {
            return new User
            {
                UserId = rd.GetInt32(0),
                Username = rd.GetString(1),
                PasswordHash = rd.GetString(2),
                Role = rd.GetString(3),
                IsActive = rd.GetBoolean(4),
                CustomerId = await rd.IsDBNullAsync(5, ct) ? null : rd.GetInt32(5)
            };
        }
        return null;
    }

    public async Task<bool> UpdateUsernameAsync(string connectionString, int userId, string newUsername, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"update Users set Username=@u where UserId=@id";
        cmd.Parameters.Add(new SqlParameter("@u", SqlDbType.NVarChar, 100) { Value = newUsername });
        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = userId });
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0;
    }

    public async Task<bool> UpdatePasswordHashAsync(string connectionString, int userId, string newPasswordHash, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"update Users set PasswordHash=@p where UserId=@id";
        cmd.Parameters.Add(new SqlParameter("@p", SqlDbType.NVarChar, 256) { Value = newPasswordHash });
        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = userId });
        var n = await cmd.ExecuteNonQueryAsync(ct);
        return n > 0;
    }

    public async Task<bool> UsernameExistsAsync(string connectionString, string username, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select count(*) from Users where Username=@u";
        cmd.Parameters.Add(new SqlParameter("@u", SqlDbType.NVarChar, 100) { Value = username });
        var count = (int)await cmd.ExecuteScalarAsync(ct);
        return count > 0;
    }

    public async Task<int> AddAsync(string connectionString, User user, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"insert into Users(Username, PasswordHash, Role, IsActive, CustomerId)
values(@u, @p, @r, @a, @c); select cast(SCOPE_IDENTITY() as int);";
        cmd.Parameters.Add(new SqlParameter("@u", SqlDbType.NVarChar, 100) { Value = user.Username });
        cmd.Parameters.Add(new SqlParameter("@p", SqlDbType.NVarChar, 256) { Value = user.PasswordHash ?? string.Empty });
        cmd.Parameters.Add(new SqlParameter("@r", SqlDbType.NVarChar, 50) { Value = user.Role });
        cmd.Parameters.Add(new SqlParameter("@a", SqlDbType.Bit) { Value = user.IsActive });
        cmd.Parameters.Add(new SqlParameter("@c", SqlDbType.Int) { Value = (object?)user.CustomerId ?? DBNull.Value });
        var id = (int)await cmd.ExecuteScalarAsync(ct);
        return id;
    }
}
