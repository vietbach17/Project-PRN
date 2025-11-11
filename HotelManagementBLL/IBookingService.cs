using HotelManagementModels;

namespace HotelManagementBLL;

public interface IBookingService
{
    Task<IReadOnlyList<BookingListItem>> GetAllAsync(string connectionString, CancellationToken ct = default);
    Task<IReadOnlyList<BookingListItem>> GetAllForCustomerAsync(string connectionString, int customerId, CancellationToken ct = default);
    Task<Booking?> GetByIdAsync(string connectionString, int bookingId, CancellationToken ct = default);
    Task<int> AddAsync(string connectionString, Booking booking, CancellationToken ct = default);
    Task<bool> UpdateAsync(string connectionString, Booking booking, CancellationToken ct = default);
    Task<bool> DeleteAsync(string connectionString, int bookingId, CancellationToken ct = default);
    Task UpdateServicesAsync(string connectionString, int bookingId, IEnumerable<BookingServiceItem> services, CancellationToken ct = default);
}
