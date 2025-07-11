
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;


namespace PDC_System
{
    /// <summary>
    /// Interaction logic for Contact.xaml
    /// </summary>
    public partial class Customers : System.Windows.Controls.UserControl
    {
        private List<Customer> customers = new List<Customer>();
        public Customers()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            if (File.Exists("customers.json"))
            {
                customers = JsonConvert.DeserializeObject<List<Customer>>(File.ReadAllText("customers.json"));
                CustomerDataGrid.ItemsSource = customers;
            }
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var addCustomerWindow = new AddCustomerWindow();
            if (addCustomerWindow.ShowDialog() == true)
            {
                customers.Add(addCustomerWindow.Customer);
                CustomerDataGrid.Items.Refresh();
                File.WriteAllText("customers.json", JsonConvert.SerializeObject(customers));
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            var selectedCustomer = CustomerDataGrid.SelectedItem as Customer;
            if (selectedCustomer != null)
            {
                // Show a confirmation dialog
                var confirmationDialog = new ConfirmationDialogCustomer(); // Assuming you have created this dialog class
                confirmationDialog.Owner = Application.Current.MainWindow; // Set the main window as the owner
                confirmationDialog.ShowDialog();

                // If user confirms deletion
                if (confirmationDialog.IsConfirmed)
                {
                    customers.Remove(selectedCustomer);
                    CustomerDataGrid.Items.Refresh();
                    File.WriteAllText("customers.json", JsonConvert.SerializeObject(customers));
                }
            }
        }

    }
}
