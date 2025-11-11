using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class CustomerServicesWindow : Window
{
    private readonly IBookingService _bookingService = new BookingService();
    private readonly IBookingServiceItemService _serviceItemService = new BookingServiceItemService();
    private IReadOnlyList<BookingListItem> _bookings = new List<BookingListItem>();

    public CustomerServicesWindow()
    {
        InitializeComponent();
        Loaded += async (_, __) => await LoadBookingsAsync();
    }

    private async Task LoadBookingsAsync()
    {
        txtInfo.Text = string.Empty;
        btnSelect.IsEnabled = false;
        string conn = Config.GetConnectionString();
        if (AppSession.CurrentUser?.CustomerId is not int cid || cid <= 0)
        {
            MessageBox.Show("No customer profile linked to this account.", "Services", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
            return;
        }
        try
        {
            _bookings = await _bookingService.GetAllForCustomerAsync(conn, cid);
            dgBookings.ItemsSource = _bookings;
            if (_bookings.Count == 0)
            {
                txtInfo.Text = "No bookings found.";
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message, "Load bookings", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadBookingsAsync();
    }

    private void Bookings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        btnSelect.IsEnabled = dgBookings.SelectedItem is BookingListItem;
    }

    private async void SelectServices_Click(object sender, RoutedEventArgs e)
    {
        if (dgBookings.SelectedItem is not BookingListItem booking)
        {
            MessageBox.Show("Please select a booking first.", "Services", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string conn = Config.GetConnectionString();
        List<BookingServiceItem> current = new();
        try
        {
            var existing = await _serviceItemService.GetByBookingIdAsync(conn, booking.BookingId);
            current = existing.ToList();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message, "Load services", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var dlg = new ServiceSelectionWindow(current) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            try
            {
                await _bookingService.UpdateServicesAsync(conn, booking.BookingId, dlg.SelectedServices);
                txtInfo.Text = "Services updated successfully.";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Update services", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
