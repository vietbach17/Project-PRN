using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;
using System.Windows.Controls;

namespace ManagementHotel;

public partial class BookingEditWindow : Window
{
    public Booking Model { get; private set; }
    private readonly ICustomerService _customerService = new CustomerService();
    private readonly IRoomService _roomService = new RoomService();

    public BookingEditWindow(Booking? model = null)
    {
        InitializeComponent();
        Model = model ?? new Booking { Status = "Pending", CheckInDate = System.DateTime.Today, CheckOutDate = System.DateTime.Today.AddDays(1) };
        Loaded += async (_, __) =>
        {
            await LoadCustomersAsync();
            await LoadRoomsAsync();
            BindFromModel();
            // lock customer selection for Customer role
            if (AppSession.IsCustomer && AppSession.CurrentUser?.CustomerId is int cid && cid > 0)
            {
                cbCustomer.SelectedValue = cid;
                cbCustomer.IsEnabled = false;
                Model.CustomerId = cid;
            }
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
        var status = (cbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pending";
        Model.CustomerId = customerId;
        Model.RoomId = roomId;
        Model.CheckInDate = ci;
        Model.CheckOutDate = co;
        Model.Status = status;
        Model.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
        return true;
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
