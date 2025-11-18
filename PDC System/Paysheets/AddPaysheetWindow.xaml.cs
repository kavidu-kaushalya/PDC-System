using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using PDC_System.Models;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Colors = QuestPDF.Helpers.Colors;

namespace PDC_System.Paysheets
{
    /// <summary>
    /// Interaction logic for PaysheetWindow.xaml
    /// </summary>
    public partial class AddPaysheetWindow : Window
    {

        private Paysheet? existingPaysheet;   // 🔹 NEW

        private string employeeFile = "Savers/employee.json";
        private List<AttendanceRecord>? attendanceRecords;
        private List<Earning>? earnings;
        private List<Deducation>? deducations;
        private List<Loan>? loan;
        private List<ETF>? etf;
        private object saleryAmount;

        public object Employeename { get; private set; }
        public object Employeeid { get; private set; }

        public object filteredEarnings { get; private set; }
        public object TotalOvertimeAmount { get; private set; }
        public object TotalDoubleOvertimeAmount { get; private set; }
        public object PDFtotalSalery { get; private set; }
        public decimal PDFdoubeovetime { get; private set; }
        public decimal PDFtotalOvertime { get; private set; }
        public decimal PDFToalEarning { get; private set; }
        public decimal PDFEtfAmount { get; private set; }
        public decimal PDFLoanamount { get; private set; }
        public decimal PDFAbsentdateamount { get; private set; }
        public decimal PDFtotalDeducations { get; private set; }
        public int PDFworkingdays { get; private set; }
        public int PDFAbsentdays { get; private set; }
        public string PDFformattedAOT { get; private set; }
        public string PDFformattedDoubleOT { get; private set; }
        public string PDFformattedLate { get; private set; }
        public string PDFformattedLeave { get; private set; }

        public AddPaysheetWindow()
        {
            InitializeComponent();
            LoadEmployees();
            LoadAttendance();
            LoadEarnings();
            LoadDeducation();
            LoadLoan();
            LoadMonths();
            LoadETF();
            Contorls.IsEnabled = false;
            Datepickers.IsEnabled = false;
            Infomations.IsEnabled = false;
            QuestPDF.Settings.License = LicenseType.Community; // ✅ Add this line



        }


