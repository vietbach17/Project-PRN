using System.Windows;
using HotelManagementBLL;
using System.Threading.Tasks;
using HotelManagementModels;

namespace ManagementHotel;

public partial class RoomsWindow : Window
{
    private readonly IRoomService _roomService = new RoomService();

    public RoomsWindow()
    {
        InitializeComponent();
        Loaded += async (_, __) =>
        {
            UpdateCurrentUserLabel();
            await ReloadAsync();
        };
        ApplyRolePermissions();
    }

    private void UpdateCurrentUserLabel()
    {
        txtCurrentUser.Text = AppSession.CurrentUser?.Username ?? "Guest";
    }

    private async Task ReloadAsync()
    {
        string conn = Config.GetConnectionString();
        try
        {
            var rooms = await _roomService.GetRoomsAsync(conn);
            dgRooms.ItemsSource = rooms;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Load Rooms Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        ApplyRolePermissions();
        UpdateCurrentUserLabel();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await ReloadAsync();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new RoomEditWindow { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            string conn = Config.GetConnectionString();
            try
            {
                await _roomService.AddAsync(conn, dlg.Model);
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Add Room Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (dgRooms.SelectedItem is not Room selected)
        {
            MessageBox.Show("Please select a room.");
            return;
        }
        var clone = new Room
        {
            RoomId = selected.RoomId,
            RoomNumber = selected.RoomNumber,
            Floor = selected.Floor,
            RoomTypeId = selected.RoomTypeId,
            Status = selected.Status,
            PricePerNight = selected.PricePerNight
        };
        var dlg = new RoomEditWindow(clone) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            string conn = Config.GetConnectionString();
            try
            {
                await _roomService.UpdateAsync(conn, dlg.Model);
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Room Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (dgRooms.SelectedItem is not Room selected)
        {
            MessageBox.Show("Please select a room.");
            return;
        }
        if (MessageBox.Show($"Delete room '{selected.RoomNumber}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        string conn = Config.GetConnectionString();
        try
        {
            await _roomService.DeleteAsync(conn, selected.RoomId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete Room Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        btnAdd.IsEnabled = false;
        btnEdit.IsEnabled = false;
        btnDelete.IsEnabled = false;
    }
}
