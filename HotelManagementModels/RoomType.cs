namespace HotelManagementModels;

public class RoomType
{
    public int RoomTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BasePrice { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