        public void LoadPaysheetData(Paysheet paysheet)
        {
            // Set Employee
            EmployeeCombo.SelectedValue = paysheet.EmployeeId;

            // Set Dates

            StartDatePicker.SelectedDate = paysheet.StartDate;
            EndDatePicker.SelectedDate = paysheet.EndDate;

            // Set Checkboxes
            Earning_Checked.IsChecked = paysheet.IncludeEarnings;
            Deducation_Checked.IsChecked = paysheet.IncludeDeductions;
            Loan_Checked.IsChecked = paysheet.IncludeLoan;
            ETF_Checked.IsChecked = paysheet.IncludeETF;

            // Set Loan Amount
            if (paysheet.IncludeLoan)
            {
                LoanAmount.Text = paysheet.LoanAmount.ToString("N2");
                LoanAmount.Visibility = Visibility.Visible;
            }

            // Set Month
            foreach (ComboBoxItem item in Month.Items)
            {
                if (item.Content.ToString() == paysheet.Month)
                {
                    Month.SelectedItem = item;
                    break;
                }
            }
            // 🔹 VERY IMPORTANT: mark this as existing paysheet
            existingPaysheet = paysheet;

            // Trigger calculation to load all data
            FilterChanged(null, null);
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

        private void LoadAttendance()
        {
            var json = File.ReadAllText("Savers/Attendance.json");
            attendanceRecords = JsonConvert.DeserializeObject<List<AttendanceRecord>>(json) ?? new List<AttendanceRecord>();
        }

        private void LoadEarnings()
        {
            var json = File.ReadAllText("Savers/Earning.json");
            earnings = JsonConvert.DeserializeObject<List<Earning>>(json) ?? new List<Earning>();
        }

        private void LoadDeducation()
        {
            var json = File.ReadAllText("Savers/Deducation.json");
            deducations = JsonConvert.DeserializeObject<List<Deducation>>(json) ?? new List<Deducation>();
        }
        private void LoadLoan()
        {
            var json = File.ReadAllText("Savers/loan.json");
            loan = JsonConvert.DeserializeObject<List<Loan>>(json) ?? new List<Loan>();
        }
        private void LoadETF()
        {
            var json = File.ReadAllText("Savers/ETF.json");
            etf = JsonConvert.DeserializeObject<List<ETF>>(json) ?? new List<ETF>();
        }


        // ✅ Helper method to reuse filtering logic
        // ✅ Helper method to reuse filtering logic
        private (List<AttendanceRecord> attendance, List<Earning> earnings, List<Deducation> deducations) GetFilteredData()
        {
            if (EmployeeCombo.SelectedValue == null)
                return (new List<AttendanceRecord>(), new List<Earning>(), new List<Deducation>());

            EmployeeCombo.SelectionChanged += EmployeeCombo_SelectionChanged;

            // Get selected employee ID safely
            string selectedId = (EmployeeCombo.SelectedValue as string) ?? string.Empty;

            // ✅ FIX: Check if dates are actually selected, don't default to DateTime.Today
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                // If dates aren't selected yet, return empty lists
                return (new List<AttendanceRecord>(), new List<Earning>(), new List<Deducation>());
            }

            // Get start and end dates from selected dates (no default to Today)
            DateTime start = StartDatePicker.SelectedDate.Value;
            DateTime end = EndDatePicker.SelectedDate.Value;

            Datepickers.IsEnabled = true;

            // 🟦 Filter attendance within the SELECTED date range
            var filteredAttendance = attendanceRecords?
                .Where(a => a.EmployeeId == selectedId &&
                            a.Date.Date >= start.Date &&
                            a.Date.Date <= end.Date)
                .ToList() ?? new List<AttendanceRecord>();

            // 🟩 Initialize filtered lists
            var filteredEarnings = new List<Earning>();
            var filteredDeducations = new List<Deducation>();

            // 🟢 Earnings filter
            if (Earning_Checked.IsChecked == true)
            {
                filteredEarnings = earnings?
                    .Where(e => e.EmployeeId == selectedId &&
                                e.EarningDate >= DateOnly.FromDateTime(start) &&
                                e.EarningDate <= DateOnly.FromDateTime(end))
                    .ToList() ?? new List<Earning>();
            }

            // 🔴 Deductions filter
            if (Deducation_Checked.IsChecked == true)
            {
                filteredDeducations = deducations?
                    .Where(d => d.EmployeeId == selectedId &&
                                d.DeducationDate >= DateOnly.FromDateTime(start) &&
                                d.DeducationDate <= DateOnly.FromDateTime(end))
                    .ToList() ?? new List<Deducation>();
            }

            // ✅ Return all three lists
            return (filteredAttendance, filteredEarnings, filteredDeducations);
        }





