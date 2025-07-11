using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PDC_System
{
    public partial class AddAttendanceWindow : Window
    {
        public Attendance Attendance { get; private set; }
        
        private Employee _employee;

        public AddAttendanceWindow(List<Employee> employees)
        {
            InitializeComponent();
            EmployeeComboBox.ItemsSource = employees; // Ensure this matches the XAML name
            EmployeeComboBox.SelectionChanged += EmployeeComboBox_SelectionChanged; // Subscribe to the selection change
            LoadYears();
        }

        private void EmployeeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _employee = (Employee)EmployeeComboBox.SelectedItem; // Get selected employee
            LoadPaySheetData(); // Load the selected employee's data
        }

        private void LoadPaySheetData()
        {
            if (_employee != null) // Ensure _employee is not null
            {
                EmployeeNameTextBlock.Text = $"Name: {_employee.Name}";
                EmployeeSaleryTextBlock.Text = $"Salary: {_employee.Salary}";
                EmployeeOTTextBlock.Text = $"OT(LKR): {_employee.OT}";
                EmployeeDOTTextBlock.Text = $"DOT(LKR): {_employee.DOT}";
            }
        }

        private void LoadYears()
        {
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 1; i <= currentYear + 999; i++) // 50 years back, 10 years ahead
            {
                YearComboBox.Items.Add(i);
            }
            YearComboBox.SelectedItem = currentYear;
            MonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
        }

        private void SaveAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            // Safely parse decimal and integer values
            decimal cv = decimal.TryParse(OTTextBox.Text, out decimal otValue) ? otValue : 0;
            decimal late = decimal.TryParse(LateTextBox.Text, out decimal lateValue) ? lateValue : 0;
            decimal early = decimal.TryParse(EarlyTextBox.Text, out decimal earlyValue) ? earlyValue : 0;
            decimal dot2h = decimal.TryParse(DOTTextBox.Text, out decimal dot2hValue) ? dot2hValue : 0;
            decimal weekendDot = decimal.TryParse(Weekendot.Text, out decimal weekendotValue) ? weekendotValue : 0;


            decimal ot = cv * 60;
            decimal aot = Math.Round((ot - (late + early)) / 60, 2);
            int selectedYear = (int)YearComboBox.SelectedItem;
            string selectedMonth = ((ComboBoxItem)MonthComboBox.SelectedItem).Content.ToString();
            string formattedDate = $"{selectedYear} {selectedMonth}";
            decimal elate = Math.Round((decimal)late / 60, 2);
            decimal eerly = Math.Round((decimal)early / 60, 2);
            decimal totalDOT = Math.Round(dot2h + weekendDot);



            // Check if an employee is selected in the combo box
            var selectedEmployee = EmployeeComboBox.SelectedItem as Employee;
            if (selectedEmployee == null)
            {
                MessageBox.Show("Please select a valid employee.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Exit the method if no employee is selected
            }

            // Check if the employee exists in the employee list (EmployeeComboBox.Items)
            bool employeeExists = EmployeeComboBox.Items
                .Cast<Employee>()
                .Any(emp => emp.Name.Equals(selectedEmployee.Name, StringComparison.OrdinalIgnoreCase));

            if (!employeeExists)
            {
                MessageBox.Show("This name is not in the employee list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Exit the method if the employee is not found
            }





            Attendance = new Attendance
            {
                Employee_Name = (EmployeeComboBox.SelectedItem as Employee)?.Name,
                E_Salary = (decimal)((EmployeeComboBox.SelectedItem as Employee)?.Salary),
                E_OT = (decimal)((EmployeeComboBox.SelectedItem as Employee)?.OT),
                E_DOT = (decimal)((EmployeeComboBox.SelectedItem as Employee)?.DOT),
                absentlkr = (int)((EmployeeComboBox.SelectedItem as Employee)?.ABSENT),

                eot = cv,
                elate = elate,
                eerly = eerly,
                jobr = (EmployeeComboBox.SelectedItem as Employee)?.jobrole,

                address1 = (EmployeeComboBox.SelectedItem as Employee)?.Address1,
                address2 = (EmployeeComboBox.SelectedItem as Employee)?.Address2,
                city = (EmployeeComboBox.SelectedItem as Employee)?.City,
                contact = (EmployeeComboBox.SelectedItem as Employee)?.Contactn1,



                Month = formattedDate,

                OTMin = ot,
                edot = totalDOT,
                WorkingDays = int.TryParse(WorkingDaysTextBox.Text, out int workingDaysValue) ? workingDaysValue : 0,
                No_PAY = int.TryParse(NoPayDaysTextBox.Text, out int NoPayDaysValue) ? NoPayDaysValue : 0,
                AbsentDays = int.TryParse(AbsentDaysTextBox.Text, out int absentDaysValue) ? absentDaysValue : 0,
                Early = early,
                Late = late,
                AOT = aot,
                Loans = decimal.TryParse(LoansTextBox.Text, out decimal loansValue) ? loansValue : 0,
                CollectedMoney = decimal.TryParse(CollectedMoneyTextBox.Text, out decimal collectedMoneyValue) ? collectedMoneyValue : 0,
                ETF = decimal.TryParse(ETFTextBox.Text, out decimal etfValue) ? etfValue : 0,











            };

            // Close the window and return true
            DialogResult = true;
        }

        private void PreviewfloteTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string currentText = textBox?.Text ?? string.Empty;

            // Allow only numeric input (0-9) or a single decimal point
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;  // Prevent the input
            }
            else
            {
                // Allow only one decimal point
                if (e.Text == "." && currentText.Contains("."))
                {
                    e.Handled = true;  // Prevent input if there's already a decimal point
                }

                // Limit the length to 10 characters including the decimal point
                if (currentText.Length >= 10)
                {
                    e.Handled = true;  // Prevent input if length exceeds 10
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;

                // If the text is not empty and is a valid number
                if (!string.IsNullOrWhiteSpace(text) && decimal.TryParse(text, out decimal result))
                {
                    // Format the number to 2 decimal places (e.g., 500 to 500.00)
                    textBox.Text = result.ToString("F2");
                }
            }
        }


        private void OtTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;

                // If the text is not empty and is a valid number
                if (!string.IsNullOrWhiteSpace(text) && decimal.TryParse(text, out decimal result))
                {
                    // Format the number to 2 decimal places (e.g., 500 to 500.00)
                    textBox.Text = result.ToString("F1");
                }
            }
        }

        private void IntegerOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void Weekendot_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}