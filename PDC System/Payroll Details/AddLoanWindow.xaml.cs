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
                        Remeining = loanAmt,
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



    } 
}
