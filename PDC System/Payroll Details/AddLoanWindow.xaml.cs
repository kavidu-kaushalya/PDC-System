using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PDC_System.Models;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for AddLoanWindow.xaml
    /// </summary>
    public partial class AddLoanWindow : Window
    {

        private string loanFile = "Savers/loan.json";

        private string employeeFile = "Savers/employee.json";
        public event Action<Loan> LoanSaved;

        public AddLoanWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            if (File.Exists(employeeFile))
            {
                string json = File.ReadAllText(employeeFile);
                var employees = JsonConvert.DeserializeObject<List<Employee>>(json);
                EmployeeCombo.ItemsSource = employees;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeCombo.SelectedItem is Employee emp)
            {
                // Load all loans
                var existingLoans = File.Exists(loanFile)
                    ? JsonConvert.DeserializeObject<List<Loan>>(File.ReadAllText(loanFile))
                    : new List<Loan>();

                // ❗ Block if employee has ANY previous loan (Active or Finished)
                bool hasAnyLoan = existingLoans.Any(l =>
                    l.EmployeeId == emp.EmployeeId
                );

                if (hasAnyLoan)
                {


                    CustomMessageBox.Show("This employee already has a loan. Loan End and Can Create Loan");
                    return;
                }

                var selectedDate = LoanDatePicker.SelectedDate ?? DateTime.Now;

                if (decimal.TryParse(LoanAmountBox.Text, out decimal loanAmt) &&
                    decimal.TryParse(MonthlyPayBox.Text, out decimal monthly))
                {
                    var newLoan = new Loan
                    {
                        EmployeeId = emp.EmployeeId,
                        Name = emp.Name,
                        LoanAmount = loanAmt,
                        MonthlyPay = monthly,
                        
                        LoanDate = selectedDate,
                        Status = "Active"
                    };

                    existingLoans.Add(newLoan);
                    File.WriteAllText(loanFile, JsonConvert.SerializeObject(existingLoans, Formatting.Indented));

                    LoanSaved?.Invoke(newLoan);
                    this.Close();
                }
                else
                {
                    CustomMessageBox.Show("Please enter valid numbers for loan and monthly pay.");
                }
            }
            else
            {
                CustomMessageBox.Show("Please select an employee.");
            }
        }




        #region Window Control

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private bool _isMaximized = false;
        private double _previousLeft;
        private double _previousTop;
        private double _previousWidth;
        private double _previousHeight;

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (_isMaximized)
            {
                // Restore to previous size and position
                this.Left = _previousLeft;
                this.Top = _previousTop;
                this.Width = _previousWidth;
                this.Height = _previousHeight;
                _isMaximized = false;
            }
            else
            {
                // get before maximizing
                _previousLeft = this.Left;
                _previousTop = this.Top;
                _previousWidth = this.Width;
                _previousHeight = this.Height;

                // Get the working area (screen minus taskbar)
                var workingArea = SystemParameters.WorkArea;

                // Set window position and size to working area
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                _isMaximized = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }

        #endregion


    }
}
