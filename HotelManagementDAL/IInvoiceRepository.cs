using HotelManagementModels;

namespace HotelManagementDAL;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<InvoiceListItem>> GetAllAsync(string connectionString, CancellationToken ct = default);
    Task<bool> RecalculateTotalsAsync(string connectionString, int invoiceId, CancellationToken ct = default);
}
