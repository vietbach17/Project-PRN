using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class BookingServiceItemService : IBookingServiceItemService
{
    private readonly IBookingServiceItemRepository _repo;

    public BookingServiceItemService(IBookingServiceItemRepository repo)
    {
        _repo = repo;
    }

    public BookingServiceItemService()
    {
        _repo = new BookingServiceItemRepository();
    }

    public Task<IReadOnlyList<BookingServiceItem>> GetByBookingIdAsync(string connectionString, int bookingId, CancellationToken ct = default)
        => _repo.GetByBookingIdAsync(connectionString, bookingId, ct);

    public Task ReplaceForBookingAsync(string connectionString, int bookingId, IEnumerable<BookingServiceItem> items, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddBooking();
        return _repo.ReplaceForBookingAsync(connectionString, bookingId, items, ct);
    }
}
