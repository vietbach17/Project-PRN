namespace HotelManagementBLL;

public static class Authorization
{
    public static void EnsureCanAddOrUpdateEntity()
    {
        if (!(RoleContext.IsAdmin || RoleContext.IsStaff))
            throw new UnauthorizedAccessException("You do not have permission to add or update this entity.");
    }

    public static void EnsureCanDeleteEntity()
    {
        if (!RoleContext.IsAdmin)
            throw new UnauthorizedAccessException("You do not have permission to delete this entity.");
    }

    public static void EnsureCanAddBooking()
    {
        if (!(RoleContext.IsAdmin || RoleContext.IsStaff || RoleContext.IsCustomer))
            throw new UnauthorizedAccessException("You do not have permission to create a booking.");
    }

    public static void EnsureCanUpdateBooking(int customerId)
    {
        if (RoleContext.IsAdmin || RoleContext.IsStaff)
            return;
        if (RoleContext.IsCustomer && RoleContext.CustomerId.HasValue && RoleContext.CustomerId.Value == customerId)
            return;
        throw new UnauthorizedAccessException("You do not have permission to update a booking.");
    }

    public static void EnsureCanDeleteBooking()
    {
        if (!RoleContext.IsAdmin)
            throw new UnauthorizedAccessException("You do not have permission to delete a booking.");
    }

    // Allow Admin/Staff to update any customer; allow Customer to update only their own record
    public static void EnsureCanUpdateCustomer(int customerId)
    {
        if (RoleContext.IsAdmin || RoleContext.IsStaff)
            return;
        if (RoleContext.IsCustomer && RoleContext.CustomerId.HasValue && RoleContext.CustomerId.Value == customerId)
            return;
        throw new UnauthorizedAccessException("You do not have permission to add or update this entity.");
    }
}
