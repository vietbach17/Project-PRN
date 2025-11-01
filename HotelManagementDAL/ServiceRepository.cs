using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class ServiceRepository : IServiceRepository
{
    public async Task<IReadOnlyList<Service>> GetAllAsync(string connectionString, CancellationToken ct = default)
    {
        var list = new List<Service>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select ServiceId, Name, Price, Unit from Services order by Name";
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new Service
            {
                ServiceId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Price = rd.GetDecimal(2),
                Unit = rd.GetString(3)
            });
        }
        return list;
    }

    public async Task<Service?> GetByIdAsync(string connectionString, int serviceId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select ServiceId, Name, Price, Unit from Services where ServiceId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = serviceId });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await rd.ReadAsync(ct))
        {
            return new Service
            {
                ServiceId = rd.GetInt32(0),
                Name = rd.GetString(1),
                Price = rd.GetDecimal(2),
                Unit = rd.GetString(3)
            };
        }
        return null;
    }

    public async Task<int> AddAsync(string connectionString, Service service, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"insert into Services(Name, Price, Unit) values(@Name,@Price,@Unit); select cast(SCOPE_IDENTITY() as int);";
        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 150) { Value = service.Name });
        cmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = service.Price });
        cmd.Parameters.Add(new SqlParameter("@Unit", SqlDbType.NVarChar, 50) { Value = service.Unit });
        var id = (int)await cmd.ExecuteScalarAsync(ct);
        return id;
    }

    public async Task<bool> UpdateAsync(string connectionString, Service service, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"update Services set Name=@Name, Price=@Price, Unit=@Unit where ServiceId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = service.ServiceId });
        cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 150) { Value = service.Name });
        cmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = service.Price });
        cmd.Parameters.Add(new SqlParameter("@Unit", SqlDbType.NVarChar, 50) { Value = service.Unit });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string connectionString, int serviceId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"delete from Services where ServiceId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = serviceId });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}
