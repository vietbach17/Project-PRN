using System.Windows;
using HotelManagementBLL;

namespace ManagementHotel
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly IAuthService _authService = new AuthService();

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string current = txtCurrent.Password;
            string next = txtNew.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(current) || string.IsNullOrEmpty(next))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            try
            {
                string conn = Config.GetConnectionString();
                bool ok = await _authService.ChangePasswordAsync(conn, username, current, next);
                if (!ok)
                {
                    MessageBox.Show("Unable to change password. Verify your current password.", "Change password", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show("Password updated.", "Change password", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Change password error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
