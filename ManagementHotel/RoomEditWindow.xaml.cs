using System.Globalization;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class RoomEditWindow : Window
{
    public Room Model { get; private set; }
    private readonly IRoomTypeService _roomTypeService = new RoomTypeService();

    public RoomEditWindow(Room? model = null)
    {
        InitializeComponent();
        Model = model ?? new Room { Status = "Available" };
        Loaded += async (_, __) =>
        {
            await LoadRoomTypesAsync();
            BindFromModel();
        };
    }

    private async Task LoadRoomTypesAsync()
    {
        string conn = Config.GetConnectionString();
        var types = await _roomTypeService.GetAllAsync(conn);
        cbRoomType.ItemsSource = types;
        cbRoomType.SelectedValue = Model.RoomTypeId == 0 && types.Count > 0 ? types[0].RoomTypeId : Model.RoomTypeId;
    }

    private void BindFromModel()
    {
        txtRoomNumber.Text = Model.RoomNumber ?? string.Empty;
        txtFloor.Text = Model.Floor?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        foreach (var item in cbStatus.Items)
        {
            if (item is ComboBoxItem cbi && (string)cbi.Content == (Model.Status ?? "Available"))
            {
                cbStatus.SelectedItem = cbi;
                break;
            }
        }
        txtPrice.Text = Model.PricePerNight.ToString(CultureInfo.InvariantCulture);
    }

    private bool BindToModel()
    {
        if (string.IsNullOrWhiteSpace(txtRoomNumber.Text))
        {
            MessageBox.Show("Room Number is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtRoomNumber.Focus();
            return false;
        }
        int floor = 0;
        if (!string.IsNullOrWhiteSpace(txtFloor.Text))
        {
            if (!int.TryParse(txtFloor.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out floor))
            {
                MessageBox.Show("Floor must be an integer", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFloor.Focus();
                return false;
            }
        }
        if (!decimal.TryParse(txtPrice.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price < 0)
        {
            MessageBox.Show("Price/Night must be a non-negative number", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPrice.Focus();
            return false;
        }
        if (cbRoomType.SelectedValue is not int roomTypeId)
        {
            MessageBox.Show("Please select a Room Type", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            cbRoomType.Focus();
            return false;
        }
        var status = (cbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Available";
        Model.RoomNumber = txtRoomNumber.Text.Trim();
        Model.Floor = string.IsNullOrWhiteSpace(txtFloor.Text) ? null : floor;
        Model.RoomTypeId = roomTypeId;
        Model.Status = status;
        Model.PricePerNight = price;
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
