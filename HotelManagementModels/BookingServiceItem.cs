using System;

namespace HotelManagementModels;

public class BookingServiceItem
{
    public int BookingServiceId { get; set; }
    public int BookingId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Total => UnitPrice * Quantity;
}
