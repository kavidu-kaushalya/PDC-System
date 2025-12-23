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
    public partial class AddEPFWindow : Window
    {

        private string epfFile = "Savers/epf.json";

        private string employeeFile = "Savers/employee.json";
        public event Action<EPF> ETFsaved;
        internal TextBox ETFPercentage;
        public AddEPFWindow()
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
                // -------------------------------
                // LOAD EXISTING EPF RECORDS
                // -------------------------------
                List<EPF> existing = new List<EPF>();

                if (File.Exists(epfFile))
                {
                    string epfJson = File.ReadAllText(epfFile);
                    existing = JsonConvert.DeserializeObject<List<EPF>>(epfJson) ?? new List<EPF>();
                }

                // ❌ Check duplicate EPF for same employee
                if (existing.Any(x => x.EmployeeId == emp.EmployeeId))
                {
                    CustomMessageBox.Show("This employee already has an EPF record!");
                    return;
                }

                // -------------------------------
                // SALARY CALCULATIONS
                // -------------------------------

                if (decimal.TryParse(BasicSalery.Text, out decimal BasicSaleryAmount))
                {
                    decimal EPFEmployeePrecentage = Properties.Settings.Default.EPFEmployee;
                    decimal EmployeeAmount = (BasicSaleryAmount * EPFEmployeePrecentage) / 100;

                    decimal EPFEmployerrecentage = Properties.Settings.Default.EPFEmployer;
                    decimal EmployerAmount = (BasicSaleryAmount * EPFEmployerrecentage) / 100;

                    var newEPF = new EPF
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeName = emp.Name,
                        BasicSalary = BasicSaleryAmount,
                        EmployeeAmount = EmployeeAmount,
                        EmployerAmount = EmployerAmount,
                        Total = EmployeeAmount + EmployerAmount
                    };

                    // -------------------------------
                    // SAVE NEW EPF RECORD
                    // -------------------------------
                    existing.Add(newEPF);
                    File.WriteAllText(epfFile, JsonConvert.SerializeObject(existing, Formatting.Indented));

                    ETFsaved?.Invoke(newEPF);
                    this.Close();
                }
                else
                {
                    CustomMessageBox.Show("Please enter valid numbers for basic salary.");
                }
            }
            else
            {
                CustomMessageBox.Show("Please select an employee.");
            }
        }



    }
}
