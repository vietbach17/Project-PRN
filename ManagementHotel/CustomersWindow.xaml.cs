using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class CustomersWindow : Window
{
    private readonly ICustomerService _service = new CustomerService();

    public CustomersWindow()
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
            MessageBox.Show(ex.Message, "Load Customers Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        ApplyRolePermissions();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await ReloadAsync();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new CustomerEditWindow { Owner = this };
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
                MessageBox.Show(ex.Message, "Add Customer Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not Customer selected)
        {
            MessageBox.Show("Please select a customer.");
            return;
        }
        var clone = new Customer
        {
            CustomerId = selected.CustomerId,
            FullName = selected.FullName,
            Phone = selected.Phone,
            Email = selected.Email,
            IDNumber = selected.IDNumber,
            Address = selected.Address
        };
        var dlg = new CustomerEditWindow(clone) { Owner = this };
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
                MessageBox.Show(ex.Message, "Update Customer Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not Customer selected)
        {
            MessageBox.Show("Please select a customer.");
            return;
        }
        if (MessageBox.Show($"Delete customer '{selected.FullName}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        string conn = Config.GetConnectionString();
        try
        {
            await _service.DeleteAsync(conn, selected.CustomerId);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete Customer Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
