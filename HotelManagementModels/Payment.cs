namespace HotelManagementModels;

public class Payment
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string? Reference { get; set; }
}
