using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HotelManagementBLL;
using HotelManagementModels;

namespace ManagementHotel
{
    public partial class CustomerPickerWindow : Window
    {
        private readonly ICustomerService _customerService = new CustomerService();
        private IReadOnlyList<Customer> _customers = Array.Empty<Customer>();
        public Customer? SelectedCustomer { get; private set; }

        public CustomerPickerWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            string conn = Config.GetConnectionString();
            _customers = await _customerService.GetAllAsync(conn);
            ApplyFilter(txtSearch.Text);
        }

        private void ApplyFilter(string? keyword)
        {
            if (_customers is null)
            {
                dgCustomers.ItemsSource = null;
                return;
            }
            if (string.IsNullOrWhiteSpace(keyword))
            {
                dgCustomers.ItemsSource = _customers;
                return;
            }
            keyword = keyword.Trim().ToLowerInvariant();
            var filtered = _customers
                .Where(c => (!string.IsNullOrEmpty(c.FullName) && c.FullName.ToLowerInvariant().Contains(keyword))
                            || (!string.IsNullOrEmpty(c.Phone) && c.Phone.ToLowerInvariant().Contains(keyword))
                            || (!string.IsNullOrEmpty(c.Email) && c.Email.ToLowerInvariant().Contains(keyword))
                            || (!string.IsNullOrEmpty(c.IDNumber) && c.IDNumber.ToLowerInvariant().Contains(keyword)))
                .ToList();
            dgCustomers.ItemsSource = filtered;
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            if (_customers.Count == 0)
            {
                await LoadAsync();
            }
            else
            {
                ApplyFilter(txtSearch.Text);
            }
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
            await LoadAsync();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrent();
        }

        private void dgCustomers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectCurrent();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilter(txtSearch.Text);
                e.Handled = true;
            }
        }

        private void SelectCurrent()
        {
            if (dgCustomers.SelectedItem is Customer customer)
            {
                SelectedCustomer = customer;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a customer.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
