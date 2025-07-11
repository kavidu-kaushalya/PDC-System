using System.Windows;
using System.Windows.Controls;

namespace PDC_System
{
    public partial class AddCustomerWindow : Window
    {
        public Customer Customer { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public AddCustomerWindow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Customer = new Customer
            {
                Name = NameTextBox.Text,
                Address = AddressTextBox.Text,
                ContactNo = ContactNoTextBox.Text,
                Email= EmailTextBox.Text,
                cp = CP.Text,
            };
            DialogResult = true;
        }

        private void ContactNoTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only digits
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void ContactNoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ensure the text length does not exceed 10 characters
            if (ContactNoTextBox.Text.Length > 10)
            {
                ContactNoTextBox.Text = ContactNoTextBox.Text.Substring(0, 10);
                ContactNoTextBox.SelectionStart = ContactNoTextBox.Text.Length; // Keep the cursor at the end
            }
        }

    }
}