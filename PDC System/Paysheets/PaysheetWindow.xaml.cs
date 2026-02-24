using Newtonsoft.Json;
using PDC_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDC_System.Paysheets
{
    /// <summary>
    /// Interaction logic for PaysheetWindow.xaml
    /// </summary>
    public partial class PaysheetWindow : UserControl
    {
        public PaysheetWindow()
        {
            InitializeComponent();
            LoadPaysheets();
        }

        private void CreatePaysheet_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddPaysheetWindow();
            window.ShowDialog();

            LoadPaysheets();          // ✅ reload JSON → list update
            PaysheetGrid.Items.Refresh();
        }

        private string paysheetFile = "Savers/Paysheets.json";

        private List<Paysheet> paysheets = new List<Paysheet>();

        private void LoadPaysheets()
        {
            if (File.Exists(paysheetFile))
            {
                string json = File.ReadAllText(paysheetFile);
                paysheets = JsonConvert.DeserializeObject<List<Paysheet>>(json) ?? new List<Paysheet>();
            }

            PaysheetGrid.ItemsSource = paysheets;

            LoadYears();
        }


        private void LoadYears()
        {
            YearCombo.Items.Clear();
            YearCombo.Items.Add("All Years");

            var years = paysheets
                .Select(p =>
                {
                    DateTime d;
                    if (DateTime.TryParse(p.Month, out d))
                        return d.Year.ToString();
                    return null;
                })
                .Where(y => y != null)
                .Distinct()
                .OrderByDescending(y => y);

            foreach (var year in years)
                YearCombo.Items.Add(year);

            YearCombo.SelectedIndex = 0;
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var filtered = paysheets.AsEnumerable();

            // SEARCH FILTER
            string search = SearchBox.Text?.ToLower();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(p =>
                    (p.EmployeeName != null && p.EmployeeName.ToLower().Contains(search)) ||
                    (p.EmployeeId != null && p.EmployeeId.ToLower().Contains(search)) ||
                    (p.PaysheetId != null && p.PaysheetId.ToLower().Contains(search))
                );
            }

            // MONTH FILTER
            if (MonthCombo.SelectedIndex > 0)
            {
                string selectedMonth = ((ComboBoxItem)MonthCombo.SelectedItem).Content.ToString();

                filtered = filtered.Where(p =>
                {
                    DateTime d;
                    if (DateTime.TryParse(p.Month, out d))
                        return d.ToString("MMMM") == selectedMonth;
                    return false;
                });
            }

            // YEAR FILTER
            if (YearCombo.SelectedIndex > 0)
            {
                string selectedYear = YearCombo.SelectedItem.ToString();

                filtered = filtered.Where(p =>
                {
                    DateTime d;
                    if (DateTime.TryParse(p.Month, out d))
                        return d.Year.ToString() == selectedYear;
                    return false;
                });
            }

            PaysheetGrid.ItemsSource = filtered.ToList();
        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";

            MonthCombo.SelectedIndex = 0;
            YearCombo.SelectedIndex = 0;

            PaysheetGrid.ItemsSource = paysheets;
        }



        private void DeletePaysheet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var paysheet = btn.Tag as Paysheet;

            if (paysheet == null)
                return;

            // ================================
            // ✅ CONFIRMATION MESSAGE BOX
            // ================================
            var result = CustomMessageBox.Show(
                $"Are you sure you want to delete Paysheet '{paysheet.PaysheetId}'?\n\nThis will also reverse Loan and EPF history.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // ================================
            // 🔥 1. REVERSE LOAN IF EXISTED
            // ================================
            var history = LoanHistoryService.GetByPaysheetId(paysheet.PaysheetId);

            if (history != null)
            {
                var loanJson = File.ReadAllText("Savers/loan.json");
                var loans = JsonConvert.DeserializeObject<List<Loan>>(loanJson) ?? new List<Loan>();

                var loan = loans.FirstOrDefault(l => l.EmployeeId == paysheet.EmployeeId);

                if (loan != null)
                {
                    loan.Remeining += history.PaidAmount;

                    if (loan.Remeining > 0)
                        loan.Status = "Active";

                    File.WriteAllText("Savers/loan.json",
                        JsonConvert.SerializeObject(loans, Formatting.Indented));
                }

                LoanHistoryService.DeleteByPaysheetId(paysheet.PaysheetId);
            }

            // ================================
            // 🔥 2. DELETE EPF HISTORY ALSO
            // ================================
            DeleteEPFHistory(paysheet.PaysheetId);

            // ================================
            // 🔥 3. DELETE PAYSHEET
            // ================================
            paysheets.Remove(paysheet);
            File.WriteAllText(paysheetFile, JsonConvert.SerializeObject(paysheets, Formatting.Indented));

            LoadPaysheets();
            PaysheetGrid.Items.Refresh();

            CustomMessageBox.Show("Paysheet deleted successfully. Loan & EPF corrected 🔄");
        }



        private void EditPaysheet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var paysheet = btn.Tag as Paysheet;

            if (paysheet != null)
            {
                var window = new AddPaysheetWindow();

                // Load all paysheet data
                window.LoadPaysheetData(paysheet);

                window.ShowDialog();

                // Reload grid after editing
                LoadPaysheets();
                PaysheetGrid.Items.Refresh();
            }
        }


        private void OpenPDF_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string path = btn?.Tag?.ToString();

            if (string.IsNullOrWhiteSpace(path))
            {
                CustomMessageBox.Show("No PDF found for this paysheet.");
                return;
            }

            if (!File.Exists(path))
            {
                CustomMessageBox.Show("PDF file does not exist in saved location!");
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            });
        }





        private string epfHistoryFile = "Savers/EPFHistory.json";

        private void DeleteEPFHistory(string paysheetId)
        {
            if (!File.Exists(epfHistoryFile))
                return;

            string json = File.ReadAllText(epfHistoryFile);
            var list = JsonConvert.DeserializeObject<List<EPFHistory>>(json) ?? new List<EPFHistory>();

            // Delete matching paysheet EPF history record
            list = list.Where(x => x.PaysheetId != paysheetId).ToList();

            File.WriteAllText(epfHistoryFile, JsonConvert.SerializeObject(list, Formatting.Indented));
        }






    }




}
