using PDC_System.Models;
using Newtonsoft.Json;
using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
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

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for AddETFWindow.xaml
    /// </summary>
    public partial class AddETFWindow : Window
    {

        private string employeeFile = "Saver/employee.json";
        public event Action<ETF> ETFsaved;
        internal TextBox ETFPercentage;
        public AddETFWindow()
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
                if (decimal.TryParse(BasicSalery.Text, out decimal BasicSaleryAmount))
                     // Added parsing for ETFPrecentage
                {
                    decimal ETFEmployeePrecentage = Properties.Settings.Default.ETFEmployee;

                    decimal EmployeeAmount = (BasicSaleryAmount * ETFEmployeePrecentage) / 100;

                    decimal ETFEmployerrecentage = Properties.Settings.Default.ETFEmployer;

                    decimal EmployerAmount = (BasicSaleryAmount * ETFEmployerrecentage) / 100;

                    var newETF = new ETF
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeName = emp.Name,
                        BasicSalary = BasicSaleryAmount,
                        EmployeeAmount = EmployeeAmount,
                        EmployerAmount = EmployerAmount,
                        Total = EmployeeAmount + EmployerAmount
                    };

                    ETFsaved?.Invoke(newETF);
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
