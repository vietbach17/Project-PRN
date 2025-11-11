using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel;

public partial class ServiceSelectionWindow : Window
{
    private readonly IServiceService _serviceService = new ServiceService();
    private readonly IReadOnlyList<BookingServiceItem> _initialSelection;
    private ObservableCollection<BookingServiceItem> _items = new();

    public IList<BookingServiceItem> SelectedServices { get; private set; } = new List<BookingServiceItem>();

    public ServiceSelectionWindow(IEnumerable<BookingServiceItem>? selected = null)
    {
        _initialSelection = selected?.Select(CloneItem).ToList() ?? new List<BookingServiceItem>();
        InitializeComponent();
        Loaded += async (_, __) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        string conn = Config.GetConnectionString();
        try
        {
            var services = await _serviceService.GetAllAsync(conn);
            var list = new List<BookingServiceItem>();
            foreach (var svc in services)
            {
                var match = _initialSelection.FirstOrDefault(i => i.ServiceId == svc.ServiceId);
                list.Add(new BookingServiceItem
                {
                    ServiceId = svc.ServiceId,
                    ServiceName = svc.Name,
                    Unit = svc.Unit,
                    UnitPrice = svc.Price,
                    Quantity = match?.Quantity ?? 0
                });
            }
            _items = new ObservableCollection<BookingServiceItem>(list);
            dgServices.ItemsSource = _items;
            UpdateTotal();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Load services", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static BookingServiceItem CloneItem(BookingServiceItem item) => new()
    {
        BookingServiceId = item.BookingServiceId,
        BookingId = item.BookingId,
        ServiceId = item.ServiceId,
        ServiceName = item.ServiceName,
        Unit = item.Unit,
        UnitPrice = item.UnitPrice,
        Quantity = item.Quantity
    };

    private void UpdateTotal()
    {
        decimal total = _items.Sum(i => i.Total);
        txtTotal.Text = total.ToString("N0");
    }

    private static readonly Regex DigitsOnly = new("^[0-9]+$", RegexOptions.Compiled);

    private void Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !DigitsOnly.IsMatch(e.Text);
    }

    private void Quantity_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var text = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;
            if (!DigitsOnly.IsMatch(text))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is BookingServiceItem item)
        {
            if (!int.TryParse(tb.Text, out int qty))
            {
                qty = 0;
            }
            item.Quantity = qty;
            CollectionViewSource.GetDefaultView(dgServices.ItemsSource)?.Refresh();
            UpdateTotal();
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SelectedServices = _items.Where(i => i.Quantity > 0)
            .Select(CloneItem)
            .ToList();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
