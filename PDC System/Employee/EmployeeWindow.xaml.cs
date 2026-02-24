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

        // Set the path to the Savers folder in the current directory
        private string saversFolder = Path.Combine(Directory.GetCurrentDirectory(), "Savers");
        private string employeeFile;

        public EmployeeWindow()
        {
            InitializeComponent();

            // Ensure Savers folder exists
            if (!Directory.Exists(saversFolder))
                Directory.CreateDirectory(saversFolder);

            // Full path to JSON file
            employeeFile = Path.Combine(saversFolder, "employee.json");

            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists(employeeFile))
            {
                employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText(employeeFile));
                filteredEmployees = new List<Employee>(employees); // Initialize filtered list
                EmployeeDataGrid.ItemsSource = filteredEmployees;
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var addEmployeeWindow = new EmployeeAddData();  // NO PARAM

            if (addEmployeeWindow.ShowDialog() == true)
            {
                employees.Add(addEmployeeWindow.Employee);
                ApplyFilter();
                File.WriteAllText(employeeFile, JsonConvert.SerializeObject(employees, Formatting.Indented));
            }
        }


        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeeDataGrid.SelectedItem as Employee;

            if (selectedEmployee != null)
            {
                MessageBoxResult result = CustomMessageBox.Show(
                    $"Are you sure you want to delete employee '{selectedEmployee.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    employees.Remove(selectedEmployee);
                    ApplyFilter();
                    File.WriteAllText(employeeFile, JsonConvert.SerializeObject(employees));
                }
            }
            else
            {
                CustomMessageBox.Show(
                    "Please select an employee to delete.",
                    "No Selection",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
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

            EmployeeDataGrid.ItemsSource = null;
            EmployeeDataGrid.ItemsSource = filteredEmployees;
        }


        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            var emp = (sender as Button)?.Tag as Employee;
            if (emp == null) return;

            // Pass employee to edit window
            var editWindow = new EmployeeAddData(emp);

            if (editWindow.ShowDialog() == true)
            {
                // Update employee in the list
                var index = employees.FindIndex(x => x.EmployeeId == emp.EmployeeId);

                if (index != -1)
                {
                    employees[index] = editWindow.Employee;
                }

                ApplyFilter();

                // Save JSON
                File.WriteAllText(employeeFile, JsonConvert.SerializeObject(employees, Formatting.Indented));
            }
        }

    }
}
