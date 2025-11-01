namespace HotelManagementModels;

public class HousekeepingTask
{
    public int TaskId { get; set; }
    public int RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public DateTime TaskDate { get; set; }
    public string? StaffName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
