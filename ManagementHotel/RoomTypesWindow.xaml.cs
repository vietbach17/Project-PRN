using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class RoomTypesWindow : Window
{
    private readonly IRoomTypeService _service = new RoomTypeService();

    public RoomTypesWindow()
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
            var items = await _service.GetAllAsync(conn);
            dg.ItemsSource = items;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Load Room Types Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        var dlg = new RoomTypeEditWindow { Owner = this };
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
                MessageBox.Show(ex.Message, "Add Room Type Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not RoomType selected)
        {
            MessageBox.Show("Please select a room type.");
            return;
        }
        var clone = new RoomType
        {
            RoomTypeId = selected.RoomTypeId,
            Name = selected.Name,
            Capacity = selected.Capacity,
            BasePrice = selected.BasePrice,
            Description = selected.Description
        };
        var dlg = new RoomTypeEditWindow(clone) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            string conn = Config.GetConnectionString();
            try
            {
                await _service.UpdateAsync(conn, dlg.Model);
                await ReloadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Update Room Type Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not RoomType selected)
        {
            MessageBox.Show("Please select a room type.");
            return;
        }
        if (MessageBox.Show($"Delete room type '{selected.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        string conn = Config.GetConnectionString();
        try
        {
            await _service.DeleteAsync(conn, selected.RoomTypeId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete Room Type Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
