using System.Windows;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Controls;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class HousekeepingEditWindow : Window
{
    public HousekeepingTask Model { get; private set; }
    private readonly IRoomService _roomService = new RoomService();

    public HousekeepingEditWindow(HousekeepingTask? model = null)
    {
        InitializeComponent();
        Model = model ?? new HousekeepingTask { TaskDate = System.DateTime.Today, Status = "Pending" };
        Loaded += async (_, __) =>
        {
            await LoadRoomsAsync();
            BindFromModel();
        };
    }

    private async Task LoadRoomsAsync()
    {
        string conn = Config.GetConnectionString();
        var rooms = await _roomService.GetRoomsAsync(conn);
        cbRoom.ItemsSource = rooms;
        cbRoom.SelectedValue = Model.RoomId;
        if (cbRoom.SelectedValue == null && rooms.Count > 0)
            cbRoom.SelectedValue = rooms[0].RoomId;
    }

    private void BindFromModel()
    {
        dpDate.SelectedDate = Model.TaskDate;
        txtStaff.Text = Model.StaffName ?? string.Empty;
        foreach (var item in cbStatus.Items)
        {
            if (item is ComboBoxItem cbi && (string)cbi.Content == (Model.Status ?? "Pending"))
            {
                cbStatus.SelectedItem = cbi;
                break;
            }
        }
    }

    private bool BindToModel()
    {
        if (cbRoom.SelectedValue is not int roomId)
        {
            MessageBox.Show("Please select a room", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            cbRoom.Focus();
            return false;
        }
        if (dpDate.SelectedDate is null)
        {
            MessageBox.Show("Please select task date", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            dpDate.Focus();
            return false;
        }
        var status = (cbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pending";
        Model.RoomId = roomId;
        Model.TaskDate = dpDate.SelectedDate.Value.Date;
        Model.StaffName = string.IsNullOrWhiteSpace(txtStaff.Text) ? null : txtStaff.Text.Trim();
        Model.Status = status;
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