        private void FilterChanged(object sender, EventArgs e)
        {
            var (filtered, filteredEarnings, filteredDeducations) = GetFilteredData();

            // Employee info
            Employeename = (EmployeeCombo.SelectedItem as Employee)?.Name ?? string.Empty;
            Employeeid = (EmployeeCombo.SelectedItem as Employee)?.EmployeeId ?? string.Empty;

            Datepickers.IsEnabled = true;


            DeductionGrid.ItemsSource = filteredDeducations;

            EarningGrid.ItemsSource = filteredEarnings;

            AttendanceDataGrid.ItemsSource = filtered;


            if (Loan_Checked.IsChecked == true)
            {
                LoanAmount.Visibility = Visibility.Visible;

                var filteredLoan = loan?
                    .Where(e => e.EmployeeId == Employeeid.ToString())
                    .ToList() ?? new List<Loan>();

                decimal MonthlyPay = filteredLoan.Sum(e => e.MonthlyPay);

                // If user already edited LoanAmount manually, keep that value
                if (string.IsNullOrWhiteSpace(LoanAmount.Text))
                {
                    LoanAmount.Text = $"{MonthlyPay}";
                }
                else
                {
                    // Use the manually entered value for calculation
                    if (decimal.TryParse(LoanAmount.Text, out decimal editedValue))
                        MonthlyPay = editedValue;
                    else
                        LoanAmount.Text = $"{MonthlyPay}"; // fallback if invalid input
                }
            }
            else
            {
                LoanAmount.Visibility = Visibility.Collapsed;
                LoanAmount.Text = null;
            }


            // ETF
            // Declare ETFAmount at the beginning of the FilterChanged method
            decimal ETFAmount = 0;

            // ... (rest of your method code)

            // ETF
            if (ETF_Checked.IsChecked == true)
            {
                var filteredETF = etf?
                                   .Where(e => e.EmployeeId == Employeeid.ToString())
                                   .ToList() ?? new List<ETF>();

                ETFAmount = filteredETF.Sum(e => e.EmployeeAmount);

            }

            // Check missing records
            var missingRecords = filtered
                .Where(a =>
                    (a.CheckIn == "-" && a.CheckOut == "-") ||
                    a.Status.Contains("Missing Finger Print"))
                .Where(a =>
                    !(a.Status.Contains("Holiday") ||
                      a.Status.Contains("Poya Day") ||
                      a.Status.Contains("Non-Working Day") ||
                      a.Status.Contains("Absent")))
                .ToList();
            Contorls.IsEnabled = true;
            Infomations.IsEnabled = true;
            if (missingRecords.Any())
            {
                Infomations.IsEnabled = false;
                Contorls.IsEnabled = false;
                string dates = string.Join(", ", missingRecords.Select(a => a.Date.ToString("yyyy-MM-dd")));
                MessageBox.Show($"Cannot calculate totals. Missing attendance for these dates: {dates}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                TotalsTextBlock.Text = "";
                return;
            }



            int workingDays = filtered.Count(a => a.CheckIn != "-" && a.CheckOut != "-");
            int absentDays = filtered.Count(a =>
                (a.CheckIn == "-" && a.CheckOut == "-") &&
                !(a.Status.Contains("Holiday") || a.Status.Contains("Poya Day") || a.Status.Contains("Non-Working Day"))
            );
            double AOT = Math.Round(filtered.Sum(a => ParseHours(a.OverTime)), 2);
            double totalLate = Math.Round(filtered.Sum(a => ParseHours(a.LateHours)), 2);
            double totalDoubleOT = Math.Round(filtered.Sum(a => ParseHours(a.DoubleOT)), 2);
            double totalEarlyLeave = Math.Round(filtered.Sum(a => ParseHours(a.EarlyLeave)), 2);


            decimal totalEarnings = filteredEarnings.Sum(e => e.EarningAmount);

            double totalOT = AOT - (totalEarlyLeave + totalLate);

            #region Slection

            var Overtimeamount = (EmployeeCombo.SelectedItem as Employee)?.OvertimeAmount ?? 0m;
            var Doubleovertimeamount = (EmployeeCombo.SelectedItem as Employee)?.DoubleOvertimeAmount ?? 0m;
            var absentDayAmount = (EmployeeCombo.SelectedItem as Employee)?.AbesentAmount ?? 0m;
            var NopayDays = (EmployeeCombo.SelectedItem as Employee)?.Nopay ?? 0m;
            var saleryAmount = (EmployeeCombo.SelectedItem as Employee)?.Salary ?? 0m;

            #endregion


            #region Earning Calcualtion

            decimal TotalOvertimeAmount = (decimal)totalOT * Overtimeamount;
            decimal TotalDoubleOvertimeAmount = (decimal)totalDoubleOT * Doubleovertimeamount;
            decimal TotalEarings = Math.Round((totalEarnings + TotalOvertimeAmount + TotalDoubleOvertimeAmount), 2);

            #endregion


            #region Deducation Calcualtion

            decimal Loanamount = 0;
            decimal.TryParse(LoanAmount.Text, out Loanamount);

            absentDayAmount = (absentDays - NopayDays) * absentDayAmount;
            decimal totalDeducations = filteredDeducations.Sum(d => d.DeducationAmount);
            decimal totalofDeducations = absentDayAmount + totalDeducations + ETFAmount + Loanamount;

            #endregion

            #region Total Calcualtion

            decimal TotalOfSalary = Math.Round((saleryAmount + TotalEarings) - totalofDeducations, 2);


            #endregion

            decimal actualAbesntDate = absentDays - NopayDays;

            string formattedAOT = ConvertHoursToHM(AOT);
            string formattedDoubleOT = ConvertHoursToHM(totalDoubleOT);
            string formattedLate = ConvertHoursToHM(totalLate);
            string formattedLeave = ConvertHoursToHM(totalEarlyLeave);


            #region Text Box Assign
            eMPLOYEENAME.Text = $"{Employeename}";
            EmployeeID.Text = $"{Employeeid}";
            TotalEarnings.Text = $"{totalEarnings:N2} LKR";
            TotalDeducations.Text = $"{totalDeducations:N2} LKR";
            WorkingDays.Text = $"{workingDays}";
            DoubleOTTextBox.Text = $"{TotalDoubleOvertimeAmount:N2} LKR";
            AbsentDays.Text = $"{absentDays}";
            OTTextBox.Text = $"{TotalOvertimeAmount:N2} LKR";
            OTTextBox.Text = $"{TotalOvertimeAmount:N2} LKR";
            EmployeeSalary.Text = $"{TotalOfSalary:N2} LKR";
            #endregion

            #region PDF Variable

            PDFtotalSalery = TotalOfSalary;
            PDFdoubeovetime = TotalDoubleOvertimeAmount;
            PDFtotalOvertime = TotalOvertimeAmount;
            PDFToalEarning = TotalEarings;
            PDFEtfAmount = ETFAmount;
            PDFLoanamount = Loanamount;
            PDFAbsentdateamount = absentDayAmount;
            PDFtotalDeducations = totalofDeducations;


            PDFworkingdays = workingDays;
            PDFAbsentdays = absentDays;
            PDFformattedAOT = formattedAOT;
            PDFformattedDoubleOT = formattedDoubleOT;
            PDFformattedLate = formattedLate;
            PDFformattedLeave = formattedLeave;

            #endregion






            TotalsTextBlock.Text = $"Working Days: {workingDays} | Absent Days: {absentDays} | OT: {formattedAOT} | Double OT: {formattedDoubleOT} |Late: {formattedLate} | Early Leave: {formattedLeave}";
        }

        private void LoanAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Recalculate automatically when user edits loan amount
            if (IsLoaded && Loan_Checked.IsChecked == true)
            {
                FilterChanged(sender, e);
            }
        }

