using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class CustomerRepository : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(string connectionString, CancellationToken ct = default)
    {
        var list = new List<Customer>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select CustomerId, FullName, Phone, Email, IDNumber, Address, CreatedAt from Customers order by FullName";
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new Customer
            {
                CustomerId = rd.GetInt32(0),
                FullName = rd.GetString(1),
                Phone = await rd.IsDBNullAsync(2, ct) ? null : rd.GetString(2),
                Email = await rd.IsDBNullAsync(3, ct) ? null : rd.GetString(3),
                IDNumber = await rd.IsDBNullAsync(4, ct) ? null : rd.GetString(4),
                Address = await rd.IsDBNullAsync(5, ct) ? null : rd.GetString(5),
                CreatedAt = rd.GetDateTime(6)
            });
        }
        return list;
    }

    public async Task<Customer?> GetByIdAsync(string connectionString, int customerId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select CustomerId, FullName, Phone, Email, IDNumber, Address, CreatedAt from Customers where CustomerId = @id";
        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = customerId });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await rd.ReadAsync(ct))
        {
            return new Customer
            {
                CustomerId = rd.GetInt32(0),
                FullName = rd.GetString(1),
                Phone = await rd.IsDBNullAsync(2, ct) ? null : rd.GetString(2),
                Email = await rd.IsDBNullAsync(3, ct) ? null : rd.GetString(3),
                IDNumber = await rd.IsDBNullAsync(4, ct) ? null : rd.GetString(4),
                Address = await rd.IsDBNullAsync(5, ct) ? null : rd.GetString(5),
                CreatedAt = rd.GetDateTime(6)
            };
        }
        return null;
    }

    public async Task<int> AddAsync(string connectionString, Customer customer, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"insert into Customers(FullName, Phone, Email, IDNumber, Address, CreatedAt)
values(@FullName,@Phone,@Email,@IDNumber,@Address,SYSDATETIME());
select cast(SCOPE_IDENTITY() as int);";
        cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = customer.FullName });
        cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 30) { Value = (object?)customer.Phone ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = (object?)customer.Email ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IDNumber", SqlDbType.NVarChar, 50) { Value = (object?)customer.IDNumber ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 300) { Value = (object?)customer.Address ?? DBNull.Value });
        var id = (int)await cmd.ExecuteScalarAsync(ct);
        return id;
    }

    public async Task<bool> UpdateAsync(string connectionString, Customer customer, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"update Customers
set FullName=@FullName, Phone=@Phone, Email=@Email, IDNumber=@IDNumber, Address=@Address
where CustomerId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = customer.CustomerId });
        cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = customer.FullName });
        cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 30) { Value = (object?)customer.Phone ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = (object?)customer.Email ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@IDNumber", SqlDbType.NVarChar, 50) { Value = (object?)customer.IDNumber ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@Address", SqlDbType.NVarChar, 300) { Value = (object?)customer.Address ?? DBNull.Value });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string connectionString, int customerId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"delete from Customers where CustomerId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = customerId });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}
