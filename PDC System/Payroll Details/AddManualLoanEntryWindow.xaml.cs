using Newtonsoft.Json;
using PDC_System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PDC_System.Payroll_Details
{
    public partial class AddManualLoanEntryWindow : Window
    {
        private string _employeeId;
        private string _employeeName;
        private decimal _currentRemaining;

        public AddManualLoanEntryWindow(string empId, string empName)
        {
            InitializeComponent();

            _employeeId = empId;
            _employeeName = empName;

            // AUTO FILL FIELDS
            EmployeeIdBox.Text = empId;
            EmployeeNameBox.Text = empName;

            LoadRemainingAmount();
        }

        private void LoadRemainingAmount()
        {
            string json = File.ReadAllText("Savers/loan.json");
            var loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();

            var loan = loans.FirstOrDefault(l =>
                l.EmployeeId == _employeeId &&
                l.Status == "Active"
            );

            if (loan != null)
            {
               

                // 🔥 AUTO SET INSTALLMENT FROM LOAN.JSON
                MonthlyInstallmentBox.Text = loan.MonthlyPay.ToString("N2");

            }
            else
            {
                _currentRemaining = 0;
                CurrentRemainingBox.Text = "No Active Loan";
                MonthlyInstallmentBox.Text = "0.00";
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


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            decimal monthlyPay = 0;
            decimal.TryParse(MonthlyInstallmentBox.Text, out monthlyPay);

            decimal paid = 0;
            decimal.TryParse(PaidAmountBox.Text, out paid);

            decimal newRemaining = _currentRemaining - paid;
            if (newRemaining < 0) newRemaining = 0;

            LoanHistoryService.AddEntry(new LoanHistoryService.LoanHistoryEntry
            {
                PaysheetId = Guid.NewGuid().ToString(),
                EmployeeId = _employeeId,
                EmployeeName = _employeeName,
                PaidAmount = paid,
                MonthlyInstallment = monthlyPay,
               
                Month = (DatePicker.SelectedDate?.ToString("MMMM yyyy")) ?? "",
                Date = DatePicker.SelectedDate ?? DateTime.Now
            });

            UpdateLoanJsonRemaining(newRemaining);

            CustomMessageBox.Show("Manual loan history added!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }

        private void UpdateLoanJsonRemaining(decimal newRemaining)
        {
            string path = "Savers/loan.json";

            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();

            // Active loan eka hoya ganna
            var loan = loans.FirstOrDefault(l =>
                l.EmployeeId == _employeeId &&
                l.Status == "Active"
            );

          
        }




    }
}
