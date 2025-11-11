using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;
using System.Linq;

namespace ManagementHotel;

public partial class BookingsWindow : Window
{
    private readonly IBookingService _service = new BookingService();
    private IReadOnlyList<BookingListItem> _allItems = Array.Empty<BookingListItem>();

    public BookingsWindow()
    {
        InitializeComponent();
        Loaded += async (_, __) =>
        {
            txtUserBanner.Text = $"Signed in as {AppSession.GetUserDisplay()}";
            await ReloadAsync();
        };
        ApplyRolePermissions();
    }

    private async Task ReloadAsync()
    {
        string conn = Config.GetConnectionString();
        try
        {
            IReadOnlyList<BookingListItem> items;
            if (AppSession.IsCustomer && AppSession.CurrentUser?.CustomerId is int cid && cid > 0)
            {
                items = await _service.GetAllForCustomerAsync(conn, cid);
            }
            else
            {
                items = await _service.GetAllAsync(conn);
            }
            _allItems = items;
            ApplyStatusFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Load Bookings Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        ApplyRolePermissions();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await ReloadAsync();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new BookingEditWindow { Owner = this };
        if (AppSession.IsCustomer)
        {
            // customer self booking: try preselect by matching username with a customer if applicable (skipping here)
        }
        if (dlg.ShowDialog() == true)
        {
            string conn = Config.GetConnectionString();
            try
            {
                await _service.AddAsync(conn, dlg.Model);
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Add Booking Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not BookingListItem selected)
        {
            MessageBox.Show("Please select a booking.");
            return;
        }
        string conn = Config.GetConnectionString();
        var full = await _service.GetByIdAsync(conn, selected.BookingId);
        if (full is null)
        {
            MessageBox.Show("Booking not found.");
            return;
        }
        var dlg = new BookingEditWindow(full) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            try
            {
                await _service.UpdateAsync(conn, dlg.Model);
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Booking Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not BookingListItem selected)
        {
            MessageBox.Show("Please select a booking.");
            return;
        }
        if (MessageBox.Show($"Delete booking #{selected.BookingId}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        string conn = Config.GetConnectionString();
        try
        {
            await _service.DeleteAsync(conn, selected.BookingId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete Booking Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyRolePermissions()
    {
        if (AppSession.IsAdmin)
        {
            btnAdd.IsEnabled = true;
            btnEdit.IsEnabled = true;
            btnDelete.IsEnabled = true;
            return;
        }
        if (AppSession.IsStaff)
        {
            btnAdd.IsEnabled = true;
            btnEdit.IsEnabled = true;
            btnDelete.IsEnabled = false;
            return;
        }
        // Customer: only Add new booking
        btnAdd.IsEnabled = true;
        btnEdit.IsEnabled = false;
        btnDelete.IsEnabled = false;
    }

    private void StatusFilter_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return; // avoid running during InitializeComponent
        ApplyStatusFilter();
    }

    private void ApplyStatusFilter()
    {
        if (_allItems is null) return;
        if (dg == null) return; // DataGrid not ready yet
        string? selected = (cbStatusFilter?.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
        if (string.IsNullOrEmpty(selected) || selected == "All")
        {
            dg.ItemsSource = _allItems;
            return;
        }
        var filtered = _allItems.Where(i => string.Equals(i.Status, selected, StringComparison.OrdinalIgnoreCase)).ToList();
        dg.ItemsSource = filtered;
    }
}
