using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class RoomTypeRepository : IRoomTypeRepository
{
    public async Task<IReadOnlyList<RoomType>> GetAllAsync(string connectionString, CancellationToken ct = default)
    {
        var list = new List<RoomType>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select RoomTypeId, Name, Capacity, BasePrice, Description, CreatedAt from RoomTypes order by Name";
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new RoomType
            {
                RoomTypeId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Capacity = rd.GetInt32(2),
                BasePrice = rd.GetDecimal(3),
                Description = await rd.IsDBNullAsync(4, ct) ? null : rd.GetString(4),
                CreatedAt = rd.GetDateTime(5)
            });
        }
        return list;
    }

    public async Task<RoomType?> GetByIdAsync(string connectionString, int roomTypeId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select RoomTypeId, Name, Capacity, BasePrice, Description, CreatedAt from RoomTypes where RoomTypeId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = roomTypeId });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await rd.ReadAsync(ct))
        {
            return new RoomType
            {
                RoomTypeId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Capacity = rd.GetInt32(2),
                BasePrice = rd.GetDecimal(3),
                Description = await rd.IsDBNullAsync(4, ct) ? null : rd.GetString(4),
                CreatedAt = rd.GetDateTime(5)
            };
        }
        return null;
    }

    public async Task<int> AddAsync(string connectionString, RoomType roomType, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"insert into RoomTypes(Name, Capacity, BasePrice, Description, CreatedAt)
values(@Name,@Capacity,@BasePrice,@Description,SYSDATETIME());
select cast(SCOPE_IDENTITY() as int);";
        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = roomType.Name });
        cmd.Parameters.Add(new SqlParameter("@Capacity", SqlDbType.Int) { Value = roomType.Capacity });
        var p = new SqlParameter("@BasePrice", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = roomType.BasePrice };
        cmd.Parameters.Add(p);
        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 300) { Value = (object?)roomType.Description ?? DBNull.Value });
        var id = (int)await cmd.ExecuteScalarAsync(ct);
        return id;
    }

    public async Task<bool> UpdateAsync(string connectionString, RoomType roomType, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"update RoomTypes set Name=@Name, Capacity=@Capacity, BasePrice=@BasePrice, Description=@Description where RoomTypeId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = roomType.RoomTypeId });
        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = roomType.Name });
        cmd.Parameters.Add(new SqlParameter("@Capacity", SqlDbType.Int) { Value = roomType.Capacity });
        var p = new SqlParameter("@BasePrice", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = roomType.BasePrice };
        cmd.Parameters.Add(p);
        cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 300) { Value = (object?)roomType.Description ?? DBNull.Value });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string connectionString, int roomTypeId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"delete from RoomTypes where RoomTypeId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = roomTypeId });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}
