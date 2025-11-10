using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel
{
    public partial class CustomerPickerWindow : Window
    {
        private readonly ICustomerService _customerService = new CustomerService();
        private IReadOnlyList<Customer> _allCustomers = Array.Empty<Customer>();

        public Customer? SelectedCustomer { get; private set; }
        public int? SelectedCustomerId { get; private set; }

        public CustomerPickerWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            string conn = Config.GetConnectionString();
            try
            {
                _allCustomers = await _customerService.GetAllAsync(conn);
                dgCustomers.ItemsSource = _allCustomers;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load customers", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allCustomers == null) return;
            string term = txtSearch.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(term))
            {
                dgCustomers.ItemsSource = _allCustomers;
                return;
            }

            term = term.ToLowerInvariant();
            var filtered = _allCustomers.Where(c =>
                (!string.IsNullOrEmpty(c.FullName) && c.FullName.ToLowerInvariant().Contains(term)) ||
                (!string.IsNullOrEmpty(c.Email) && c.Email.ToLowerInvariant().Contains(term)) ||
                (!string.IsNullOrEmpty(c.Phone) && c.Phone.ToLowerInvariant().Contains(term)) ||
                (!string.IsNullOrEmpty(c.IDNumber) && c.IDNumber.ToLowerInvariant().Contains(term))
            ).ToList();

            dgCustomers.ItemsSource = filtered;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer customer)
            {
                SelectedCustomer = customer;
                SelectedCustomerId = customer.CustomerId;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a customer.", "Select", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
