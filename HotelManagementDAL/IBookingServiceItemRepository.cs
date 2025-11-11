using HotelManagementModels;

namespace HotelManagementDAL;

public interface IBookingServiceItemRepository
{
    Task<IReadOnlyList<BookingServiceItem>> GetByBookingIdAsync(string connectionString, int bookingId, CancellationToken ct = default);
    Task ReplaceForBookingAsync(string connectionString, int bookingId, IEnumerable<BookingServiceItem> items, CancellationToken ct = default);
}
