using Newtonsoft.Json;
using PDC_System.Attendance_And_Paysheets;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static PDC_System.QuotationWindow;

namespace PDC_System
{
    
    public partial class AttendanceWindow : System.Windows.Controls.UserControl
    {
        private List<Employee> employees = new List<Employee>();
        private List<Attendance> attendance = new List<Attendance>();
        
        

        public AttendanceWindow()
        {
            InitializeComponent();
            PopulateMonthYearSelectors();




            LoadData();
        }

        private void LoadData()
        {
            if (File.Exists("attendance.json"))
            {
                var attendanceData = JsonConvert.DeserializeObject<List<Attendance>>(File.ReadAllText("attendance.json"));
                if (attendanceData != null)
                {
                    attendance = attendanceData;
                    AttendanceDataGrid.ItemsSource = attendance;
                }
            }

            if (File.Exists("employee.json"))
            {
                var employeeData = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText("employee.json"));
                if (employeeData != null)
                {
                    employees = employeeData;
                }
            }
        }



        private void AddAttendance_Click(object sender, RoutedEventArgs e)
        {
            var addAttendanceWindow = new AddAttendanceWindow(employees);
            if (addAttendanceWindow.ShowDialog() == true)
            {
                attendance.Insert(0, addAttendanceWindow.Attendance); // Insert at the beginning instead of Add()
                AttendanceDataGrid.ItemsSource = null; // Reset DataGrid source
                AttendanceDataGrid.ItemsSource = attendance; // Reassign source to refresh DataGrid
                File.WriteAllText("attendance.json", JsonConvert.SerializeObject(attendance));
            }
        }


        private void DeleteAttendance_Click(object sender, RoutedEventArgs e)
        {
            var selectedAttendance = AttendanceDataGrid.SelectedItem as Attendance;
            if (selectedAttendance != null)
            {
                // Show a confirmation dialog
                var confirmationDialog = new ConfirmationDialogAttendance(); // Assuming you have the ConfirmationDialog created
                confirmationDialog.Owner = Application.Current.MainWindow; // Set the main window as the owner
                confirmationDialog.ShowDialog();

                // If user confirms deletion
                if (confirmationDialog.IsConfirmed)
                {
                    attendance.Remove(selectedAttendance);
                    AttendanceDataGrid.Items.Refresh();
                    File.WriteAllText("attendance.json", JsonConvert.SerializeObject(attendance));
                }
            }
        }


        private void CreatePaySheetButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAttendance = AttendanceDataGrid.SelectedItem as Attendance;
            if (selectedAttendance != null)
            {
                var quotationCreateWindow = new PaySheetWindow(selectedAttendance);
                if (quotationCreateWindow.ShowDialog() == true)
                {
                    // Implement any required logic after closing the PaySheetWindow
                }
            }
            else
            {
                MessageBox.Show("Please select an attendance record.");
            }
        }

        private void OpenPaySheetButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected row
            var selectedRow = AttendanceDataGrid.SelectedItem as Attendance;
            if (selectedRow != null)
            {
                // Open the details window and pass the data
                PaySheetWindow detailsWindow = new PaySheetWindow(selectedRow);
                detailsWindow.Show();
            }
            else
            {
                MessageBox.Show("Please select an employee.");
            }
        }



        private void ComboBoxGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Toggle the drop-down open/close when the ComboBox body is clicked
            if (MonthComboBox.IsDropDownOpen)
            {
                MonthComboBox.IsDropDownOpen = false; // Close dropdown if it's already open
            }
            else
            {
                MonthComboBox.IsDropDownOpen = true; // Open dropdown
            }
        }

        private void ComboBoxGrid_MouseLeftButtonDown1(object sender, MouseButtonEventArgs e)
        {
            // Toggle the drop-down open/close when the ComboBox body is clicked
            if (YearComboBox.IsDropDownOpen)
            {
                YearComboBox.IsDropDownOpen = false; // Close dropdown if it's already open
            }
            else
            {
                YearComboBox.IsDropDownOpen = true; // Open dropdown
            }
        }


        private void PopulateMonthYearSelectors()
        {
            // Populate months
            MonthComboBox.ItemsSource = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToList();
            MonthComboBox.SelectedIndex = DateTime.Now.Month - 1; // Default to current month

            // Populate years (from 2000 to current year)
            int currentYear = DateTime.Now.Year;
            YearComboBox.ItemsSource = Enumerable.Range(2000, currentYear - 1999).Reverse().ToList();
            YearComboBox.SelectedItem = currentYear; // Default to current year
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (MonthComboBox.SelectedIndex == -1 || YearComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show("Please select both a month and a year.", "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get selected month and year
            int selectedMonth = MonthComboBox.SelectedIndex + 1; // Convert to 1-based index
            int selectedYear = (int)YearComboBox.SelectedItem;

            DateTime startDate = new DateTime(selectedYear, selectedMonth, 1);
            DateTime endDate = new DateTime(selectedYear, selectedMonth, DateTime.DaysInMonth(selectedYear, selectedMonth));

            // Filter data
            var filteredData = attendance.Where(r => r.Month != null && IsMonthInRange(r.Month, startDate, endDate)).ToList();

            AttendanceDataGrid.ItemsSource = filteredData;

            if (filteredData.Count == 0)
            {
                System.Windows.MessageBox.Show("No records found for the selected month and year.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Clear selected items in the ComboBoxes
            MonthComboBox.SelectedItem = DateTime.Now.ToString("MMMM"); // Example: "March"
            YearComboBox.SelectedIndex = 0;

            // Reset DataGrid to show all attendance data
            AttendanceDataGrid.ItemsSource = attendance;  // Assuming 'attendance' holds all your data
        }

        private bool IsMonthInRange(string monthString, DateTime startDate, DateTime endDate)
        {
            if (DateTime.TryParseExact(monthString, "yyyy MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime monthDate))
            {
                monthDate = new DateTime(monthDate.Year, monthDate.Month, 1); // Set to 1st of the month
                return monthDate >= startDate && monthDate <= endDate;
            }
            return false;
        }


       


        private void NameAutoCompleteBox_TextChanged(object sender, RoutedEventArgs e)
        {
            string typedText = NameAutoCompleteBox.Text.Trim();

            if (!string.IsNullOrEmpty(typedText))
            {
                // Filter attendance records where name starts with the typed text
                var filteredData = attendance
                    .Where(a => a.Employee_Name != null && a.Employee_Name.StartsWith(typedText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Update the DataGrid with filtered results
                AttendanceDataGrid.ItemsSource = filteredData;
            }
            else
            {
                // If text is cleared, show all attendance records
                AttendanceDataGrid.ItemsSource = attendance;
            }
        }


     


       




    }
}
