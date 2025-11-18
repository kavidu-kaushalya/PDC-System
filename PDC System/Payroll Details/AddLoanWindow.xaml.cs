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
       
        
        private string employeeFile = "Saver/employee.json";
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
                if (decimal.TryParse(LoanAmountBox.Text, out decimal loanAmt) &&
                    decimal.TryParse(MonthlyPayBox.Text, out decimal monthly))
                {
                    var newLoan = new Loan
                    {
                        EmployeeId = emp.EmployeeId,
                        Name = emp.Name,
                        LoanAmount = loanAmt,
                        MonthlyPay = monthly
                    };

                    LoanSaved?.Invoke(newLoan);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter valid numbers for loan and monthly pay.");
                }
            }
            else
            {
                MessageBox.Show("Please select an employee.");
            }
        }
    } 
}
