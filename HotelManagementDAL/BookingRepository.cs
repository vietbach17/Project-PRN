using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class BookingRepository : IBookingRepository
{
    public async Task<IReadOnlyList<BookingListItem>> GetAllAsync(string connectionString, CancellationToken ct = default)
    {
        var list = new List<BookingListItem>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select b.BookingId, c.FullName, b.CheckInDate, b.CheckOutDate, b.Status, b.Notes
from Bookings b join Customers c on c.CustomerId = b.CustomerId order by b.BookingId desc";
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new BookingListItem
            {
                BookingId = rd.GetInt32(0),
                CustomerName = rd.GetString(1),
                CheckInDate = rd.GetDateTime(2),
                CheckOutDate = rd.GetDateTime(3),
                Status = rd.GetString(4),
                Notes = await rd.IsDBNullAsync(5, ct) ? null : rd.GetString(5)
            });
        }
        return list;
    }

    public async Task<IReadOnlyList<BookingListItem>> GetAllForCustomerAsync(string connectionString, int customerId, CancellationToken ct = default)
    {
        var list = new List<BookingListItem>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select b.BookingId, c.FullName, b.CheckInDate, b.CheckOutDate, b.Status, b.Notes
from Bookings b join Customers c on c.CustomerId = b.CustomerId
where b.CustomerId=@cid order by b.BookingId desc";
        cmd.Parameters.Add(new SqlParameter("@cid", SqlDbType.Int) { Value = customerId });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new BookingListItem
            {
                BookingId = rd.GetInt32(0),
                CustomerName = rd.GetString(1),
                CheckInDate = rd.GetDateTime(2),
                CheckOutDate = rd.GetDateTime(3),
                Status = rd.GetString(4),
                Notes = await rd.IsDBNullAsync(5, ct) ? null : rd.GetString(5)
            });
        }
        return list;
    }

    public async Task<Booking?> GetByIdAsync(string connectionString, int bookingId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select b.BookingId, b.CustomerId,
       (select top 1 br.RoomId from BookingRooms br where br.BookingId=b.BookingId order by br.BookingRoomId) as RoomId,
       b.CheckInDate, b.CheckOutDate, b.Status, b.Notes, b.CreatedAt
from Bookings b where b.BookingId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = bookingId });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow, ct);
        if (await rd.ReadAsync(ct))
        {
            return new Booking
            {
                BookingId = rd.GetInt32(0),
                CustomerId = rd.GetInt32(1),
                RoomId = await rd.IsDBNullAsync(2, ct) ? 0 : rd.GetInt32(2),
                CheckInDate = rd.GetDateTime(3),
                CheckOutDate = rd.GetDateTime(4),
                Status = rd.GetString(5),
                Notes = await rd.IsDBNullAsync(6, ct) ? null : rd.GetString(6),
                CreatedAt = rd.GetDateTime(7)
            };
        }
        return null;
    }

    public async Task<int> AddAsync(string connectionString, Booking booking, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        // availability check via BookingRooms join
        await using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = @"select count(*)
from BookingRooms br
join Bookings b on b.BookingId = br.BookingId
where br.RoomId=@RoomId and not (b.CheckOutDate <= @CheckIn or b.CheckInDate >= @CheckOut)";
            checkCmd.Parameters.Add(new SqlParameter("@RoomId", SqlDbType.Int) { Value = booking.RoomId });
            checkCmd.Parameters.Add(new SqlParameter("@CheckIn", SqlDbType.Date) { Value = booking.CheckInDate.Date });
            checkCmd.Parameters.Add(new SqlParameter("@CheckOut", SqlDbType.Date) { Value = booking.CheckOutDate.Date });
            var conflicts = (int)await checkCmd.ExecuteScalarAsync(ct);
            if (conflicts > 0)
                throw new InvalidOperationException("Selected room is not available for the given dates.");
        }
        int bookingId;
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"insert into Bookings(CustomerId, CheckInDate, CheckOutDate, Status, Notes, CreatedAt)
values(@CustomerId, @CheckInDate, @CheckOutDate, @Status, @Notes, SYSDATETIME());
select cast(SCOPE_IDENTITY() as int);";
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = booking.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@CheckInDate", SqlDbType.Date) { Value = booking.CheckInDate.Date });
            cmd.Parameters.Add(new SqlParameter("@CheckOutDate", SqlDbType.Date) { Value = booking.CheckOutDate.Date });
            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 30) { Value = booking.Status });
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 300) { Value = (object?)booking.Notes ?? DBNull.Value });
            bookingId = (int)await cmd.ExecuteScalarAsync(ct);
        }
        // insert BookingRooms with default Guests=1 and RatePerNight from Rooms
        await using (var cmdBr = conn.CreateCommand())
        {
            cmdBr.CommandText = @"insert into BookingRooms(BookingId, RoomId, Guests, RatePerNight)
values(@Bid, @RoomId, 1, (select PricePerNight from Rooms where RoomId=@RoomId))";
            cmdBr.Parameters.Add(new SqlParameter("@Bid", SqlDbType.Int) { Value = bookingId });
            cmdBr.Parameters.Add(new SqlParameter("@RoomId", SqlDbType.Int) { Value = booking.RoomId });
            await cmdBr.ExecuteNonQueryAsync(ct);
        }
        return bookingId;
    }

    public async Task<bool> UpdateAsync(string connectionString, Booking booking, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        // availability check excluding this booking (via BookingRooms join)
        await using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = @"select count(*)
from BookingRooms br
join Bookings b on b.BookingId = br.BookingId
where br.RoomId=@RoomId and b.BookingId<>@Id and not (b.CheckOutDate <= @CheckIn or b.CheckInDate >= @CheckOut)";
            checkCmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = booking.BookingId });
            checkCmd.Parameters.Add(new SqlParameter("@RoomId", SqlDbType.Int) { Value = booking.RoomId });
            checkCmd.Parameters.Add(new SqlParameter("@CheckIn", SqlDbType.Date) { Value = booking.CheckInDate.Date });
            checkCmd.Parameters.Add(new SqlParameter("@CheckOut", SqlDbType.Date) { Value = booking.CheckOutDate.Date });
            var conflicts = (int)await checkCmd.ExecuteScalarAsync(ct);
            if (conflicts > 0)
                throw new InvalidOperationException("Selected room is not available for the given dates.");
        }
        int rows;
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"update Bookings set CustomerId=@CustomerId, CheckInDate=@CheckInDate, CheckOutDate=@CheckOutDate, Status=@Status, Notes=@Notes where BookingId=@Id";
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = booking.BookingId });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = booking.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@CheckInDate", SqlDbType.Date) { Value = booking.CheckInDate.Date });
            cmd.Parameters.Add(new SqlParameter("@CheckOutDate", SqlDbType.Date) { Value = booking.CheckOutDate.Date });
            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 30) { Value = booking.Status });
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 300) { Value = (object?)booking.Notes ?? DBNull.Value });
            rows = await cmd.ExecuteNonQueryAsync(ct);
        }
        // Recreate single BookingRooms row (simple approach)
        await using (var del = conn.CreateCommand())
        {
            del.CommandText = "delete from BookingRooms where BookingId=@Id";
            del.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = booking.BookingId });
            await del.ExecuteNonQueryAsync(ct);
        }
        await using (var ins = conn.CreateCommand())
        {
            ins.CommandText = @"insert into BookingRooms(BookingId, RoomId, Guests, RatePerNight)
values(@Bid, @RoomId, 1, (select PricePerNight from Rooms where RoomId=@RoomId))";
            ins.Parameters.Add(new SqlParameter("@Bid", SqlDbType.Int) { Value = booking.BookingId });
            ins.Parameters.Add(new SqlParameter("@RoomId", SqlDbType.Int) { Value = booking.RoomId });
            await ins.ExecuteNonQueryAsync(ct);
        }
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string connectionString, int bookingId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"delete from Bookings where BookingId=@Id";
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = bookingId });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}
