using HotelManagementDAL;
using HotelManagementModels;

namespace HotelManagementBLL;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repo;
    public InvoiceService(IInvoiceRepository repo) { _repo = repo; }
    public InvoiceService() { _repo = new InvoiceRepository(); }
    public Task<IReadOnlyList<InvoiceListItem>> GetAllAsync(string connectionString, CancellationToken ct = default)
        => _repo.GetAllAsync(connectionString, ct);

    public Task<bool> RecalculateTotalsAsync(string connectionString, int invoiceId, CancellationToken ct = default)
    {
        // Only Admin/Staff can recalc
        if (!(RoleContext.IsAdmin || RoleContext.IsStaff))
            throw new UnauthorizedAccessException("You do not have permission to recalculate invoices.");
        return _repo.RecalculateTotalsAsync(connectionString, invoiceId, ct);
    }
}
