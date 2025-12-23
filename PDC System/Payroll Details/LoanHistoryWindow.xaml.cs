using LiveCharts;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using PDC_System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDC_System.Payroll_Details
{
    /// <summary>
    /// Interaction logic for LoanHistoryWindow.xaml
    /// </summary>
    public partial class LoanHistoryWindow : Window
    {
        private string _employeeId;

        public LoanHistoryWindow(string employeeId, string employeeName)
        {
            InitializeComponent();
            _employeeId = employeeId;
            EmployeeNameText.Text = employeeName;
            LoadHistory();
        }


        private void LoadHistory()
        {
            var history = LoanHistoryService.Load()
                .Where(h => h.EmployeeId == _employeeId)
                .OrderByDescending(h => h.Date)
                .ToList();

            HistoryGrid.ItemsSource = history;

            if (history.Count == 0)
            {
                SummaryText.Text = "No loan history available.";
                return;
            }

            var latest = history.First();

            SummaryText.Text =
                $"Original Loan: {latest.OriginalLoanAmount:N2} | " +
                $"Paid: {history.Sum(h => h.PaidAmount):N2} | " +
                $"Remaining: {latest.RemainingAmount:N2}";




            decimal monthlyInstallment = latest.MonthlyInstallment;
            decimal remaining = latest.RemainingAmount;

            int remainingMonths = (int)Math.Ceiling(remaining / monthlyInstallment);

            // --- Calculate End Date ---
            DateTime startMonth = latest.Date;   // loan record date
            DateTime endDate = startMonth.AddMonths(remainingMonths);

            // --- Display text box ---
            EndDateTextBox.Text =
                $"Ends In: {remainingMonths} months ( {endDate:MMMM yyyy} )";


            // 🔥 Add this
            LoadPieChart(history);
        }


        private void LoadPieChart(List<LoanHistoryService.LoanHistoryEntry> history)
        {
            decimal totalLoan = history.First().OriginalLoanAmount;
            decimal totalPaid = history.Sum(h => h.PaidAmount);
            decimal remaining = history.First().RemainingAmount;

            LoanPieChart.Series = new SeriesCollection
    {
        new PieSeries
        {
            Title = $"Paid ({totalPaid:N2})",
            Values = new ChartValues<decimal>{ totalPaid },
            DataLabels = true,
            Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#3FA7D6"),
            LabelPoint = cp => $"{cp.Participation:P0}"
        },
        new PieSeries
        {
            Title = $"Remaining ({remaining:N2})",
            Values = new ChartValues<decimal>{ remaining },
            DataLabels = true,
            Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#F24E1E"),
            LabelPoint = cp => $"{cp.Participation:P0}"
        }
    };
        }


        private void AddManualEntry_Click(object sender, RoutedEventArgs e)
        {
            var empHistory = LoanHistoryService.Load()
                .FirstOrDefault(h => h.EmployeeId == _employeeId);

            string name = empHistory?.EmployeeName ?? "Unknown";

            var win = new AddManualLoanEntryWindow(_employeeId, name);
            win.ShowDialog();

            LoadHistory();
        }


        private void DeleteManual_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var entry = btn.DataContext as LoanHistoryService.LoanHistoryEntry;

            if (entry == null) return;

            // ❌ BLOCK SYSTEM-GENERATED ENTRIES
            if (entry.PaysheetId.StartsWith("PS-"))
            {
                CustomMessageBox.Show("System paysheet entries cannot be deleted!",
                    "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirm delete
            if (CustomMessageBox.Show("Delete this manual entry?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            // Load all entries
            var all = LoanHistoryService.Load();

            // Remove this exact entry
            all.RemoveAll(h => h.PaysheetId == entry.PaysheetId);

            // Save updated file
            LoanHistoryService.Save(all);

            // 🟢 UPDATE loan.json remaining (add back paid amount)
            RestoreLoanRemaining(entry);

            // Refresh UI
            LoadHistory();
        }


        private void RestoreLoanRemaining(LoanHistoryService.LoanHistoryEntry entry)
        {
            string json = File.ReadAllText("Savers/loan.json");
            var loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();

            var loan = loans.FirstOrDefault(l =>
                l.EmployeeId == entry.EmployeeId);

            if (loan != null)
            {
                // Add back the deleted manual payment
                loan.Remeining += entry.PaidAmount;

                // If remaining > 0 → loan should become Active again
                if (loan.Remeining > 0)
                {
                    loan.Status = "Active";
                }
                else
                {
                    // If 0 or below, force finish state
                    loan.Remeining = 0;
                    loan.Status = "Finished";
                }

                // Save
                File.WriteAllText("Savers/loan.json",
                    JsonConvert.SerializeObject(loans, Formatting.Indented));
            }
        }


        private void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            var history = LoanHistoryService.Load()
                .Where(h => h.EmployeeId == _employeeId)
                .OrderBy(h => h.Date)
                .ToList();

            if (history.Count == 0)
            {
                CustomMessageBox.Show("No loan history available to export.");
                return;
            }

            string empName = history.First().EmployeeName;

            // Convert chart to image
            var chartImage = LoanHistoryPdfService.ConvertChartToImage(LoanPieChart);

            string filePath = $"LoanHistory_{_employeeId}.pdf";

            LoanHistoryPdfService.ExportLoanHistory(
                _employeeId,
                empName,
                history,
                filePath,
                chartImage
            );

            CustomMessageBox.Show("PDF Exported Successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = filePath,
                UseShellExecute = true
            });





        }

        private string GetEmployeeEmail(string employeeId)
        {
            if (!File.Exists("Savers/employee.json"))
                return "";

            string json = File.ReadAllText("Savers/employee.json");
            var employees = JsonConvert.DeserializeObject<List<Employee>>(json)
                            ?? new List<Employee>();

            var emp = employees.FirstOrDefault(e => e.EmployeeId == employeeId);

            return emp?.EmployeeEmail?.Trim() ?? "";
        }


        public void SendLoanEmailDirect()
        {
            SendEmail_Click(this, null);
        }



        public void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            var history = LoanHistoryService.Load()
                .Where(h => h.EmployeeId == _employeeId)
                .OrderBy(h => h.Date)
                .ToList();

            if (history.Count == 0)
            {
                CustomMessageBox.Show("No loan history available to email.");
                return;
            }

            string name = history.First().EmployeeName;

            // 🔥 1. Export PDF safely (chart is optional)
            string pdfPath = $"LoanHistory_{_employeeId}.pdf";

            var chartImage = LoanHistoryPdfService.ConvertChartToImage(LoanPieChart);

            LoanHistoryPdfService.ExportLoanHistory(
                _employeeId,
                name,
                history,
                pdfPath,
                chartImage // chart included safely
            );

            // 🔥 2. Load employee email
            string employeeEmail = GetEmployeeEmail(_employeeId);

            if (string.IsNullOrWhiteSpace(employeeEmail))
            {
                CustomMessageBox.Show("Employee email not found in employee.json!",
                    "Email Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🔥 3. Send Email
            bool sent = LoanHistoryEmailService.SendLoanHistoryEmail(
                employeeEmail,
                name,
                pdfPath
            );

            if (sent)
            {
                CustomMessageBox.Show("Loan history emailed successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                CustomMessageBox.Show("Failed to send email!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }






    }

}
