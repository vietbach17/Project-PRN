using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;
    public BookingService(IBookingRepository repo) { _repo = repo; }
    public BookingService() { _repo = new BookingRepository(); }
    public Task<IReadOnlyList<BookingListItem>> GetAllAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);
    public Task<IReadOnlyList<BookingListItem>> GetAllForCustomerAsync(string connectionString, int customerId, CancellationToken ct = default)
        => _repo.GetAllForCustomerAsync(connectionString, customerId, ct);
    public Task<Booking?> GetByIdAsync(string connectionString, int bookingId, CancellationToken ct = default)
        => _repo.GetByIdAsync(connectionString, bookingId, ct);
    public Task<int> AddAsync(string connectionString, Booking booking, CancellationToken ct = default)
    {
        Authorization.EnsureCanAddBooking();
        booking.Status = NormalizeStatus(booking.Status);
        return _repo.AddAsync(connectionString, booking, ct);
    }
    public Task<bool> UpdateAsync(string connectionString, Booking booking, CancellationToken ct = default)
    {
        Authorization.EnsureCanUpdateBooking();
        booking.Status = NormalizeStatus(booking.Status);
        return _repo.UpdateAsync(connectionString, booking, ct);
    }
    public Task<bool> DeleteAsync(string connectionString, int bookingId, CancellationToken ct = default)
    {
        Authorization.EnsureCanDeleteBooking();
        return _repo.DeleteAsync(connectionString, bookingId, ct);
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
