using PDC_System.Models;
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
using Newtonsoft.Json;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDC_System
{
    /// <summary>
    /// Interaction logic for AddErningWindow.xaml
    /// </summary>
    public partial class AddErningWindow : Window
    {

        private string employeeFile = "Saver/employee.json";
        public event Action<Earning> Earingsaved;

        public AddErningWindow()
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
                if (decimal.TryParse(EarningAmount.Text, out decimal Earningamount))
                {
                    var newEaring = new Earning
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeName = emp.Name,
                        EarningAmount = Earningamount,
                        Status = "Earning",
                        EarningDescription = EarningDescription.Text,
                        EarningDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    Earingsaved?.Invoke(newEaring);
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
