using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PDC_System
{
    public partial class EmployeeWindow : System.Windows.Controls.UserControl
    {
        private List<Employee> employees = new List<Employee>();
        private List<Employee> filteredEmployees = new List<Employee>();

        public EmployeeWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists("employee.json"))
            {
                employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText("employee.json"));
                filteredEmployees = new List<Employee>(employees); // Initialize filtered list
                EmployeeDataGrid.ItemsSource = filteredEmployees;
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var addEmployeeWindow = new EmployeeAddData();
            if (addEmployeeWindow.ShowDialog() == true)
            {
                employees.Add(addEmployeeWindow.Employee);
                ApplyFilter(); // Refresh DataGrid with filter applied
                File.WriteAllText("employee.json", JsonConvert.SerializeObject(employees));
            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeeDataGrid.SelectedItem as Employee;
            if (selectedEmployee != null)
            {
                // Show a confirmation dialog
                var confirmationDialog = new ConfirmationDialogEmployee(); // Assuming you've created this dialog class
                confirmationDialog.Owner = Application.Current.MainWindow; // Set the main window as the owner
                confirmationDialog.ShowDialog();

                // If user confirms deletion
                if (confirmationDialog.IsConfirmed)
                {
                    employees.Remove(selectedEmployee);
                    ApplyFilter(); // Refresh DataGrid with filter applied
                    File.WriteAllText("employee.json", JsonConvert.SerializeObject(employees));
                }
            }
        }


        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedRow = EmployeeDataGrid.SelectedItem as Employee;
            if (selectedRow != null)
            {
                MainContent.Content = new EDetailsWindow(selectedRow); // Pass selected employee
            }
            else
            {
                System.Windows.MessageBox.Show("Please select an employee.");
            }
        }


        private void OpenDuplicateWindow_Click(object sender, RoutedEventArgs e)
        {
            var passwordWindow = new PasswordWindow();
            if (passwordWindow.ShowDialog() == true)
            {
                EmployeeDetailsEdit newWindow = new EmployeeDetailsEdit();
                newWindow.Show();
            }
            else
            {
                System.Windows.MessageBox.Show("Password incorrect or cancelled.");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = SearchBox.Text.Trim().ToLower();
            filteredEmployees = employees
                .Where(emp => emp.Name.ToLower().Contains(query) || emp.NID.ToLower().Contains(query))
                .ToList();

            EmployeeDataGrid.ItemsSource = null;  // Reset DataGrid source
            EmployeeDataGrid.ItemsSource = filteredEmployees;
        }
    }
}
