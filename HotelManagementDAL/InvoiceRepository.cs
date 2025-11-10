using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagementModels;

namespace HotelManagementDAL;

public class InvoiceRepository : IInvoiceRepository
{
    public async Task<IReadOnlyList<InvoiceListItem>> GetAllAsync(string connectionString, CancellationToken ct = default)
    {
        var list = new List<InvoiceListItem>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"select i.InvoiceId, i.BookingId, c.FullName, c.Phone, c.Email,
       i.SubtotalRoom, i.SubtotalService, i.Tax, i.Discount, i.Total, i.IssuedAt,
       b.CheckInDate, b.CheckOutDate,
       (select isnull(sum(p.Amount),0) from Payments p where p.BookingId = i.BookingId) as AmountPaid
from Invoices i
join Bookings b on b.BookingId = i.BookingId
join Customers c on c.CustomerId = b.CustomerId
order by i.InvoiceId desc";
        await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new InvoiceListItem
            {
                InvoiceId = rd.GetInt32(0),
                BookingId = rd.GetInt32(1),
                CustomerName = rd.GetString(2),
                CustomerPhone = await rd.IsDBNullAsync(3, ct) ? null : rd.GetString(3),
                CustomerEmail = await rd.IsDBNullAsync(4, ct) ? null : rd.GetString(4),
                SubtotalRoom = rd.GetDecimal(5),
                SubtotalService = rd.GetDecimal(6),
                Tax = rd.GetDecimal(7),
                Discount = rd.GetDecimal(8),
                Total = rd.GetDecimal(9),
                IssuedAt = rd.GetDateTime(10),
                CheckInDate = rd.GetDateTime(11),
                CheckOutDate = rd.GetDateTime(12),
                AmountPaid = rd.GetDecimal(13)
            });
        }
        return list;
    }

    public async Task<bool> RecalculateTotalsAsync(string connectionString, int invoiceId, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        // Compute SubtotalRoom based on nights * RatePerNight from BookingRooms
        var sql = @"
declare @bid int = (select BookingId from Invoices where InvoiceId=@Id);
if @bid is null return;

declare @nights int = DATEDIFF(DAY, (select CheckInDate from Bookings where BookingId=@bid), (select CheckOutDate from Bookings where BookingId=@bid));
if (@nights < 1) set @nights = 1;

update i
set SubtotalRoom = (select ISNULL(SUM(@nights * br.RatePerNight),0) from BookingRooms br where br.BookingId = @bid),
    SubtotalService = (select ISNULL(SUM(bs.Quantity * bs.UnitPrice),0) from BookingServices bs where bs.BookingId = @bid),
    Tax = (select ROUND((ISNULL((select ISNULL(SUM(@nights * br2.RatePerNight),0) from BookingRooms br2 where br2.BookingId=@bid),0) + ISNULL((select ISNULL(SUM(bs2.Quantity * bs2.UnitPrice),0) from BookingServices bs2 where bs2.BookingId=@bid),0)) * 0.10, 0))
from Invoices i where i.InvoiceId=@Id;";
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = invoiceId });
        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }
}
