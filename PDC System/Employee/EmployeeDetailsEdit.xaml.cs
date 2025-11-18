using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class EmployeeDetailsEdit : Window
    {
        private List<Employee> employees = new List<Employee>(); // Consistent list variable

        public EmployeeDetailsEdit()
        {
            InitializeComponent();
            
            LoadData();
        }

        // Load employee data from a JSON file
        private void LoadData()
        {
            if (File.Exists("employee.json"))
            {
                employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText("employee.json"));
                EmployeeDataGrid.ItemsSource = employees;
            }
        }

        // Event handler for saving the data when editing ends
        private void EmployeeDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Check if the edit was made on the Salary, NID, or O.T(lkr) column
            if (e.Column.Header.ToString() == "Salary" || e.Column.Header.ToString() == "D.OT (lkr)" || e.Column.Header.ToString() == "O.T(lkr)" || e.Column.Header.ToString() == "NID")
            {
                // Ensure that the data is properly formatted (e.g., to 2 decimal places for Salary and O.T(lkr))
                if (e.Column.Header.ToString() == "Salary" || e.Column.Header.ToString() == "O.T (lkr)" || e.Column.Header.ToString() == "D.OT (lkr)")
                {
                    TextBox editedTextBox = e.EditingElement as TextBox;
                    if (decimal.TryParse(editedTextBox.Text, out decimal result))
                    {
                        editedTextBox.Text = result.ToString("0.00");
                    }
                }

                // Call the SaveData method to save the changes after cell edit
                SaveData();
            }

            // Check if editing the specific column (e.g., D.OT (LKR)) 
            if (e.Column.Header.ToString() == "NID")
            {
                var textBox = e.EditingElement as TextBox;
                if (textBox != null)
                {
                    // Attach the PreviewTextInput event handler for this column
                    textBox.PreviewTextInput += NatId_PreviewTextInput;
                }
            }
        }


        private bool vEntered = false;
        private void NatId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow "V" only once
            if (e.Text == "V" && !vEntered)
            {
                vEntered = true;
            }
            // Block non-numeric characters and ensure the length doesn't exceed 12 characters
            else if ((!char.IsDigit(e.Text, 0) && e.Text != "V") || (e.Source is TextBox textBox && textBox.Text.Length >= 12))
            {
                e.Handled = true;
            }
        }

        // Event handler for Save button click
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            

            SaveData(); // Manually save data
            MessageBox.Show("Data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Method to save updated employee data to the JSON file
        private void SaveData()
        {
            File.WriteAllText("employee.json", JsonConvert.SerializeObject(employees, Formatting.Indented));
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchNID = NIDSearchBox.Text.Trim();

            if (!string.IsNullOrEmpty(searchNID))
            {
                var filteredEmployees = employees.Where(emp => emp.NID != null && emp.NID.Contains(searchNID)).ToList();
                EmployeeDataGrid.ItemsSource = filteredEmployees;

                if (filteredEmployees.Count == 0)
                {
                    MessageBox.Show("No employee found with the given NID.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeDataGrid.ItemsSource = employees; // Reset to full list
            EmployeeDataGrid.Items.Refresh();
            NIDSearchBox.Clear();
        }


        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers (0-9)
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void FloatOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers (0-9) and one decimal point
            string currentText = (sender as TextBox).Text;
            bool isDecimalPresent = currentText.Contains(".");

            // If there's already a decimal point, ensure only up to two decimal places are allowed
            if (isDecimalPresent)
            {
                e.Handled = !Regex.IsMatch(e.Text, @"^\d{1,2}(\.\d{0,2})?$");  // Up to 2 decimal places after one decimal point
            }
            else
            {
                e.Handled = !Regex.IsMatch(e.Text, @"^\d*\.?\d*$");  // Only digits and one decimal point allowed
            }
        }


        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numeric input (0-9) and limit to 10 characters
            if (!char.IsDigit(e.Text, 0) || ((sender as TextBox).Text.Length >= 10))
            {
                e.Handled = true;  // Prevent the input
            }
        }






    }
}
