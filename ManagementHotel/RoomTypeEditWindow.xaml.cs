using System.Globalization;
using System.Windows;
using HotelManagementModels;

namespace ManagementHotel;

public partial class RoomTypeEditWindow : Window
{
    public RoomType Model { get; private set; }

    public RoomTypeEditWindow(RoomType? model = null)
    {
        InitializeComponent();
        Model = model ?? new RoomType();
        Loaded += (_, __) => BindFromModel();
    }

    private void BindFromModel()
    {
        txtName.Text = Model.Name ?? string.Empty;
        txtCapacity.Text = Model.Capacity.ToString(CultureInfo.InvariantCulture);
        txtBasePrice.Text = Model.BasePrice.ToString(CultureInfo.InvariantCulture);
        txtDescription.Text = Model.Description ?? string.Empty;
    }

    private bool BindToModel()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Name is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtName.Focus();
            return false;
        }
        if (!int.TryParse(txtCapacity.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var capacity) || capacity <= 0)
        {
            MessageBox.Show("Capacity must be > 0", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCapacity.Focus();
            return false;
        }
        if (!decimal.TryParse(txtBasePrice.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var basePrice) || basePrice < 0)
        {
            MessageBox.Show("Base Price must be a non-negative number", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtBasePrice.Focus();
            return false;
        }
        Model.Name = txtName.Text.Trim();
        Model.Capacity = capacity;
        Model.BasePrice = basePrice;
        Model.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
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