        private void EmployeeCombo_LostFocus(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null) return;

            // Get all valid names
            var validNames = (combo.ItemsSource as IEnumerable<Employee>)?.Select(x => x.Name).ToList();

            // If typed text is not a valid name, reset
            if (!validNames.Contains(combo.Text))
            {
                combo.Text = (combo.SelectedItem as Employee)?.Name ?? "";

            }




        }

        private void EmployeeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetAllFields();
        }


        private void ResetAllFields()
        {
            // Clear DataGrids
            AttendanceDataGrid.ItemsSource = null;
            EarningGrid.ItemsSource = null;
            DeductionGrid.ItemsSource = null;

            // Clear text fields
            eMPLOYEENAME.Text = string.Empty;
            EmployeeID.Text = string.Empty;
            TotalEarnings.Text = string.Empty;
            TotalDeducations.Text = string.Empty;
            WorkingDays.Text = string.Empty;
            AbsentDays.Text = string.Empty;
            OTTextBox.Text = string.Empty;
            DoubleOTTextBox.Text = string.Empty;
            EmployeeSalary.Text = string.Empty;
            LoanAmount.Text = string.Empty;
            TotalsTextBlock.Text = string.Empty;

            // Hide loan box
            LoanAmount.Visibility = Visibility.Collapsed;

            // Disable controls
            Contorls.IsEnabled = false;
            Infomations.IsEnabled = false;

            // Reset date pickers
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

            // Uncheck all checkboxes
            Earning_Checked.IsChecked = false;
            Deducation_Checked.IsChecked = false;
            Loan_Checked.IsChecked = false;
            ETF_Checked.IsChecked = false;

            // Reset Month
            Month.SelectedIndex = -1;
        }



        private string ConvertHoursToHM(double hours)
        {
            int h = (int)Math.Floor(hours);
            int m = (int)Math.Round((hours - h) * 60);
            return $"{h}h {m}m";
        }



        private double ParseHours(string timeStr)
        {
            if (string.IsNullOrWhiteSpace(timeStr))
                return 0;

            double totalHours = 0;

            // Example: "1h 30m" or "0h 15m"
            var parts = timeStr.Split(' ');
            foreach (var part in parts)
            {
                if (part.EndsWith("h"))
                {
                    if (double.TryParse(part.TrimEnd('h'), out double hours))
                        totalHours += hours;
                }
                else if (part.EndsWith("m"))
                {
                    if (double.TryParse(part.TrimEnd('m'), out double minutes))
                        totalHours += minutes / 60.0; // convert minutes to hours
                }
            }

            return totalHours;
        }


        private void ApplyPaysheet()
        {
            // Load existing paysheets
            List<Paysheet> paysheets = new List<Paysheet>();
            if (File.Exists("Savers/Paysheets.json"))
            {
                string json = File.ReadAllText("Savers/Paysheets.json");
                paysheets = JsonConvert.DeserializeObject<List<Paysheet>>(json) ?? new List<Paysheet>();
            }

            // Remove all text except numbers and decimal point
            string cleanSalary = new string(EmployeeSalary.Text.Where(c => char.IsDigit(c) || c == '.').ToArray());
            decimal salaryValue = 0;
            decimal.TryParse(cleanSalary, out salaryValue);

            // Parse loan amount
            decimal loanAmount = 0;
            decimal.TryParse(LoanAmount.Text, out loanAmount);

            var newPaysheet = new Paysheet
            {
                EmployeeId = Employeeid.ToString(),
                EmployeeName = Employeename.ToString(),
                Salary = salaryValue,
                TotalEarnings = PDFToalEarning,
                TotalDeductions = PDFtotalDeducations,
                WorkingDays = PDFworkingdays,
                AbsentDays = PDFAbsentdays,
                Month = Month.Text,

                StartDate = StartDatePicker.SelectedDate ?? DateTime.Today,
                EndDate = EndDatePicker.SelectedDate ?? DateTime.Today,
                LoanAmount = loanAmount,
                ETFAmount = PDFEtfAmount,
                IncludeEarnings = Earning_Checked.IsChecked ?? false,
                IncludeDeductions = Deducation_Checked.IsChecked ?? false,
                IncludeLoan = Loan_Checked.IsChecked ?? false,
                IncludeETF = ETF_Checked.IsChecked ?? false
            };

            // 🔹 NEW: if this is a NEW paysheet (no existingPaysheet)
            if (existingPaysheet == null)
            {
                // Check duplicate: same employee + same month already exist da balanawa
                bool alreadyExists = paysheets.Any(p =>
                    p.EmployeeId == newPaysheet.EmployeeId &&
                    p.Month == newPaysheet.Month);

                if (alreadyExists)
                {
                    MessageBox.Show(
                        $"Paysheet for {newPaysheet.Month} already exists for this employee!",
                        "Duplicate Paysheet",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return; // ❌ stop here, no save
                }

                // New ID
                newPaysheet.PaysheetId = GeneratePaysheetId(paysheets);
                paysheets.Add(newPaysheet);
            }
            else
            {
                // 🔹 EDIT MODE – keep same ID, update record
                newPaysheet.PaysheetId = existingPaysheet.PaysheetId;

                var index = paysheets.FindIndex(p => p.PaysheetId == existingPaysheet.PaysheetId);
                if (index >= 0)
                    paysheets[index] = newPaysheet;
                else
                    paysheets.Add(newPaysheet); // fallback, shouldn't normally happen
            }

            File.WriteAllText("Savers/Paysheets.json", JsonConvert.SerializeObject(paysheets, Formatting.Indented));

            MessageBox.Show("Paysheet saved successfully ✅");

            this.Close();
        }


        private void SavePaysheet_Click(object sender, RoutedEventArgs e)
        {
            ApplyPaysheet();
        }








        public async void SavePDFPaysheet_Click(object sender, RoutedEventArgs e)
        {


            var (filtered, filteredEarnings, filteredDeducations) = GetFilteredData();





            // Create folder if not exists
            string folderPath = "PaysheetFile";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Create auto file name
            string fileName = $"Paysheet_{Employeeid}_{Month.Text}_{DateTime.Now:yyyyMMddHHmm}.pdf";
            string fullPath = System.IO.Path.Combine(folderPath, fileName);


            // Run PDF generation in a background thread
            await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30); // reduced from 50
                        page.Size(PageSizes.A4);

                        page.Header()
                            .Element(ComposeHeader);

                        void ComposeHeader(IContainer container)
                        {
                            container.Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item()
                                        .Text("Payslip")
                                        .FontSize(22).SemiBold().FontColor(Colors.Blue.Medium); // reduced font size

                                    column.Item().Text(text =>
                                    {
                                        text.Span("Issue date: ").SemiBold();
                                        text.Span(DateTime.Now.ToString("yyyy-MM-dd"));
                                    });

                                    column.Item().Text(text =>
                                    {
                                        text.Span("Month: ").SemiBold();
                                        text.Span(DateTime.Now.ToString("MMMM yyyy"));
                                    });
                                });

                                row.ConstantItem(80).Height(40).Placeholder();
                            });
                        }

                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Spacing(8);

                                // Employee and Salary Info Section
                                column.Item().Row(row =>
                                {
                                    // Left side column (Employee details)
                                    row.RelativeItem().Column(left =>
                                    {
                                        left.Spacing(3);
                                        left.Item().Text("Employee Information").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium);
                                        left.Item().Container().Height(1).Background(Colors.Grey.Lighten2);
                                        left.Item().PaddingTop(3).Text($"Employee Name: {Employeename}");
                                        left.Item().Text($"Employee ID: {Employeeid}");
                                        left.Item().Text("Designation: Software Engineer");

                                    });

                                    row.Spacing(20);

                                    // Right side column (Salary details)
                                    row.RelativeItem().Column(right =>
                                    {
                                        right.Spacing(3);
                                        right.Item().Text("Salary Information").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium);
                                        right.Item().Container().Height(1).Background(Colors.Grey.Lighten2);
                                        right.Item().PaddingTop(3).Text($"Basic Salary: {saleryAmount} LKR");
                                        right.Item().Text($"Working Days: {PDFworkingdays}");
                                        right.Item().Text($"Absent Days: {PDFAbsentdays}");

                                    });
                                });

                                column.Item().PaddingVertical(5);

                                // Earnings Table
                                column.Item().Text("Earnings").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Description").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Amount (LKR.)").Bold();
                                    });

                                    // Data Rows
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Basic Salary");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{saleryAmount} LKR");

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Overtime");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{PDFtotalOvertime:N2} LKR");

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Double Overtime");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{PDFdoubeovetime:N2} LKR");

                                    // Dynamic Earnings
                                    foreach (var e in filteredEarnings)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text($"{e.EarningDescription} ({e.EarningDate:yyyy-MM-dd})");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                            .Text($"{e.EarningAmount:N2} LKR");
                                    }
                                });

                                column.Item().PaddingVertical(5);

                                // Deductions Table
                                column.Item().Text("Deductions").FontSize(12).SemiBold().FontColor(Colors.Blue.Medium);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Description").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Amount (LKR.)").Bold();
                                    });

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Absent");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{PDFAbsentdateamount:N2} LKR");

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("ETF");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{PDFEtfAmount:N2} LKR");

                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Loan");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{PDFLoanamount:N2} LKR");

                                    foreach (var e in filteredDeducations)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text($"{e.DeducationDescription} ({e.DeducationDate:yyyy-MM-dd})");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                                            .Text($"{e.DeducationAmount:N2} LKR");
                                    }
                                });

                                column.Item().PaddingVertical(8);

                                // Summary Section
                                column.Item().BorderTop(1).BorderColor(Colors.Blue.Medium).PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text("");
                                    row.ConstantItem(250).Column(summary =>
                                    {
                                        summary.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text("Total Earnings:").SemiBold();
                                            r.ConstantItem(120).AlignRight().Text($"{PDFToalEarning:N2} LKR");
                                        });
                                        summary.Item().Row(r =>
                                        {
                                            r.RelativeItem().Text("Total Deductions:").SemiBold();
                                            r.ConstantItem(120).AlignRight().Text($"{PDFtotalDeducations:N2} LKR");
                                        });
                                        summary.Item().PaddingTop(5).Row(r =>
                                        {
                                            r.RelativeItem().Text("Net Salary:").FontSize(13).Bold().FontColor(Colors.Blue.Medium);
                                            r.ConstantItem(120).AlignRight().Text($"{PDFtotalSalery:N2} LKR").FontSize(13).Bold().FontColor(Colors.Blue.Medium);
                                        });
                                    });
                                });
                            });



                        page.Footer()
                                    .AlignCenter()
                                    .Text(txt =>
                                    {
                                        txt.Span("Generated on ");
                                        txt.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                                    });
                    });




                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(20);

                        page.Content()
                            .Column(column =>
                            {
                                column.Spacing(10);



                                // Employee info
                                column.Item().Text($"Employee Name: {Employeename}");
                                column.Item().Text($"Employee ID: {Employeeid}");
                                column.Item().Text($"Working Days: {PDFworkingdays}  |  Absent Days: {PDFAbsentdays}  |  OT: {PDFformattedAOT}  |  Double OT: {PDFformattedDoubleOT}  |  Late: {PDFformattedLate}  |  Early Leave: {PDFformattedLeave}");

                                // Table
                                column.Item().Table(table =>
                                {
                                    // Columns definition
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(50);  // ID
                                        columns.RelativeColumn();    // Name
                                        columns.RelativeColumn();    // Date
                                        columns.RelativeColumn();    // Check In
                                        columns.RelativeColumn();    // Check Out
                                        columns.RelativeColumn();    // Over Time
                                        columns.RelativeColumn();    // Double OT
                                        columns.RelativeColumn();    // Early Leave
                                        columns.RelativeColumn();    // Status
                                    });

                                    // Header row
                                    table.Header(header =>
                                    {
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("ID").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Name").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Date").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Check In").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Check Out").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Over Time").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Double OT").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Early Leave").SemiBold();
                                        header.Cell().BorderBottom(3).BorderColor(Colors.Grey.Lighten1).Padding(8).Text("Status").SemiBold();
                                    });

                                    // Data rows
                                    foreach (var a in filtered)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.EmployeeId);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.Name);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.Date.ToShortDateString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.CheckIn);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.CheckOut);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.OverTime);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.DoubleOT);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.EarlyLeave);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(a.Status);
                                    }
                                });
                            });

                        // Footer with automatic page numbers
                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });

                    });

                });




                // Live Preview (Make sure Companion app is running)
                document.GeneratePdf(fullPath);
            });

            // Load existing paysheets
            List<Paysheet> paysheets = new List<Paysheet>();
            if (File.Exists("Savers/Paysheets.json"))
            {
                string json = File.ReadAllText("Savers/Paysheets.json");
                paysheets = JsonConvert.DeserializeObject<List<Paysheet>>(json) ?? new List<Paysheet>();
            }

            // Remove all text except numbers and decimal point
            string cleanSalary = new string(EmployeeSalary.Text.Where(c => char.IsDigit(c) || c == '.').ToArray());
            decimal salaryValue = 0;
            decimal.TryParse(cleanSalary, out salaryValue);

            // Parse loan amount
            decimal loanAmount = 0;
            decimal.TryParse(LoanAmount.Text, out loanAmount);

            var newPaysheet = new Paysheet
            {
                EmployeeId = Employeeid.ToString(),
                EmployeeName = Employeename.ToString(),
                Salary = salaryValue,
                TotalEarnings = PDFToalEarning,
                TotalDeductions = PDFtotalDeducations,
                WorkingDays = PDFworkingdays,
                AbsentDays = PDFAbsentdays,
                Month = Month.Text,

                StartDate = StartDatePicker.SelectedDate ?? DateTime.Today,
                EndDate = EndDatePicker.SelectedDate ?? DateTime.Today,
                LoanAmount = loanAmount,
                ETFAmount = PDFEtfAmount,
                IncludeEarnings = Earning_Checked.IsChecked ?? false,
                IncludeDeductions = Deducation_Checked.IsChecked ?? false,
                IncludeLoan = Loan_Checked.IsChecked ?? false,
                IncludeETF = ETF_Checked.IsChecked ?? false,
                PDFPath = fullPath
            };


            if (existingPaysheet == null)
            {
                bool alreadyExists = paysheets.Any(p =>
                    p.EmployeeId == newPaysheet.EmployeeId &&
                    p.Month == newPaysheet.Month);

                if (alreadyExists)
                {
                    MessageBox.Show(
                        $"Paysheet for {newPaysheet.Month} already exists for this employee!",
                        "Duplicate Paysheet",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                newPaysheet.PaysheetId = GeneratePaysheetId(paysheets);
                paysheets.Add(newPaysheet);
            }
            else
            {
                newPaysheet.PaysheetId = existingPaysheet.PaysheetId;

                var index = paysheets.FindIndex(p => p.PaysheetId == existingPaysheet.PaysheetId);
                if (index >= 0)
                    paysheets[index] = newPaysheet;
                else
                    paysheets.Add(newPaysheet);
            }




            // Remove existing paysheet for same employee & month (edit)
            paysheets.RemoveAll(p => p.EmployeeId == newPaysheet.EmployeeId && p.Month == newPaysheet.Month);

            paysheets.Add(newPaysheet);

            File.WriteAllText("Savers/Paysheets.json", JsonConvert.SerializeObject(paysheets, Formatting.Indented));

            MessageBox.Show("Paysheet saved successfully ✅");

            this.Close();

            MessageBox.Show($"PDF Saved Successfully!\n\nLocation: {fullPath}",
        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void EndDatePicker_SelectedDateChanged(object sender, ModernCalendarLib.SelectedDateChangedEventArgs e)
        {

        }

        private void EarningGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Month_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private string GeneratePaysheetId(List<Paysheet> paysheets)
        {
            if (paysheets == null || paysheets.Count == 0)
                return "PS-0001";

            // last paysheet eka ganna (oya JSON walata save wela thiyena list eken)
            var last = paysheets
                .Where(p => !string.IsNullOrEmpty(p.PaysheetId))
                .OrderBy(p => p.PaysheetId)
                .LastOrDefault();

            if (last == null || string.IsNullOrEmpty(last.PaysheetId))
                return "PS-0001";

            // PS-0005 -> 5
            var parts = last.PaysheetId.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int num))
                return "PS-0001";

            num++;
            return $"PS-{num:0000}";
        }



        private void LoadMonths()
        {
            string[] months = new string[]
            {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
            };

            foreach (string month in months)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = month;
                Month.Items.Add(item);
            }
        }

      
    }

    public class Paysheet
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal Salary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public int WorkingDays { get; set; }
        public int AbsentDays { get; set; }
        public string Month { get; set; }

        // NEW FIELDS - Add these to save complete paysheet state
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal ETFAmount { get; set; }
        public bool IncludeEarnings { get; set; }
        public bool IncludeDeductions { get; set; }
        public bool IncludeLoan { get; set; }
        public bool IncludeETF { get; set; }

        public string PDFPath { get; set; }

        public string PaysheetId { get; set; }

    }


}
