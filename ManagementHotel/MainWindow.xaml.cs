using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IRoomService _roomService = new RoomService();
        private readonly IBookingService _bookingService = new BookingService();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, __) => ApplyRolePermissions();
            Loaded += async (_, __) => await LoadRoomsForCustomerAsync();
        }

        private async void btnLoad_Click(object sender, RoutedEventArgs e)
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
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenLogin_Click(object sender, RoutedEventArgs e)
        {
            var w = new LoginWindow { Owner = this };
            _ = w.ShowDialog();
        }

        private void OpenRooms_Click(object sender, RoutedEventArgs e)
        {
            var w = new RoomsWindow { Owner = this };
            w.Show();
        }

        private void OpenRoomTypes_Click(object sender, RoutedEventArgs e)
        {
            var w = new RoomTypesWindow { Owner = this };
            w.Show();
        }

        private void OpenCustomers_Click(object sender, RoutedEventArgs e)
        {
            var w = new CustomersWindow { Owner = this };
            w.Show();
        }

        private void OpenBookings_Click(object sender, RoutedEventArgs e)
        {
            var w = new BookingsWindow { Owner = this };
            w.Show();
        }

        private void OpenServices_Click(object sender, RoutedEventArgs e)
        {
            var w = new ServicesWindow { Owner = this };
            w.Show();
        }

        private void OpenInvoices_Click(object sender, RoutedEventArgs e)
        {
            var w = new InvoicesWindow { Owner = this };
            w.Show();
        }

        private void OpenHousekeeping_Click(object sender, RoutedEventArgs e)
        {
            var w = new HousekeepingWindow { Owner = this };
            w.Show();
        }

        private void OpenProfile_Click(object sender, RoutedEventArgs e)
        {
            var w = new ProfileWindow { Owner = this };
            w.ShowDialog();
        }

        private void ApplyRolePermissions()
        {
            // If Customer: only allow Bookings menu; hide management menus and main room UI
            if (AppSession.IsCustomer)
            {
                miProfile.Visibility = Visibility.Visible;
                btnMyBookings.Visibility = Visibility.Visible;
                miRooms.Visibility = Visibility.Collapsed;
                miRoomTypes.Visibility = Visibility.Collapsed;
                miCustomers.Visibility = Visibility.Collapsed;
                miServices.Visibility = Visibility.Collapsed;
                miInvoices.Visibility = Visibility.Collapsed;
                miHousekeeping.Visibility = Visibility.Collapsed;

                // Show rooms list with quick booking
                btnLoad.Visibility = Visibility.Collapsed;
                dgRooms.Visibility = Visibility.Visible;
                colBook.Visibility = Visibility.Visible;
                // Do not show other customers' names to a customer
                colCurrentCustomer.Visibility = Visibility.Collapsed;
            }
            else
            {
                miProfile.Visibility = Visibility.Visible;
                btnMyBookings.Visibility = Visibility.Collapsed;
                miRooms.Visibility = Visibility.Visible;
                miRoomTypes.Visibility = Visibility.Visible;
                miCustomers.Visibility = Visibility.Visible;
                miServices.Visibility = Visibility.Visible;
                miInvoices.Visibility = Visibility.Visible;
                miHousekeeping.Visibility = Visibility.Visible;
                colBook.Visibility = Visibility.Collapsed;
                btnLoad.Visibility = Visibility.Visible;
                dgRooms.Visibility = Visibility.Visible;
                // Staff/Admin can see current customer name
                colCurrentCustomer.Visibility = Visibility.Visible;
            }
        }

        private async Task LoadRoomsForCustomerAsync()
        {
            if (!AppSession.IsCustomer) return;
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
        }

        private async void BookRoom_Click(object sender, RoutedEventArgs e)
        {
            if (dgRooms.SelectedItem is not Room room) return;
            // Prepare booking with selected room
            var booking = new Booking
            {
                RoomId = room.RoomId,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1),
                Status = "Reserved"
            };
            var dlg = new BookingEditWindow(booking) { Owner = this };
            var ok = dlg.ShowDialog();
            if (ok == true)
            {
                string conn = Config.GetConnectionString();
                try
                {
                    await _bookingService.AddAsync(conn, dlg.Model);
                    await LoadRoomsForCustomerAsync();
                    MessageBox.Show("Booked successfully.", "Booking", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Booking Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}