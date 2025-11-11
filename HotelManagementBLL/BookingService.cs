using HotelManagementDAL;
using HotelManagementModels;
using System.Collections.Generic;
using System.Linq;

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

    public async Task UpdateServicesAsync(string connectionString, int bookingId, IEnumerable<BookingServiceItem> services, CancellationToken ct = default)
    {
        var booking = await _repo.GetByIdAsync(connectionString, bookingId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        if (RoleContext.IsCustomer)
        {
            if (RoleContext.CustomerId is not int cid || cid != booking.CustomerId)
                throw new UnauthorizedAccessException("You cannot update services for this booking.");
        }
        else
        {
            Authorization.EnsureCanUpdateBooking();
        }

        await PersistServicesAsync(connectionString, bookingId, services, ct);
        await _repo.RecalculateInvoiceTotalsAsync(connectionString, bookingId, ct);
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
