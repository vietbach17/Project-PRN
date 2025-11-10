using System.Windows;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using HotelManagementBLL;
using HotelManagementDAL;

namespace ManagementHotel;

public partial class ProfileWindow : Window
{
    private readonly IUserRepository _usersRepo = new UserRepository();
    private readonly ICustomerService _customerService = new CustomerService();

    public ProfileWindow()
    {
        InitializeComponent();
        Loaded += async (_, __) => await LoadAsync();
    }

static string ComputeSha256Hex(string input)
{
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
    var sb = new StringBuilder(bytes.Length * 2);
    foreach (var b in bytes) sb.Append(b.ToString("x2"));
    return sb.ToString();
}

static bool LooksLikeSha256(string s)
    => !string.IsNullOrEmpty(s) && s.Length == 64 && IsHex(s);

static bool IsHex(string s)
{
    for (int i = 0; i < s.Length; i++)
    {
        char c = s[i];
        bool ok = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        if (!ok) return false;
    }
    return true;
}

static bool VerifyPasswordLocal(string storedHashOrPlain, string inputPlain)
{
    var stored = (storedHashOrPlain ?? string.Empty).Trim();
    var input = (inputPlain ?? string.Empty);
    if (LooksLikeSha256(stored))
    {
        var hash = ComputeSha256Hex(input);
        return string.Equals(stored, hash, StringComparison.OrdinalIgnoreCase);
    }
    return string.Equals(stored, input, StringComparison.Ordinal);
}

    private async Task LoadAsync()
    {
        if (AppSession.CurrentUser == null)
        {
            MessageBox.Show("No user in session");
            Close();
            return;
        }
        var u = AppSession.CurrentUser;
        txtUsername.Text = u.Username;
        txtCurrentPass.Password = string.Empty;
        txtNewPass.Password = string.Empty;
        txtConfirm.Password = string.Empty;
        expPassword.IsExpanded = false;
        // Load customer info if mapped
        if (u.CustomerId is int cid && cid > 0)
        {
            string conn = Config.GetConnectionString();
            var c = await _customerService.GetByIdAsync(conn, cid);
            if (c != null)
            {
                txtFullName.Text = c.FullName ?? string.Empty;
                txtEmail.Text = c.Email ?? string.Empty;
                txtPhone.Text = c.Phone ?? string.Empty;
                txtIDNumber.Text = c.IDNumber ?? string.Empty;
                txtAddress.Text = c.Address ?? string.Empty;
            }
        }
        else
        {
            txtFullName.IsEnabled = false;
            txtEmail.IsEnabled = false;
            txtPhone.IsEnabled = false;
            txtIDNumber.IsEnabled = false;
            txtAddress.IsEnabled = false;
        }
    }

    private bool ValidateEmail(string email)
        => string.IsNullOrWhiteSpace(email) || Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (AppSession.CurrentUser == null) return;
        string conn = Config.GetConnectionString();
        var u = AppSession.CurrentUser;

        // Username change
        string newUsername = txtUsername.Text.Trim();
        if (string.IsNullOrWhiteSpace(newUsername))
        {
            MessageBox.Show("Username is required");
            return;
        }

        // Password change (optional)
        string newPass = txtNewPass.Password;
        string confirm = txtConfirm.Password;
        if (!string.IsNullOrEmpty(newPass) || !string.IsNullOrEmpty(confirm))
        {
            // require current password to change
            string current = txtCurrentPass.Password;
            if (string.IsNullOrEmpty(current))
            {
                MessageBox.Show("Please enter your current password to change it.");
                return;
            }
            if (!VerifyPasswordLocal(u.PasswordHash, current))
            {
                MessageBox.Show("Current password is incorrect.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (newPass != confirm)
            {
                MessageBox.Show("Password confirmation does not match");
                return;
            }
        }

        // Email validate
        string email = txtEmail.Text.Trim();
        if (!ValidateEmail(email))
        {
            MessageBox.Show("Invalid email format");
            return;
        }

        string idNumber = txtIDNumber.Text.Trim();
        string address = txtAddress.Text.Trim();
        string phone = txtPhone.Text.Trim();
        string fullName = txtFullName.Text.Trim();

        try
        {
            // Update username if changed
            if (!string.Equals(u.Username, newUsername, System.StringComparison.Ordinal))
            {
                await _usersRepo.UpdateUsernameAsync(conn, u.UserId, newUsername);
                u.Username = newUsername;
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(newPass))
            {
                var hash = ComputeSha256Hex(newPass);
                await _usersRepo.UpdatePasswordHashAsync(conn, u.UserId, hash);
                u.PasswordHash = hash;
            }

            // Update customer info if mapped
            if (u.CustomerId is int cid && cid > 0)
            {
                var c = await _customerService.GetByIdAsync(conn, cid);
                if (c != null)
                {
                    if (!string.IsNullOrWhiteSpace(fullName))
                        c.FullName = fullName;
                    c.Email = string.IsNullOrWhiteSpace(email) ? null : email;
                    c.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
                    c.IDNumber = string.IsNullOrWhiteSpace(idNumber) ? null : idNumber;
                    c.Address = string.IsNullOrWhiteSpace(address) ? null : address;
                    await _customerService.UpdateAsync(conn, c);
                }
            }

            MessageBox.Show("Saved");
            Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message, "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
