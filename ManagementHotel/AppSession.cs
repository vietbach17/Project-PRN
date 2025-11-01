using HotelManagementModels;

namespace ManagementHotel;

public static class AppSession
{
    public static User? CurrentUser { get; private set; }

    public static void SetUser(User? user) => CurrentUser = user;

    public static bool IsAdmin => string.Equals(CurrentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public static bool IsStaff => string.Equals(CurrentUser?.Role, "Staff", StringComparison.OrdinalIgnoreCase);
    public static bool IsCustomer => string.Equals(CurrentUser?.Role, "Customer", StringComparison.OrdinalIgnoreCase);
}
