using System.Windows;
using HotelManagementBLL;

namespace ManagementHotel
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly IAuthService _authService = new AuthService();

        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string idNumber = txtIDNumber.Text.Trim();
            string newPass = txtNewPassword.Password;
            string confirm = txtConfirm.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(idNumber) || string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }
            if (newPass != confirm)
            {
                MessageBox.Show("Password confirmation does not match.");
                return;
            }

            try
            {
                string conn = Config.GetConnectionString();
                bool ok = await _authService.ResetPasswordAsync(conn, username, idNumber, newPass);
                if (!ok)
                {
                    MessageBox.Show("Unable to reset password. Check your information and try again.", "Reset password", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show("Password has been reset.", "Reset password", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Reset password error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
