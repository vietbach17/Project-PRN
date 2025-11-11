using HotelManagementModels;

namespace HotelManagementBLL;

public interface IBookingServiceItemService
{
    Task<IReadOnlyList<BookingServiceItem>> GetByBookingIdAsync(string connectionString, int bookingId, CancellationToken ct = default);
    Task ReplaceForBookingAsync(string connectionString, int bookingId, IEnumerable<BookingServiceItem> items, CancellationToken ct = default);
}
