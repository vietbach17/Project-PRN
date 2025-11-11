using System.Windows;
using System.Threading.Tasks;
using HotelManagementBLL;
using HotelManagementDAL;
using HotelManagementModels;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace ManagementHotel;

public partial class RegisterWindow : Window
{
    private readonly IUserRepository _users = new UserRepository();
    private readonly ICustomerService _customers = new CustomerService();

    public RegisterWindow()
    {
        InitializeComponent();
    }

    private static string Sha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    private bool ValidateEmail(string email)
        => string.IsNullOrWhiteSpace(email) || Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string pass = txtPassword.Password;
        string confirm = txtConfirm.Password;
        string fullName = txtFullName.Text.Trim();
        string email = txtEmail.Text.Trim();
        string phone = txtPhone.Text.Trim();
        string idNumber = txtIDNumber.Text.Trim();
        string address = txtAddress.Text.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Username, password and full name are required.");
            return;
        }
        if (pass != confirm)
        {
            MessageBox.Show("Password confirmation does not match.");
            return;
        }
        if (!ValidateEmail(email))
        {
            MessageBox.Show("Invalid email format.");
            return;
        }

        try
        {
            string conn = Config.GetConnectionString();
            if (await _users.UsernameExistsAsync(conn, username))
            {
                MessageBox.Show("Username already exists.");
                return;
            }

            // Create customer first
            var customer = new Customer
            {
                FullName = fullName,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                IDNumber = string.IsNullOrWhiteSpace(idNumber) ? null : idNumber,
                Address = string.IsNullOrWhiteSpace(address) ? null : address
            };
            int customerId = await _customers.AddForRegistrationAsync(conn, customer);

            // Create user mapped to this customer
            var user = new User
            {
                Username = username,
                PasswordHash = Sha256(pass),
                Role = "Customer",
                IsActive = true,
                CustomerId = customerId
            };
            int userId = await _users.AddAsync(conn, user);

            // Auto login
            user.UserId = userId;
            AppSession.SetUser(user);
            RoleContext.SetRole(user.Role);
            RoleContext.SetCustomerId(user.CustomerId);
            DialogResult = true;
            Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message, "Register error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
