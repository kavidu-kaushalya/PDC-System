using System.Windows;
using System.Windows.Controls;

namespace PDC_System
{
    public partial class AddCustomerWindow : Window
    {
        public Customerinfo Customer { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public AddCustomerWindow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            InitializeComponent();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Customer = new Customerinfo
            {
                Name = NameTextBox.Text,
                Address = AddressTextBox.Text,
                ContactNo = ContactNoTextBox.Text,
                Email = EmailTextBox.Text,
                Type = CP.IsChecked == true ? "Company" : PersonRB.IsChecked == true ? "Person" : "",
                companyname = CP.IsChecked == true ? CompanyTextBox.Text : null // Save only if Company selected
            };
            DialogResult = true;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (CP.IsChecked == true)
            {
                CompanyLabel.Visibility = Visibility.Visible;
                CompanyTextBox.Visibility = Visibility.Visible;
            }
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (CP.IsChecked != true)
            {
                CompanyLabel.Visibility = Visibility.Collapsed;
                CompanyTextBox.Visibility = Visibility.Collapsed;
                CompanyTextBox.Text = string.Empty; // Clear the text
            }
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