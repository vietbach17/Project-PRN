using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class BookingEditWindow : Window
{
    public Booking Model { get; private set; }
    private readonly ICustomerService _customerService = new CustomerService();
    private readonly IRoomService _roomService = new RoomService();
    private readonly IBookingServiceItemService _bookingServiceItems = new BookingServiceItemService();
    private Customer? _selectedCustomer;
    private List<BookingServiceItem> _selectedServices = new();

    public BookingEditWindow(Booking? model = null)
    {
        InitializeComponent();
        Model = model ?? new Booking { Status = "Pending", CheckInDate = System.DateTime.Today, CheckOutDate = System.DateTime.Today.AddDays(1) };
        Loaded += async (_, __) =>
        {
            await LoadRoomsAsync();
            await LoadCustomerAsync();
            BindFromModel();
            // lock customer selection for Customer role
            if (AppSession.IsCustomer && AppSession.CurrentUser?.CustomerId is int cid && cid > 0)
            {
                await SetCustomerByIdAsync(cid);
                btnSelectCustomer.IsEnabled = false;
                lblStatus.Visibility = Visibility.Collapsed;
                cbStatus.Visibility = Visibility.Collapsed;
                foreach (var item in cbStatus.Items.OfType<ComboBoxItem>())
                {
                    if ((string)item.Content == "Pending")
                    {
                        cbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        };
    }

    private async Task LoadCustomerAsync()
    {
        if (Model.CustomerId > 0)
        {
            await SetCustomerByIdAsync(Model.CustomerId);
        }
    }

    private async Task LoadRoomsAsync()
    {
        string conn = Config.GetConnectionString();
        var rooms = await _roomService.GetRoomsAsync(conn);
        cbRoom.ItemsSource = rooms;
        if (Model.RoomId != 0)
            cbRoom.SelectedValue = Model.RoomId;
        else if (rooms.Count > 0)
            cbRoom.SelectedValue = rooms[0].RoomId;
    }

    private void BindFromModel()
    {
        dpCheckIn.SelectedDate = Model.CheckInDate.Date;
        dpCheckOut.SelectedDate = Model.CheckOutDate.Date;
        txtCustomer.Text = _selectedCustomer?.FullName ?? string.Empty;
        foreach (var item in cbStatus.Items)
        {
            if (item is ComboBoxItem cbi && (string)cbi.Content == (Model.Status ?? "Pending"))
            {
                cbStatus.SelectedItem = cbi;
                break;
            }
        }
        txtNotes.Text = Model.Notes ?? string.Empty;
        if (Model.Services != null && Model.Services.Count > 0)
            _selectedServices = Model.Services.Select(CloneService).ToList();
        else
            _selectedServices = new List<BookingServiceItem>();
        UpdateServiceSummary();
    }

    private bool BindToModel()
    {
        if (Model.CustomerId <= 0)
        {
            MessageBox.Show("Please select a customer", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (cbRoom.SelectedValue is not int roomId)
        {
            MessageBox.Show("Please select a room", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            cbRoom.Focus();
            return false;
        }
        if (dpCheckIn.SelectedDate is null || dpCheckOut.SelectedDate is null)
        {
            MessageBox.Show("Please select check-in and check-out dates", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        var ci = dpCheckIn.SelectedDate.Value.Date;
        var co = dpCheckOut.SelectedDate.Value.Date;
        if (co <= ci)
        {
            MessageBox.Show("Check-out must be after check-in", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        var status = (cbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pending";
        Model.RoomId = roomId;
        Model.CheckInDate = ci;
        Model.CheckOutDate = co;
        Model.Status = status;
        Model.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
        Model.Services = _selectedServices.Select(CloneService).ToList();
        return true;
    }

    private async Task SetCustomerByIdAsync(int customerId)
    {
        string conn = Config.GetConnectionString();
        var customer = await _customerService.GetByIdAsync(conn, customerId);
        if (customer != null)
        {
            SetCustomer(customer);
        }
    }

    private void SetCustomer(Customer customer)
    {
        _selectedCustomer = customer;
        Model.CustomerId = customer.CustomerId;
        txtCustomer.Text = customer.FullName;
    }

    private async void SelectCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (AppSession.IsCustomer && AppSession.CurrentUser?.CustomerId is int cid && cid > 0)
        {
            await SetCustomerByIdAsync(cid);
            return;
        }
        var picker = new CustomerPickerWindow { Owner = this };
        if (picker.ShowDialog() == true && picker.SelectedCustomer != null)
        {
            SetCustomer(picker.SelectedCustomer);
        }
    }

    private static BookingServiceItem CloneService(BookingServiceItem item) => new()
    {
        BookingServiceId = item.BookingServiceId,
        BookingId = item.BookingId,
        ServiceId = item.ServiceId,
        ServiceName = item.ServiceName,
        Unit = item.Unit,
        UnitPrice = item.UnitPrice,
        Quantity = item.Quantity
    };

    private void UpdateServiceSummary()
    {
        if (_selectedServices == null || _selectedServices.Count == 0)
        {
            txtServiceSummary.Text = "No services";
            return;
        }
        int totalQty = _selectedServices.Sum(s => s.Quantity);
        decimal totalAmount = _selectedServices.Sum(s => s.Total);
        txtServiceSummary.Text = $"{totalQty} item(s) - {totalAmount:N0}";
    }

    private async void SelectServices_Click(object sender, RoutedEventArgs e)
    {
        if (Model.BookingId > 0 && (Model.Services == null || Model.Services.Count == 0) && _selectedServices.Count == 0)
        {
            try
            {
                string conn = Config.GetConnectionString();
                var existing = await _bookingServiceItems.GetByBookingIdAsync(conn, Model.BookingId);
                if (existing != null && existing.Count > 0)
                {
                    _selectedServices = existing.Select(CloneService).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load services", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        var dlg = new ServiceSelectionWindow(_selectedServices) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            _selectedServices = dlg.SelectedServices.Select(CloneService).ToList();
            UpdateServiceSummary();
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!BindToModel()) return;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
