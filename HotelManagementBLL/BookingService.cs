using HotelManagementDAL;
using HotelManagementModels;
using System.Linq;

namespace HotelManagementBLL;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;
    private readonly IBookingServiceItemRepository _serviceItems;
    public BookingService(IBookingRepository repo, IBookingServiceItemRepository serviceItems)
    {
        _repo = repo;
        _serviceItems = serviceItems;
    }
    public BookingService() : this(new BookingRepository(), new BookingServiceItemRepository()) { }
    public Task<IReadOnlyList<BookingListItem>> GetAllAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);
    public Task<IReadOnlyList<BookingListItem>> GetAllForCustomerAsync(string connectionString, int customerId, CancellationToken ct = default)
        => _repo.GetAllForCustomerAsync(connectionString, customerId, ct);
    public async Task<Booking?> GetByIdAsync(string connectionString, int bookingId, CancellationToken ct = default)
    {
        var booking = await _repo.GetByIdAsync(connectionString, bookingId, ct);
        if (booking != null)
        {
            var services = await _serviceItems.GetByBookingIdAsync(connectionString, bookingId, ct);
            booking.Services = services.ToList();
        }
        return booking;
    }
    public async Task<int> AddAsync(string connectionString, Booking booking, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddBooking();
        booking.Status = NormalizeStatus(booking.Status);
        int bookingId = await _repo.AddAsync(connectionString, booking, ct);
        await PersistServicesAsync(connectionString, bookingId, booking.Services, ct);
        await _repo.RecalculateInvoiceTotalsAsync(connectionString, bookingId, ct);
        return bookingId;
    }
    public async Task<bool> UpdateAsync(string connectionString, Booking booking, CancellationToken ct = default)
    {
        Authorization.EnsureCanUpdateBooking();
        booking.Status = NormalizeStatus(booking.Status);
        var updated = await _repo.UpdateAsync(connectionString, booking, ct);
        if (updated)
        {
            await PersistServicesAsync(connectionString, booking.BookingId, booking.Services, ct);
            await _repo.RecalculateInvoiceTotalsAsync(connectionString, booking.BookingId, ct);
        }
        return updated;
    }
    public Task<bool> DeleteAsync(string connectionString, int bookingId, CancellationToken ct = default)
    {
        Authorization.EnsureCanDeleteBooking();
        return _repo.DeleteAsync(connectionString, bookingId, ct);
    }

    private Task PersistServicesAsync(string connectionString, int bookingId, IEnumerable<BookingServiceItem>? services, CancellationToken ct)
    {
        var items = (services ?? Enumerable.Empty<BookingServiceItem>())
            .Where(s => s.Quantity > 0)
            .Select(s => new BookingServiceItem
            {
                BookingServiceId = s.BookingServiceId,
                BookingId = bookingId,
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Unit = s.Unit,
                UnitPrice = s.UnitPrice,
                Quantity = s.Quantity
            }).ToList();
        return _serviceItems.ReplaceForBookingAsync(connectionString, bookingId, items, ct);
    }

    private static string NormalizeStatus(string? s)
    {
        var v = (s ?? string.Empty).Trim();
        if (v.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) return "Cancelled";
        if (v.Equals("Canceled", StringComparison.OrdinalIgnoreCase)) return "Cancelled"; // unify to DB spelling
        if (v.Equals("CheckedOut", StringComparison.OrdinalIgnoreCase)) return "CheckedOut";
        if (v.Equals("CheckedIn", StringComparison.OrdinalIgnoreCase)) return "CheckedIn";
        if (v.Equals("Reserved", StringComparison.OrdinalIgnoreCase)) return "Reserved";
        // default safe value per DB constraint
        return "Reserved";
    }
}
