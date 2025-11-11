using System;

namespace HotelManagementModels;

public class Booking
{
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Guests { get; set; }
    public decimal TotalDue { get; set; }
}
