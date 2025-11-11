using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class BookingServiceItemRepository : IBookingServiceItemRepository
{
    public async Task<IReadOnlyList<BookingServiceItem>> GetByBookingIdAsync(string connectionString, int bookingId, CancellationToken ct = default)
    {
        var list = new List<BookingServiceItem>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select bs.BookingServiceId, bs.BookingId, bs.ServiceId, s.Name, s.Unit, bs.UnitPrice, bs.Quantity
from BookingServices bs
join Services s on s.ServiceId = bs.ServiceId
where bs.BookingId=@Bid
order by s.Name";
        cmd.Parameters.Add(new SqlParameter("@Bid", SqlDbType.Int) { Value = bookingId });
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new BookingServiceItem
            {
                BookingServiceId = rd.GetInt32(0),
                BookingId = rd.GetInt32(1),
                ServiceId = rd.GetInt32(2),
                ServiceName = rd.GetString(3),
                Unit = rd.GetString(4),
                UnitPrice = rd.GetDecimal(5),
                Quantity = rd.GetInt32(6)
            });
        }
        return list;
    }

    public async Task ReplaceForBookingAsync(string connectionString, int bookingId, IEnumerable<BookingServiceItem> items, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            await using (var delCmd = conn.CreateCommand())
            {
                delCmd.Transaction = tx;
                delCmd.CommandText = "delete from BookingServices where BookingId=@Bid";
                delCmd.Parameters.Add(new SqlParameter("@Bid", SqlDbType.Int) { Value = bookingId });
                await delCmd.ExecuteNonQueryAsync(ct);
            }

            foreach (var item in items)
            {
                await using var insCmd = conn.CreateCommand();
                insCmd.Transaction = tx;
                insCmd.CommandText = @"insert into BookingServices(BookingId, ServiceId, Quantity, UnitPrice)
values(@Bid, @Sid, @Qty, @Price)";
                insCmd.Parameters.Add(new SqlParameter("@Bid", SqlDbType.Int) { Value = bookingId });
                insCmd.Parameters.Add(new SqlParameter("@Sid", SqlDbType.Int) { Value = item.ServiceId });
                insCmd.Parameters.Add(new SqlParameter("@Qty", SqlDbType.Int) { Value = item.Quantity });
                insCmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = item.UnitPrice });
                await insCmd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
