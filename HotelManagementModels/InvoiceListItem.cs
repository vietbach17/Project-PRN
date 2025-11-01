namespace HotelManagementModels;

public class InvoiceListItem
{
    public int InvoiceId { get; set; }
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal SubtotalRoom { get; set; }
    public decimal SubtotalService { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public DateTime IssuedAt { get; set; }
}
