namespace HotelManagementBLL;

public static class RoleContext
{
    private static string? _role;
    public static void SetRole(string? role) => _role = role;
    private static string? R => _role?.Trim();
    public static bool IsAdmin => string.Equals(R, "Admin", StringComparison.OrdinalIgnoreCase);
    public static bool IsStaff => string.Equals(R, "Staff", StringComparison.OrdinalIgnoreCase);
    public static bool IsManager => string.Equals(R, "Manager", StringComparison.OrdinalIgnoreCase);
    public static bool IsCustomer => string.Equals(R, "Customer", StringComparison.OrdinalIgnoreCase);

    private static int? _customerId;
    public static void SetCustomerId(int? customerId) => _customerId = customerId;
    public static int? CustomerId => _customerId;
}
