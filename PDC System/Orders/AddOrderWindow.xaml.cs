using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PDC_System.Orders;

namespace PDC_System
{
    public partial class AddOrderWindow : Window
    {
        public Order NewOrder { get; private set; }

        public AddOrderWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Parse the date from the DatePicker
            DateTime dueDate = DueDatePicker.SelectedDate ?? DateTime.Now;

            // Parse the time from the TextBox
            if (DateTime.TryParseExact(DueTimeTextBox.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dueTime))
            {
                // Combine date and time
                DateTime dueDateTime = dueDate.Date + dueTime.TimeOfDay;

                // Create the new order
                NewOrder = new Order
                {
                    CreateDate = DateTime.Now, // Store the current date and time
                    DueDate = dueDateTime,
                    CustomerName = CustomerNameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Notes = NotesTextBox.Text
                };

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid time format. Please use HH:mm (e.g., 14:30).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void DueTimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits (0-9)
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void DueTimeTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;
            string text = textBox.Text;

            // If the user is typing the hour (before entering minutes), ensure the hour is two digits
            if (text.Length == 1 && char.IsDigit(e.Key.ToString(), 0))
            {
                textBox.Text = text.PadLeft(2, '0') + ":";
                textBox.SelectionStart = textBox.Text.Length; // Move cursor to the end
            }
        }

        private void DueTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string text = textBox.Text;

            // If the text is empty or null, set it to 00:00
            if (string.IsNullOrEmpty(text))
            {
                textBox.Text = "00:00";
                return;
            }

            // Remove any non-numeric characters (though it should only be digits now)
            text = new string(text.Where(char.IsDigit).ToArray());

            // Validate and format the hour (valid range 00-24)
            string hour = "00";
            if (text.Length >= 2)
            {
                hour = text.Substring(0, 2);
                if (int.TryParse(hour, out int hourValue) && (hourValue < 0 || hourValue > 24))
                {
                    hour = "00"; // Set invalid hour to 00
                }
            }

            // Validate and format the minute (valid range 00-59)
            string minute = "00";
            if (text.Length > 2)
            {
                minute = text.Length >= 4 ? text.Substring(2, 2) : text.Substring(2);
                if (int.TryParse(minute, out int minuteValue) && (minuteValue < 0 || minuteValue > 59))
                {
                    minute = "00"; // Set invalid minute to 00
                }
            }

            // Set the formatted text back into the TextBox
            textBox.Text = $"{hour}:{minute}";
        }


    }

}