using System.Windows;
using HotelManagementBLL;
using System.Threading.Tasks;

namespace ManagementHotel;

public partial class LoginWindow : Window
{
    public string? Username => txtUser.Text;
    public string? Password => txtPass.Password;
    private readonly IAuthService _auth = new AuthService();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        string user = Username ?? string.Empty;
        string pass = Password ?? string.Empty;
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show("Please enter username and password.", "Login failed" , MessageBoxButton.OK , MessageBoxImage.Information);
            return;
        }
        try
        {
            string conn = Config.GetConnectionString();
            var u = await _auth.LoginAsync(conn, user, pass);
            if (u == null)
            {
                MessageBox.Show("Invalid username or password, or user inactive.", "Login failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            AppSession.SetUser(u);
            RoleContext.SetRole(u.Role);
            RoleContext.SetCustomerId(u.CustomerId);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Login error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new RegisterWindow { Owner = this };
        var ok = dlg.ShowDialog();
        if (ok == true && AppSession.CurrentUser != null)
        {
            // Auto login by registration: session already set in RegisterWindow
            Close();
        }
    }
}
