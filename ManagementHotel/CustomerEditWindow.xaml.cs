using System.Windows;
using HotelManagementModels;

namespace ManagementHotel;

public partial class CustomerEditWindow : Window
{
    public Customer Model { get; private set; }

    public CustomerEditWindow(Customer? model = null)
    {
        InitializeComponent();
        Model = model ?? new Customer();
        Loaded += (_, __) => BindFromModel();
    }

    private void BindFromModel()
    {
        txtFullName.Text = Model.FullName ?? string.Empty;
        txtPhone.Text = Model.Phone ?? string.Empty;
        txtEmail.Text = Model.Email ?? string.Empty;
        txtIDNumber.Text = Model.IDNumber ?? string.Empty;
        txtAddress.Text = Model.Address ?? string.Empty;
    }

    private bool BindToModel()
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text))
        {
            MessageBox.Show("Full name is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtFullName.Focus();
            return false;
        }
        Model.FullName = txtFullName.Text.Trim();
        Model.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
        Model.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
        Model.IDNumber = string.IsNullOrWhiteSpace(txtIDNumber.Text) ? null : txtIDNumber.Text.Trim();
        Model.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
        return true;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!BindToModel()) return;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
