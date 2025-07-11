using PDC_System;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PDC_System
{
    public partial class AddJobCardWindow : Window
    {
        public JobCard JobCard { get; private set; }

        public AddJobCardWindow(List<Customer> customers)
        {
            InitializeComponent();
     
            CustomerComboBox.ItemsSource = customers;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Validate GSM input
            if (!int.TryParse(GSMTextBox.Text, out int gsm))
            {
                MessageBox.Show("Please enter a valid number for GSM.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Quantity input
            if (!int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                MessageBox.Show("Please enter a valid number for Quantity.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate Printed input
            if (!int.TryParse(PrintedTextBox.Text, out int printed))
            {
                MessageBox.Show("Please enter a valid number for Printed.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get the customer name: Check if a customer is selected, or use the manually typed name
            string customerName = (CustomerComboBox.SelectedItem as Customer)?.Name;

            if (string.IsNullOrEmpty(customerName))
            {
                customerName = CustomerComboBox.Text;  // Use manually entered name
            }

            // Create the JobCard object
            JobCard = new JobCard
            {
                Customer_Name = customerName,
                Paper_Size = PaperSizeTextBox.Text,
                Description = DescriptionTextBox.Text,
                GSM = gsm,
                Duplex = DsTextBox.Text,
                Laminate = LaminateTextBox.Text,
                Special_Note = SpecialTextBox.Text,
                Paper_Type = PaperTypeTextBox.Text,
                Quantity = quantity,
                Printed = printed
            };

            // Close the window and return true
            DialogResult = true;

        }

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Check if the input is a number
            e.Handled = !char.IsDigit(e.Text, 0);
        }

    }
}