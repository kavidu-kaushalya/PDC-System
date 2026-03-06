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
            LoadTimeComboBoxes();
        }

        private void LoadTimeComboBoxes()
        {
            // Populate hours (00 - 23)
            for (int h = 0; h < 24; h++)
                DueTimeHourComboBox.Items.Add(h.ToString("D2"));

            // Populate minutes (00 - 59)
            for (int m = 0; m < 60; m++)
                DueTimeMinuteComboBox.Items.Add(m.ToString("D2"));

            // Default selection
            DueTimeHourComboBox.SelectedIndex = 0;
            DueTimeMinuteComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields (Notes is optional)
            if (DueDatePicker.SelectedDate == null)
            {
                CustomMessageBox.Show("Please select a Due Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DueTimeHourComboBox.SelectedItem == null || DueTimeMinuteComboBox.SelectedItem == null)
            {
                CustomMessageBox.Show("Please select a valid Due Time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                CustomMessageBox.Show("Please enter a Customer Name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                CustomMessageBox.Show("Please enter a Description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Parse the date from the DatePicker
            DateTime dueDate = DueDatePicker.SelectedDate ?? DateTime.Now;

            // Get hour and minute from ComboBoxes
            string timeText = $"{DueTimeHourComboBox.SelectedItem}:{DueTimeMinuteComboBox.SelectedItem}";

            if (DateTime.TryParseExact(timeText, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dueTime))
            {
                // Combine date and time
                DateTime dueDateTime = dueDate.Date + dueTime.TimeOfDay;

                // Create the new order
                NewOrder = new Order
                {
                    CreateDate = DateTime.Now,
                    DueDate = dueDateTime,
                    CustomerName = CustomerNameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Notes = NotesTextBox.Text // Notes is optional, can be empty
                };

                DialogResult = true;
                Close();
            }
            else
            {
                CustomMessageBox.Show("Invalid time format. Please select valid hour and minute.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}