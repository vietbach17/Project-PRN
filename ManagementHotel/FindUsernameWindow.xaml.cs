using System.Windows;
using HotelManagementBLL;

namespace ManagementHotel
{
    public partial class FindUsernameWindow : Window
    {
        private readonly IAuthService _authService = new AuthService();

        public FindUsernameWindow()
        {
            InitializeComponent();
        }

        private async void Find_Click(object sender, RoutedEventArgs e)
        {
            string idNumber = txtIDNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(idNumber))
            {
                MessageBox.Show("Please enter ID Number.");
                return;
            }

            try
            {
                string conn = Config.GetConnectionString();
                string? username = await _authService.FindUsernameByIdNumberAsync(conn, idNumber);
                if (string.IsNullOrEmpty(username))
                {
                    txtResult.Visibility = Visibility.Visible;
                    txtResult.Text = "No account found.";
                }
                else
                {
                    txtResult.Visibility = Visibility.Visible;
                    txtResult.Text = $"Username: {username}";
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Find username error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
