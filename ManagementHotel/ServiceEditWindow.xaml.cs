using System.Globalization;
using System.Windows;
using HotelManagementModels;

namespace ManagementHotel;

public partial class ServiceEditWindow : Window
{
    public Service Model { get; private set; }

    public ServiceEditWindow(Service? model = null)
    {
        InitializeComponent();
        Model = model ?? new Service();
        Loaded += (_, __) => BindFromModel();
    }

    private void BindFromModel()
    {
        txtName.Text = Model.Name ?? string.Empty;
        txtPrice.Text = Model.Price.ToString(CultureInfo.InvariantCulture);
        txtUnit.Text = Model.Unit ?? string.Empty;
    }

    private bool BindToModel()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Name is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtName.Focus();
            return false;
        }
        if (!decimal.TryParse(txtPrice.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price < 0)
        {
            MessageBox.Show("Price must be a non-negative number", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPrice.Focus();
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtUnit.Text))
        {
            MessageBox.Show("Unit is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtUnit.Focus();
            return false;
        }
        Model.Name = txtName.Text.Trim();
        Model.Price = price;
        Model.Unit = txtUnit.Text.Trim();
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
