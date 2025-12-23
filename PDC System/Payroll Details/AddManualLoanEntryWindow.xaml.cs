using Newtonsoft.Json;
using PDC_System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

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
                _currentRemaining = loan.Remeining;
                CurrentRemainingBox.Text = $"Remaining: {_currentRemaining:N2}";

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
                MonthlyInstallment = monthlyPay,   // 🔥 AUTO INSTALLMENT
                OriginalLoanAmount = _currentRemaining,
                RemainingAmount = newRemaining,
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

            if (loan != null)
            {
                // New remaining set karanawa
                loan.Remeining = newRemaining;

                if (newRemaining <= 0)
                {
                    loan.Remeining = 0;
                    loan.Status = "Finished";
                }

                // write json back
                File.WriteAllText(path, JsonConvert.SerializeObject(loans, Formatting.Indented));
            }
        }




    }
}
