using PDC_System.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using Newtonsoft.Json;
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
    /// Interaction logic for AddDeducationWinodw.xaml
    /// </summary>
    public partial class AddDeducationWinodw : Window
    {

        private string employeeFile = "Savers/employee.json";
        public event Action<Deducation> DeducationSaved;
        public AddDeducationWinodw()
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
                var selectedDate = DeducationPicker.SelectedDate ?? DateTime.Now;

                if (decimal.TryParse(DeducationAmount.Text, out decimal deducationamount))
                {
                    var newLoan = new Deducation
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeName = emp.Name,
                        Status = "Deducation",
                        DeducationDescription = DeducationDescription.Text,
                        DeducationAmount = deducationamount,
                        DeducationDate = DateOnly.FromDateTime(selectedDate), // FIXED LINE
                    };

                    DeducationSaved?.Invoke(newLoan);
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
