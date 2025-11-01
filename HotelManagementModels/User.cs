namespace HotelManagementModels;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin, Staff, Customer
    public bool IsActive { get; set; } = true;
    public int? CustomerId { get; set; }
}
