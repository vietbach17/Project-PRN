using HotelManagementModels;
using System.Windows;

namespace ManagementHotel;

public static class AppSession
{
    public static User? CurrentUser { get; private set; }

    public static void SetUser(User? user) => CurrentUser = user;

    public static void Clear() => CurrentUser = null;

    public static bool IsAdmin => string.Equals(CurrentUser?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
    public static bool IsStaff => string.Equals(CurrentUser?.Role, "Staff", StringComparison.OrdinalIgnoreCase);
    public static bool IsCustomer => string.Equals(CurrentUser?.Role, "Customer", StringComparison.OrdinalIgnoreCase);

    public static string GetUserDisplay() => CurrentUser?.Username ?? "Guest";

    public static void ApplyWindowUserTitle(Window window)
    {
        if (window is null) return;
        var baseTitle = window.Tag as string;
        if (string.IsNullOrEmpty(baseTitle))
        {
            baseTitle = window.Title;
            window.Tag = baseTitle;
        }
        var username = CurrentUser?.Username;
        window.Title = string.IsNullOrWhiteSpace(username) ? baseTitle! : $"{baseTitle} - {username}";
    }
}
