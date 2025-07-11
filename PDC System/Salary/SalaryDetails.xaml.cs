using Newtonsoft.Json;
using PDC_System.Attendance_And_Paysheets;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace PDC_System
{
    /// <summary>
    /// Interaction logic for SalaryDetails.xaml
    /// </summary>
    /// 
    
    public partial class SalaryDetails : System.Windows.Controls.UserControl

    {
        private List<PaysheetDetails> payers = new List<PaysheetDetails>();
        public SalaryDetails()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {



            if (File.Exists("Paysheetdata.json"))
            {
                payers = JsonConvert.DeserializeObject<List<PaysheetDetails>>(File.ReadAllText("Paysheetdata.json"));
                Paysheet_D.ItemsSource = payers;
            }

            CalculateTotalSalary();

        }

        private void NameAutoCompleteBox_TextChanged2(object sender, RoutedEventArgs e)
        {
            string typedText = NameAutoCompleteBox1.Text.Trim();

            if (!string.IsNullOrEmpty(typedText))
            {
                // Filter attendance records where name starts with the typed text
                var filteredData = payers
                    .Where(a => a.Name != null && a.Name.StartsWith(typedText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Update the DataGrid with filtered results
                Paysheet_D.ItemsSource = filteredData;
            }
            else
            {
                // If text is cleared, show all attendance records
                Paysheet_D.ItemsSource = payers;
            }
        }


        private void FilterData(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = dpStartDate.SelectedDate;
            DateTime? endDate = dpEndDate.SelectedDate;

            // Assuming DateOfJoining is the correct property
            var filtered = payers
                .Where(emp =>
                    (!startDate.HasValue || emp.timestamp.Date >= startDate.Value.Date) &&
                    (!endDate.HasValue || emp.timestamp.Date <= endDate.Value.Date))
                .ToList(); // Execute the query

            // Update the data grid with the filtered list
            Paysheet_D.ItemsSource = filtered;

            // Automatically calculate total salary
            CalculateTotalSalary();
        }

        private void ResetFilters(object sender, RoutedEventArgs e)
        {
            // Clear the DatePickers
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;

            // Reload the original data (assuming 'payers' contains the full list)
            Paysheet_D.ItemsSource = payers;

            // Optionally, re-calculate total salary if needed
            CalculateTotalSalary();
        }



        private void CalculateTotalSalary()
        {
            if (Paysheet_D.ItemsSource is IEnumerable<PaysheetDetails> currentData)
            {
                decimal totalSalary = currentData.Sum(emp => emp.pamount);
                txtTotalSalary.Text = totalSalary.ToString("N2");
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = Paysheet_D.SelectedItem as PaysheetDetails;
            if (selectedItem != null)
            {
                OpenFile(selectedItem.pfilepath);
            }
        }
        

        private void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            else
            {
                System.Windows.MessageBox.Show("File not found!");
            }
        }
        private void DeletePaysheet_D_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = Paysheet_D.SelectedItem as PaysheetDetails;
            if (selectedItem != null)
            {
                // Show a confirmation dialog
                var confirmationDialog = new ConfirmationDialogSalary(); // Assuming you've created this dialog class
                confirmationDialog.Owner = Application.Current.MainWindow; // Set the main window as the owner
                confirmationDialog.ShowDialog();

                // If user confirms deletion
                if (confirmationDialog.IsConfirmed)
                {
                    payers.Remove(selectedItem);
                    Paysheet_D.Items.Refresh();
                    File.WriteAllText("Paysheetdata.json", JsonConvert.SerializeObject(payers));
                    CalculateTotalSalary();
                }
            }
        }



    }

}
