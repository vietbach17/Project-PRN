namespace HotelManagementModels;

public class InvoiceListItem
{
    public int InvoiceId { get; set; }
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public decimal SubtotalRoom { get; set; }
    public decimal SubtotalService { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance => Total - AmountPaid;
}
