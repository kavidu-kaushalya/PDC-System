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
            var addEmployeeWindow = new EmployeeAddData();
            if (addEmployeeWindow.ShowDialog() == true)
            {
                employees.Add(addEmployeeWindow.Employee);
                ApplyFilter(); // Refresh DataGrid with filter applied
                File.WriteAllText(employeeFile,JsonConvert.SerializeObject(employees, Formatting.Indented));

            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = EmployeeDataGrid.SelectedItem as Employee;
            if (selectedEmployee != null)
            {
                var confirmationDialog = new ConfirmationDialogEmployee();
                confirmationDialog.Owner = Application.Current.MainWindow;
                confirmationDialog.ShowDialog();

                if (confirmationDialog.IsConfirmed)
                {
                    employees.Remove(selectedEmployee);
                    ApplyFilter();
                    File.WriteAllText(employeeFile, JsonConvert.SerializeObject(employees));
                }
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedRow = EmployeeDataGrid.SelectedItem as Employee;
            if (selectedRow != null)
            {
                MainContent.Content = new EDetailsWindow(selectedRow);
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

            EmployeeDataGrid.ItemsSource = null;
            EmployeeDataGrid.ItemsSource = filteredEmployees;
        }
    }
}
