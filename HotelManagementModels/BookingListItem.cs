namespace HotelManagementModels;

public class BookingListItem
{
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
