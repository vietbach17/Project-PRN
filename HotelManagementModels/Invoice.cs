namespace HotelManagementModels;

public class Invoice
{
    public int InvoiceId { get; set; }
    public int BookingId { get; set; }
    public decimal SubtotalRoom { get; set; }
    public decimal SubtotalService { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public DateTime IssuedAt { get; set; }
}
