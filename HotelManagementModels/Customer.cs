namespace HotelManagementModels;

public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? IDNumber { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}
