using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class HousekeepingWindow : Window
{
    private readonly IHousekeepingService _service = new HousekeepingService();

    public HousekeepingWindow()
    {
        InitializeComponent();
        Loaded += async (_, __) => await ReloadAsync();
        ApplyRolePermissions();
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
            MessageBox.Show(ex.Message, "Load Housekeeping Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        ApplyRolePermissions();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await ReloadAsync();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new HousekeepingEditWindow { Owner = this };
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
                MessageBox.Show(ex.Message, "Add Task Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not HousekeepingTask selected)
        {
            MessageBox.Show("Please select a task.");
            return;
        }
        var clone = new HousekeepingTask
        {
            TaskId = selected.TaskId,
            RoomId = selected.RoomId,
            TaskDate = selected.TaskDate,
            StaffName = selected.StaffName,
            Status = selected.Status,
            Notes = selected.Notes
        };
        var dlg = new HousekeepingEditWindow(clone) { Owner = this };
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
                MessageBox.Show(ex.Message, "Update Task Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not HousekeepingTask selected)
        {
            MessageBox.Show("Please select a task.");
            return;
        }
        if (MessageBox.Show($"Delete task for room '{selected.RoomNumber}' on {selected.TaskDate:d}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        string conn = Config.GetConnectionString();
        try
        {
            await _service.DeleteAsync(conn, selected.TaskId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete Task Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
