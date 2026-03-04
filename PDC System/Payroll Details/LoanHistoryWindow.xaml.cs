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
                .OrderBy(h => h.Date) // Sort by date ascending first for calculation
                .ToList();

            if (history.Count == 0)
            {
                HistoryGrid.ItemsSource = history;
                SummaryText.Text = "No loan history available.";
                return;
            }

            // ✅ GET ORIGINAL LOAN AMOUNT FROM loan.json (NOT from loan history)
            decimal originalLoan = GetOriginalLoanAmountFromLoanJson(_employeeId);

            if (originalLoan == 0)
            {
                CustomMessageBox.Show("Loan record not found in loan.json!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal cumulativePaid = 0;

            foreach (var entry in history)
            {
                cumulativePaid += entry.PaidAmount;
                // ✅ Calculate remaining after each payment
                entry.RemainingAmount = Math.Max(0, originalLoan - cumulativePaid);
            }

            // Now reverse the order for display (newest first)
            var displayHistory = history.OrderByDescending(h => h.Date).ToList();
            HistoryGrid.ItemsSource = displayHistory;

            var latest = displayHistory.First();
            decimal calculatedRemaining = originalLoan - cumulativePaid;

            // ✅ Fixed: Use originalLoan and cumulativePaid directly
            SummaryText.Text =
                $"Original Loan: {originalLoan:N2} | " +
                $"Paid: {cumulativePaid:N2} | " +
                $"Remaining: {calculatedRemaining:N2}";

            decimal monthlyInstallment = latest.MonthlyInstallment;
            decimal remaining = calculatedRemaining;

            int remainingMonths = remaining > 0 && monthlyInstallment > 0
                ? (int)Math.Ceiling(remaining / monthlyInstallment)
                : 0;

            // --- Calculate End Date ---
            DateTime startMonth = latest.Date;   // loan record date
            DateTime endDate = startMonth.AddMonths(remainingMonths);

            // --- Display text box ---
            EndDateTextBox.Text =
                $"Ends In: {remainingMonths} months ( {endDate:MMMM yyyy} )";

            // ✅ NEW: Auto-update loan status based on remaining
            UpdateLoanStatus(calculatedRemaining);

            // Load Pie Chart
            LoadPieChart(displayHistory, originalLoan, cumulativePaid, calculatedRemaining);
        }

        /// <summary>
        /// ✅ ALWAYS retrieve the original loan amount from loan.json (NOT from loan history)
        /// </summary>
        private decimal GetOriginalLoanAmountFromLoanJson(string employeeId)
        {
            try
            {
                if (!File.Exists("Savers/loan.json"))
                    return 0;

                string json = File.ReadAllText("Savers/loan.json");
                var loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();

                var loan = loans.FirstOrDefault(l => l.EmployeeId == employeeId);

                return loan?.LoanAmount ?? 0;
            }
            catch
            {
                return 0;
            }
        }



        #region Window Control

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private bool _isMaximized = false;
        private double _previousLeft;
        private double _previousTop;
        private double _previousWidth;
        private double _previousHeight;

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (_isMaximized)
            {
                // Restore to previous size and position
                this.Left = _previousLeft;
                this.Top = _previousTop;
                this.Width = _previousWidth;
                this.Height = _previousHeight;
                _isMaximized = false;
            }
            else
            {
                // get before maximizing
                _previousLeft = this.Left;
                _previousTop = this.Top;
                _previousWidth = this.Width;
                _previousHeight = this.Height;

                // Get the working area (screen minus taskbar)
                var workingArea = SystemParameters.WorkArea;

                // Set window position and size to working area
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                _isMaximized = true;
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }

        #endregion



        /// <summary>
        /// ✅ NEW: Update loan status based on remaining balance
        /// </summary>
        private void UpdateLoanStatus(decimal remaining)
        {
            try
            {
                string path = "Savers/loan.json";
                if (!File.Exists(path)) return;

                string json = File.ReadAllText(path);
                var loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();

                var loan = loans.FirstOrDefault(l => l.EmployeeId == _employeeId);
                if (loan == null) return;

                // ✅ Set status based on remaining
                if (remaining <= 0)
                {
                    if (loan.Status != "Finished")
                    {
                        loan.Status = "Finished";
                        File.WriteAllText(path, JsonConvert.SerializeObject(loans, Formatting.Indented));
                    }
                }
                else
                {
                    if (loan.Status != "Active")
                    {
                        loan.Status = "Active";
                        File.WriteAllText(path, JsonConvert.SerializeObject(loans, Formatting.Indented));
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Error updating loan status: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadPieChart(List<LoanHistoryService.LoanHistoryEntry> history,
    decimal totalLoan, decimal totalPaid, decimal remaining)
        {
            var borderBrush = (Brush)Application.Current.Resources["BorderBrush"];

            LoanPieChart.Series = new SeriesCollection
{
    new PieSeries
    {
        Title = $"Paid ({totalPaid:N2})",
        Values = new ChartValues<decimal>{ totalPaid },
        DataLabels = true,
        Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#3FA7D6"),
        Stroke = borderBrush,
        StrokeThickness = 2,
        LabelPoint = cp => $"{cp.Participation:P0}"
    },
    new PieSeries
    {
        Title = $"Remaining ({remaining:N2})",
        Values = new ChartValues<decimal>{ remaining },
        DataLabels = true,
        Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#F24E1E"),
        Stroke = borderBrush,
        StrokeThickness = 2,
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

            // Refresh UI (this will auto-update status via UpdateLoanStatus)
            LoadHistory();
        }


        private void RestoreLoanRemaining(LoanHistoryService.LoanHistoryEntry entry)
        {
            string json = File.ReadAllText("Savers/loan.json");
            var loans = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();

            var loan = loans.FirstOrDefault(l =>
                l.EmployeeId == entry.EmployeeId);


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

            string filePath = $"LoanHistory_{_employeeId}.pdf";

            LoanHistoryPdfService.ExportLoanHistory(
                _employeeId,
                empName,
                history,
                filePath
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

            // 🔥 1. Export PDF
            string pdfPath = $"LoanHistory_{_employeeId}.pdf";

            LoanHistoryPdfService.ExportLoanHistory(
                _employeeId,
                name,
                history,
                pdfPath
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