using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class ServicesWindow : Window
{
    private readonly IServiceService _service = new ServiceService();

    public ServicesWindow()
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
            MessageBox.Show(ex.Message, "Load Services Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        ApplyRolePermissions();
        UpdateCurrentUserLabel();
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
            btnDelete.IsEnabled = false; // Staff cannot delete
            return;
        }
        // Customer: read-only
        btnAdd.IsEnabled = false;
        btnEdit.IsEnabled = false;
        btnDelete.IsEnabled = false;
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await ReloadAsync();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ServiceEditWindow { Owner = this };
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
                MessageBox.Show(ex.Message, "Add Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not Service selected)
        {
            MessageBox.Show("Please select a service.");
            return;
        }
        var clone = new Service { ServiceId = selected.ServiceId, Name = selected.Name, Price = selected.Price, Unit = selected.Unit };
        var dlg = new ServiceEditWindow(clone) { Owner = this };
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
                MessageBox.Show(ex.Message, "Update Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not Service selected)
        {
            MessageBox.Show("Please select a service.");
            return;
        }
        if (MessageBox.Show($"Delete service '{selected.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        string conn = Config.GetConnectionString();
        try
        {
            await _service.DeleteAsync(conn, selected.ServiceId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
