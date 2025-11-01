using HotelManagementModels;

namespace HotelManagementBLL;

public interface IInvoiceService
{
    Task<IReadOnlyList<InvoiceListItem>> GetAllAsync(string connectionString, CancellationToken ct = default);
    Task<bool> RecalculateTotalsAsync(string connectionString, int invoiceId, CancellationToken ct = default);
}
