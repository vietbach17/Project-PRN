using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class InvoicesWindow : Window
{
    private readonly IInvoiceService _service = new InvoiceService();

    public InvoicesWindow()
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
            MessageBox.Show(ex.Message, "Load Invoices Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        ApplyRolePermissions();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await ReloadAsync();
    }

    private async void Recalculate_Click(object sender, RoutedEventArgs e)
    {
        if (dg.SelectedItem is not InvoiceListItem selected)
        {
            MessageBox.Show("Please select an invoice.");
            return;
        }
        string conn = Config.GetConnectionString();
        try
        {
            var ok = await _service.RecalculateTotalsAsync(conn, selected.InvoiceId);
            if (!ok) MessageBox.Show("No invoice updated.");
            await ReloadAsync();
        }
        catch (UnauthorizedAccessException uex)
        {
            MessageBox.Show(uex.Message, "Permission", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Recalculate Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyRolePermissions()
    {
        // Only Admin/Staff can recalc
        btnRecalc.IsEnabled = AppSession.IsAdmin || AppSession.IsStaff;
    }
}
