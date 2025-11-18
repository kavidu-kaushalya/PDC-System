using Newtonsoft.Json;
using PDC_System.Customer;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for Contact.xaml
    /// </summary>
    public partial class Customers : UserControl
    {
        private List<Customerinfo> customers = new List<Customerinfo>();
        
        private readonly string saversFolder;
        private readonly string jsonFilePath;
        private readonly string jsonFilePath2;

        public Customers()
        {
            InitializeComponent();

            // Set current working directory to a 'Savers' folder
            saversFolder = Path.Combine(Directory.GetCurrentDirectory(), "Savers");
            if (!Directory.Exists(saversFolder))
            {
                Directory.CreateDirectory(saversFolder);
            }

            jsonFilePath = Path.Combine(saversFolder, "customers.json");
            jsonFilePath2 = Path.Combine(saversFolder, "Outsource.json");

            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists(jsonFilePath))
            {
                customers = JsonConvert.DeserializeObject<List<Customerinfo>>(File.ReadAllText(jsonFilePath));
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
                File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(customers, Formatting.Indented));
            }
        }

       

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            var selectedCustomer = CustomerDataGrid.SelectedItem as Customerinfo;
            if (selectedCustomer != null)
            {
                var confirmationDialog = new ConfirmationDialogCustomer(); // Assuming you have this dialog class
                confirmationDialog.Owner = Application.Current.MainWindow;
                confirmationDialog.ShowDialog();

                if (confirmationDialog.IsConfirmed)
                {
                    customers.Remove(selectedCustomer);
                    CustomerDataGrid.Items.Refresh();
                    File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(customers, Formatting.Indented));
                }
            }
        }
    }
}
