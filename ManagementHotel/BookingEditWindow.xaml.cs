using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class BookingEditWindow : Window
{
    public Booking Model { get; private set; }
    private readonly ICustomerService _customerService = new CustomerService();
    private readonly IRoomService _roomService = new RoomService();
    private Customer? _selectedCustomer;
    private bool _isInitialized;

    public BookingEditWindow(Booking? model = null)
    {
        InitializeComponent();
        Model = model ?? new Booking { Status = "Pending", CheckInDate = System.DateTime.Today, CheckOutDate = System.DateTime.Today.AddDays(1) };
        Loaded += async (_, __) =>
        {
            await LoadCustomersAsync();
            await LoadRoomsAsync();
            BindFromModel();
            if (Model.CustomerId <= 0 && AppSession.CurrentUser?.CustomerId is int sessionCid && sessionCid > 0)
            {
                await SetCustomerAsync(sessionCid);
            }
            // lock customer selection for Customer role
            if (AppSession.IsCustomer)
            {
                btnSelectCustomer.IsEnabled = false;
            }
            _isInitialized = true;
            UpdateTotal();
        };
    }

    private async Task LoadCustomersAsync()
    {
        string conn = Config.GetConnectionString();
        var customers = await _customerService.GetAllAsync(conn);
        cbCustomer.ItemsSource = customers;
        cbCustomer.SelectedValue = Model.CustomerId == 0 && customers.Count > 0 ? customers[0].CustomerId : Model.CustomerId;
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
        foreach (var item in cbStatus.Items)
        {
            if (item is ComboBoxItem cbi && (string)cbi.Content == (Model.Status ?? "Pending"))
            {
                cbStatus.SelectedItem = cbi;
                break;
            }
        }
        txtNotes.Text = Model.Notes ?? string.Empty;
        var guests = Model.Guests > 0 ? Model.Guests : 1;
        Model.Guests = guests;
        txtGuests.Text = guests.ToString();
        if (Model.TotalDue > 0)
        {
            txtTotal.Text = FormatCurrency(Model.TotalDue);
        }
    }

    private bool BindToModel()
    {
        if (cbCustomer.SelectedValue is not int customerId)
        {
            MessageBox.Show("Please select a customer", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            cbCustomer.Focus();
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
        if (!int.TryParse(txtGuests.Text, out var guests) || guests <= 0)
        {
            MessageBox.Show("Vui lòng nhập số người hợp lệ", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtGuests.Focus();
            return false;
        }
        var status = (cbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pending";
        Model.CustomerId = customerId;
        Model.RoomId = roomId;
        Model.CheckInDate = ci;
        Model.CheckOutDate = co;
        Model.Status = status;
        Model.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
        Model.Guests = guests;
        Model.TotalDue = CalculateTotal();
        return true;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!BindToModel()) return;
        UpdateTotal();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        UpdateTotal();
    }

    private void Room_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialized) return;
        UpdateTotal();
    }

    private void Guests_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        foreach (var ch in e.Text)
        {
            if (!char.IsDigit(ch))
            {
                e.Handled = true;
                return;
            }
        }
    }

    private void Guests_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_isInitialized) return;
        if (int.TryParse(txtGuests.Text, out var guests) && guests > 0)
        {
            Model.Guests = guests;
        }
    }

    private decimal CalculateTotal()
    {
        if (dpCheckIn.SelectedDate is not System.DateTime checkIn || dpCheckOut.SelectedDate is not System.DateTime checkOut)
            return 0m;
        if (cbRoom.SelectedItem is not Room room)
            return 0m;
        var nights = (checkOut.Date - checkIn.Date).Days;
        if (nights < 1)
            nights = 1;
        return nights * room.PricePerNight;
    }

    private void UpdateTotal()
    {
        var total = CalculateTotal();
        txtTotal.Text = FormatCurrency(total);
        if (_isInitialized)
        {
            Model.TotalDue = total;
        }
    }

    private static string FormatCurrency(decimal amount)
    {
        if (amount <= 0)
            return "0 ₫";
        return string.Concat(amount.ToString("N0", CultureInfo.CurrentCulture), " ₫");
    }
}
