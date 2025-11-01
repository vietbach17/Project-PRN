namespace HotelManagementModels;

public class Room
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int? Floor { get; set; }
    public int RoomTypeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public DateTime CreatedAt { get; set; }
    public RoomType? RoomType { get; set; }
    public string? CurrentCustomerName { get; set; }
}
