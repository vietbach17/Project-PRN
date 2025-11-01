using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class RoomRepository : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> GetAllAsync(string connectionString, CancellationToken ct = default)
    {
        var rooms = new List<Room>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
select r.RoomId, r.RoomNumber, r.Floor, r.RoomTypeId, r.Status, r.PricePerNight, r.CreatedAt,
       rt.RoomTypeId as RT_Id, rt.Name, rt.Capacity, rt.BasePrice, rt.Description, rt.CreatedAt as RT_CreatedAt,
       (
           select top 1 cu.FullName
           from BookingRooms br
           join Bookings b on b.BookingId = br.BookingId
           join Customers cu on cu.CustomerId = b.CustomerId
           where br.RoomId = r.RoomId
             and cast(getdate() as date) >= b.CheckInDate
             and cast(getdate() as date) <  b.CheckOutDate
           order by b.BookingId desc
       ) as CurrentCustomerName
from Rooms r
join RoomTypes rt on rt.RoomTypeId = r.RoomTypeId
order by r.RoomNumber";
        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await reader.ReadAsync(ct))
        {
            var room = new Room
            {
                RoomId = reader.GetInt32(0),
                RoomNumber = reader.GetString(1),
                Floor = await reader.IsDBNullAsync(2, ct) ? null : reader.GetInt32(2),
                RoomTypeId = reader.GetInt32(3),
                Status = reader.GetString(4),
                PricePerNight = reader.GetDecimal(5),
                CreatedAt = reader.GetDateTime(6),
                RoomType = new RoomType
                {
                    RoomTypeId = reader.GetInt32(7),
                    Name = reader.GetString(8),
                    Capacity = reader.GetInt32(9),
                    BasePrice = reader.GetDecimal(10),
                    Description = await reader.IsDBNullAsync(11, ct) ? null : reader.GetString(11),
                    CreatedAt = reader.GetDateTime(12)
                },
                CurrentCustomerName = await reader.IsDBNullAsync(13, ct) ? null : reader.GetString(13)
            };
            rooms.Add(room);
        }
        return rooms;
    }

    public async Task<Room?> GetByIdAsync(string connectionString, int roomId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
select r.RoomId, r.RoomNumber, r.Floor, r.RoomTypeId, r.Status, r.PricePerNight, r.CreatedAt
from Rooms r where r.RoomId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = roomId });
        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await reader.ReadAsync(ct))
        {
            return new Room
            {
                RoomId = reader.GetInt32(0),
                RoomNumber = reader.GetString(1),
                Floor = await reader.IsDBNullAsync(2, ct) ? null : reader.GetInt32(2),
                RoomTypeId = reader.GetInt32(3),
                Status = reader.GetString(4),
                PricePerNight = reader.GetDecimal(5),
                CreatedAt = reader.GetDateTime(6)
            };
        }
        return null;
    }

    public async Task<int> AddAsync(string connectionString, Room room, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"insert into Rooms(RoomNumber, Floor, RoomTypeId, Status, PricePerNight, CreatedAt)
values(@RoomNumber,@Floor,@RoomTypeId,@Status,@PricePerNight,SYSDATETIME());
select cast(SCOPE_IDENTITY() as int);";
        cmd.Parameters.Add(new SqlParameter("@RoomNumber", SqlDbType.NVarChar, 20) { Value = room.RoomNumber });
        cmd.Parameters.Add(new SqlParameter("@Floor", SqlDbType.Int) { Value = (object?)room.Floor ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@RoomTypeId", SqlDbType.Int) { Value = room.RoomTypeId });
        cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 30) { Value = room.Status });
        var p = new SqlParameter("@PricePerNight", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = room.PricePerNight };
        cmd.Parameters.Add(p);
        var id = (int)await cmd.ExecuteScalarAsync(ct);
        return id;
    }

    public async Task<bool> UpdateAsync(string connectionString, Room room, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"update Rooms set RoomNumber=@RoomNumber, Floor=@Floor, RoomTypeId=@RoomTypeId, Status=@Status, PricePerNight=@PricePerNight where RoomId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = room.RoomId });
        cmd.Parameters.Add(new SqlParameter("@RoomNumber", SqlDbType.NVarChar, 20) { Value = room.RoomNumber });
        cmd.Parameters.Add(new SqlParameter("@Floor", SqlDbType.Int) { Value = (object?)room.Floor ?? DBNull.Value });
        cmd.Parameters.Add(new SqlParameter("@RoomTypeId", SqlDbType.Int) { Value = room.RoomTypeId });
        cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 30) { Value = room.Status });
        var p = new SqlParameter("@PricePerNight", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = room.PricePerNight };
        cmd.Parameters.Add(p);
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string connectionString, int roomId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"delete from Rooms where RoomId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = roomId });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}
